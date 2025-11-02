using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
  [Preserve]
  public unsafe class CharacterSkillCreationSystem : SystemMainThreadFilter<CharacterSkillCreationSystem.Filter>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public InputContainer* InputContainer;
      public Transform2D* Transform;
      public MovementData* MovementData;
      public CharacterAttacks* CharacterAttacks;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      var input = filter.InputContainer->Input;
      var entity = filter.Entity;
      var entityPos = filter.Transform->Position;
      var movementData = filter.MovementData;
      var characterAttacks = filter.CharacterAttacks;

      // Stun - Silence checks here
      FP stun = AttributesHelper.GetCurrentValue(frame, filter.Entity, EAttributeType.Stun);
      if (stun > 0)
      {
        return;
      }
      
      // Skill Logic
      ApplyAttackInput(frame,
        input,
        input.Fire,
        entity,
        entityPos,
        movementData,
        characterAttacks->BasicSkillData, filter);

      ApplyAttackInput(frame,
        input,
        input.AltFire,
        entity,
        entityPos,
        movementData,
        characterAttacks->SpecialSkillData, filter);
    }

    private void ApplyAttackInput(Frame frame, QuantumDemoInputTopDown input, Button button, EntityRef entity, FPVector2 entityPos, MovementData* movementData, AssetRef<SkillData> dataRef, Filter filter)
    {
      SkillData data = frame.FindAsset<SkillData>(dataRef.Id);
      if(filter.InputContainer->TimeSinceAutoAim > 0)
        filter.InputContainer->TimeSinceAutoAim -= frame.DeltaTime;
      bool canAutoAim = CharacterHelpers.CanAutoAim(frame, entity, data, filter.InputContainer);
      bool shouldCast = EnemyPositionsHelper.TryGetClosestCharacterDirection(frame, filter.Entity, *filter.Transform, data.AutoAimRadius, true, data.AutoAttack, out var direction);
      
      bool actionReleased = button.WasReleased || (shouldCast && canAutoAim);

      if (actionReleased == true && movementData->IsOnAttackLock == false)
      {
        EAttributeType costType = data.CostType;
        
        FP energyAttribute = AttributesHelper.GetCurrentValue(frame, entity, costType);
        if (energyAttribute >= data.Cost)
        {
          FPVector2 aimDirection = canAutoAim ? movementData->LastAutoAimDirection : input.AimDirection;
          movementData->DirectionTimer = data.RotationLockDuration;
          frame.Signals.OnCreateSkill(entity, entityPos, data, aimDirection);
          movementData->MovementTimer = data.MovementLockDuration;
          if (canAutoAim)
          {
            filter.InputContainer->TimeSinceAutoAim = data.AutoAimInterval;
          }
        }
      }

      if (movementData->IsOnAttackLock == true)
      {
        movementData->DirectionTimer -= frame.DeltaTime;
      }

      if (movementData->IsOnAttackMovementLock == true)
      {
        movementData->MovementTimer -= frame.DeltaTime;
      }
    }
  }
}
