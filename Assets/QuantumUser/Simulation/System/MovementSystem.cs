using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
  [Preserve]
  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnComponentAdded<KCC>
  {
    public struct Filter
    {
      public EntityRef Entity;
      public InputContainer* InputContainer;
      public KCC* KCC;
      public MovementData* MovementData;
    }

    public void OnAdded(Frame frame, EntityRef entity, KCC* component)
    {
      KCCSettings kccSettings = frame.FindAsset<KCCSettings>(component->Settings.Id);
      kccSettings.Init(ref *component);
    }

    public override void Update(Frame frame, ref Filter filter)
    {
      FP stun = AttributesHelper.GetCurrentValue(frame, filter.Entity, EAttributeType.Stun);
      if (stun > 0 || filter.MovementData->IsOnAttackMovementLock == true)
      {
        return;
      }

      FP characterSpeed = AttributesHelper.GetCurrentValue(frame, filter.Entity, EAttributeType.Speed);
      filter.KCC->MaxSpeed = characterSpeed;
      
      FP acceleration = AttributesHelper.GetCurrentValue(frame, filter.Entity, EAttributeType.Acceleration);
      filter.KCC->Acceleration = acceleration;

      FPVector2 direction = filter.MovementData->IsForcedMovement ? filter.MovementData->LastAutoAimDirection :
        filter.InputContainer->Input.MoveDirection.Normalized;
      
      KCCSettings kccSettings = frame.FindAsset<KCCSettings>(filter.KCC->Settings.Id);
      KCCMovementData kccMovementData = kccSettings.ComputeRawMovement(frame,
        filter.Entity, direction);
      kccSettings.SteerAndMove(frame, filter.Entity, in kccMovementData);
      
    }
  }
}
