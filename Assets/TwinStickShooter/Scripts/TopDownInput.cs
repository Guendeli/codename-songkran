namespace TwinStickShooter
{
  using Photon.Deterministic;
  using Quantum;
  using UnityEngine;
  using UnityEngine.InputSystem;

  public class TopDownInput : MonoBehaviour
  {
    public FP AimSensitivity = 5;
    public CustomViewContext ViewContext;
    
    private FPVector2 _lastDirection = new FPVector2();
    private AttackPreview _attackPreview;
    private PlayerInput _playerInput;

    public bool IsInverseControl { get; set; } = false;

    private void Start()
    {
      _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
      CharacterView.OnLocalPlayerInstantiated += OnLocalPlayerInstantiated;
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void OnDisable()
    {
      CharacterView.OnLocalPlayerInstantiated -= OnLocalPlayerInstantiated;
    }


    private void OnLocalPlayerInstantiated(CharacterView playerView)
    {
      if (_attackPreview != null)
      {
        Destroy(_attackPreview.gameObject);
      }
      _attackPreview = ViewContext.LocalView.GetComponentInChildren<AttackPreview>(true);
      _attackPreview.transform.parent = null;
    }

    private void Update()
    {
      if (_attackPreview != null
#if UNITY_ANDROID
			&& _playerInput.actions["Fire"].IsPressed() == false
			&& _playerInput.actions["Special"].IsPressed() == false
#endif
#if UNITY_STANDALONE || UNITY_WEBGL
          && _playerInput.actions["MouseFire"].IsPressed() == false
          && _playerInput.actions["MouseSpecial"].IsPressed() == false
#endif
         )
      {
        _attackPreview.gameObject.SetActive(false);
      }
    }

    public void PollInput(CallbackPollInput callback)
    {
      Quantum.QuantumDemoInputTopDown input = new Quantum.QuantumDemoInputTopDown();

      FPVector2 directional = _playerInput.actions["Move"].ReadValue<Vector2>().ToFPVector2();
      input.MoveDirection = IsInverseControl == true ? -directional : directional;

#if UNITY_ANDROID
		input.Fire = _playerInput.actions["Fire"].IsPressed();
		input.AltFire = _playerInput.actions["Special"].IsPressed();
#endif
#if UNITY_STANDALONE || UNITY_WEBGL
      input.Fire = _playerInput.actions["MouseFire"].IsPressed();
      input.AltFire = _playerInput.actions["MouseSpecial"].IsPressed();
#endif

      if (input.Fire == true)
      {
        _lastDirection = _playerInput.actions["AimBasic"].ReadValue<Vector2>().ToFPVector2();
        _lastDirection *= AimSensitivity;
      }

      if (input.AltFire == true)
      {
        _lastDirection = _playerInput.actions["AimSpecial"].ReadValue<Vector2>().ToFPVector2();
        _lastDirection *= AimSensitivity;
      }

      FPVector2 actionVector = default;
#if UNITY_STANDALONE || UNITY_WEBGL
      if (_playerInput.currentControlScheme != null
          && _playerInput.currentControlScheme.Contains("Joystick"))
      {
        actionVector = IsInverseControl ? -_lastDirection : _lastDirection;
        input.AimDirection = actionVector;
      }
      else
      {
        actionVector = GetDirectionToMouse();
        input.AimDirection = actionVector;
      }

      if ((input.Fire == true || input.AltFire == true) && input.AimDirection != FPVector2.Zero)
      {
        _attackPreview.gameObject.SetActive(true);
        _attackPreview.UpdateAttackPreview(actionVector, input.AltFire);
      }

      callback.SetInput(input, DeterministicInputFlags.Repeatable);

#elif UNITY_ANDROID
		actionVector = IsInverseControl ? -_lastDirection : _lastDirection;
    input.AimDirection = actionVector;

		if ((input.Fire == true || input.AltFire == true) && actionVector != FPVector2.Zero)
		{
			_attackPreview.gameObject.SetActive(true);
			_attackPreview.UpdateAttackPreview(actionVector, input.AltFire);
		}
		callback.SetInput(input, DeterministicInputFlags.Repeatable);
#endif
    }

    private FPVector2 GetDirectionToMouse()
    {
      if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null)
        return default;

      Frame frame = QuantumRunner.Default.Game.Frames.Predicted;
      if (frame == null)
        return default;

      if (ViewContext.LocalView == null || frame.Exists(ViewContext.LocalView.EntityRef) == false)
        return default;
      

      FPVector2 localCharacterPosition = frame.Get<Transform2D>(ViewContext.LocalView.EntityRef).Position;

      Vector2 mousePosition = _playerInput.actions["Point"].ReadValue<Vector2>();
      Ray ray = Camera.main.ScreenPointToRay(mousePosition);
      UnityEngine.Plane plane = new UnityEngine.Plane(Vector3.up, Vector3.zero);

      if (plane.Raycast(ray, out var enter))
      {
        var dirToMouse = ray.GetPoint(enter).ToFPVector2() - localCharacterPosition;
        return dirToMouse;
      }

      return default;
    }
  }
}