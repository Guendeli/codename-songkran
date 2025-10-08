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
        characterAttacks->BasicSkillData);

      ApplyAttackInput(frame,
        input,
        input.AltFire,
        entity,
        entityPos,
        movementData,
        characterAttacks->SpecialSkillData);
    }

    private void ApplyAttackInput(Frame frame, QuantumDemoInputTopDown input, Button button, EntityRef entity, FPVector2 entityPos, MovementData* movementData, AssetRef<SkillData> dataRef)
    {
      bool actionReleased = button.WasReleased;

      if (actionReleased == true && movementData->IsOnAttackLock == false)
      {
        SkillData data = frame.FindAsset<SkillData>(dataRef.Id);
        EAttributeType costType = data.CostType;
        
        FP energyAttribute = AttributesHelper.GetCurrentValue(frame, entity, costType);
        if (energyAttribute >= data.Cost)
        {
          movementData->DirectionTimer = data.RotationLockDuration;
          frame.Signals.OnCreateSkill(entity, entityPos, data, input.AimDirection);
          movementData->MovementTimer = data.MovementLockDuration;
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
