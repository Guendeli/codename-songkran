namespace TwinStickShooter
{
  using Quantum;
  using UnityEngine;

  public class CharacterAnimations : QuantumEntityViewComponent<CustomViewContext>
  {
    private Animator _animator;

    private int _speedBuffer = 0;

    private void Awake()
    {
      _animator = GetComponentInChildren<Animator>();
    }

    public override void OnActivate(Frame frame)
    {
      base.OnActivate(frame);
      QuantumEvent.Subscribe<EventCharacterSkill>(this, OnCharacterSkill);
      QuantumEvent.Subscribe<EventGameOver>(this, OnGameOver);
    }

    private void OnCharacterSkill(EventCharacterSkill ev)
    {
      if (ev.character == EntityRef)
      {
        _animator.SetTrigger("Attack");
      }
    }

    private void OnGameOver(EventGameOver ev)
    {
      bool isWinner = ev.Game.Frames.Verified.Get<TeamInfo>(EntityRef).Index == ev.WinnerTeam;

      if (isWinner == true)
      {
        _animator.SetTrigger("Cheer");
      }
      else
      {
        _animator.SetTrigger("Defeated");
      }
    }

    public override void OnUpdateView()
    {
      if (PredictedFrame.TryGet<KCC>(EntityRef, out KCC kcc))
      {
        float speed = kcc.Velocity.Magnitude.AsFloat;

        if (speed == 0)
        {
          _speedBuffer++;
          if (_speedBuffer > 1)
          {
            _animator.SetBool("StopRun", true);
            _animator.SetFloat("Speed", 0);
          }
          else
          {
            _animator.SetFloat("Speed", 10);
          }
        }
        else
        {
          _speedBuffer = 0;
          _animator.SetBool("StopRun", false);
          _animator.SetFloat("Speed", speed);
        }
      }
    }
  }
}