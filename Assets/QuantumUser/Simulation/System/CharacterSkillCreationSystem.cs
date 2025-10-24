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
      
      bool actionReleased = button.WasReleased || CanAutoAim(frame, entity, data, filter.InputContainer);

      if (actionReleased == true && movementData->IsOnAttackLock == false)
      {
        EAttributeType costType = data.CostType;
        
        FP energyAttribute = AttributesHelper.GetCurrentValue(frame, entity, costType);
        if (energyAttribute >= data.Cost)
        {
          FPVector2 aimDirection = input.AimDirection;
          if (CanAutoAim(frame, entity, data, filter.InputContainer))
          {
            EntityRef closestEnemy = default;
            EnemyPositionsHelper.TryGetClosestCharacter(frame, entity,
              data.AutoAimRadius, checkLineSight: true, ignoreSameTeam: true, out closestEnemy);
            if (closestEnemy == default)
              return;
            
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(closestEnemy);
            
            aimDirection = (targetTransform->Position - entityPos);
            Log.Debug(string.Format("Casting auto-aim towards {0} at direction {1}", closestEnemy.Index, aimDirection));
          }
          
          
          movementData->DirectionTimer = data.RotationLockDuration;
          frame.Signals.OnCreateSkill(entity, entityPos, data, aimDirection);
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

    private bool CanAutoAim(Frame frame, EntityRef source, SkillData skillData, InputContainer* inputContainer)
    {
      Bot* bot = frame.Unsafe.GetPointer<Bot>(source);
      if (bot->IsActive)
        return false;

      if (skillData.AutoAimCheckSight && inputContainer->TimeSinceAutoAim <= 0)
      {
        return true;
      }
      
      return false;
    }
    private bool GetClosestEnemyDirection(Frame frame, EntityRef source, FPVector2 sourcePos, FP radius)
    {
      // TeamInfo sourceTeamInfo = frame.Get<TeamInfo>(source);
      // Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(source);
      //
      // var layerMask = frame.Layers.GetLayerMask(AIConstants.LAYER_CHARACTER);
      // var hits = PhysicsHelper.OverlapShape(frame, attackTransform, layerMask, );
      // for (int i = 0; i < hits.Count; i++)
      // {
      //   EntityRef target = hits[i].Entity;
      //   TeamInfo* teamInfo = frame.Unsafe.GetPointer<TeamInfo>(target);
      //
      //   if (HitAlies == false && sourceTeamInfo.Index == teamInfo->Index)
      //   {
      //     continue;
      //   }
      //
      //   if (IgnoreOwner == true && target.Equals(attack.Source) == true)
      //   {
      //     continue;
      //   }
      //   OnApplyEffectAll(frame, attack.Source, target);

      return false;
    }
  }
}
