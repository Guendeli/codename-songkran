using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
  [Preserve]
  public unsafe class RotationSystem : SystemMainThreadFilter<RotationSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public InputContainer* InputContainer;
      public MovementData* MovementData;
      public Transform2D* Transform;
      public CharacterAttacks* CharacterAttacks;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      if (filter.MovementData->IsOnAttackLock == false)
      {
        FPVector2 moveDirection = filter.InputContainer->Input.MoveDirection.Normalized;
        SkillData skillData = frame.FindAsset<SkillData>(filter.CharacterAttacks->BasicSkillData.Id);
        bool canAutoAim = CharacterHelpers.CanAutoAim(frame, filter.Entity, skillData, filter.InputContainer);

        if (moveDirection != default)
        {
          filter.Transform->Rotation = FPVector2.RadiansSignedSkipNormalize(FPVector2.Up, moveDirection);
        }

        if (filter.InputContainer->Input.Fire.WasReleased == true || filter.InputContainer->Input.AltFire.WasReleased == true)
        {

          if (filter.InputContainer->Input.AimDirection.Magnitude >= FP._0_10) // FIXED POINT DEAD ZONE
          {
            filter.MovementData->LastAutoAimDirection = filter.InputContainer->Input.AimDirection.Normalized;
          }
          
          filter.Transform->Rotation = FPVector2.RadiansSignedSkipNormalize(FPVector2.Up, filter.MovementData->LastAutoAimDirection);
        }
        
        if (canAutoAim)
        {
            bool foundTarget = EnemyPositionsHelper.TryGetClosestCharacterDirectionRaw(frame, filter.Entity, *filter.Transform, skillData.AutoAimRadius, true, skillData.AutoAttack, out var direction);
            if (foundTarget == true)
            {
              if (direction == FPVector2.Zero)
              {
                direction = filter.Transform->Up;
              }
              filter.MovementData->LastAutoAimDirection = direction;
              filter.Transform->Rotation = FPVector2.RadiansSignedSkipNormalize(FPVector2.Up, filter.MovementData->LastAutoAimDirection.Normalized);
            }
        }
      }
    }
  }
}
