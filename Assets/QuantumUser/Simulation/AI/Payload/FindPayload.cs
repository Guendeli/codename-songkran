using Photon.Deterministic;

namespace Quantum
{
    [System.Serializable]
    public unsafe class FindPayload : AIAction
    {
        public AIBlackboardValueKey ObjectivePoint;

        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            AIBlackboardComponent* blackboard = frame.Unsafe.GetPointer<AIBlackboardComponent>(entity);

            FPVector2 myPosition = frame.Get<Transform2D>(entity).Position;

            var payloadEntity = frame.Global->PayloadEntity;
            if (!payloadEntity.IsValid)
            {
                return;
            }
            Transform2D* payloadTransform = frame.Unsafe.GetPointer<Transform2D>(payloadEntity);

            if (payloadEntity == default)
                return;
            
            
            blackboard->Set(frame, ObjectivePoint.Key, payloadTransform->Position);
        }
    }
}