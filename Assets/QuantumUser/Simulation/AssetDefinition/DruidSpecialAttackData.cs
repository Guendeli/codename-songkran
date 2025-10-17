namespace Quantum
{
    /// <summary>
    /// This is an AoE attack data that follows the source
    /// </summary>
    public class DruidSpecialAttackData : AreaOfEffectAttackData
    {
        public override unsafe void OnUpdate(Frame frame, EntityRef attackEntity, Attack* attack)
        {
            base.OnUpdate(frame, attackEntity, attack);
            EntityRef sourceEntity = attack->Source;
            Transform2D* sourceTransform = frame.Unsafe.GetPointer<Transform2D>(sourceEntity);
            Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(attackEntity);
            
            attackTransform->Position = sourceTransform->Position;
            
            UpdateEffectInterval(frame, attackEntity, attack);
        }

        protected override unsafe void UpdateEffectInterval(Frame frame, EntityRef attackEntity, Attack* attack)
        {
            DruidSpecialAttackRD* attackRuntimeData = attack->AttackRuntimeData.DruidSpecialAttackRD;
            attackRuntimeData->EffectInterval -= frame.DeltaTime;

            if (attackRuntimeData->EffectInterval <= 0)
            {
                attackRuntimeData->EffectInterval = EffectInterval;
                Execute(frame, attackEntity, *attack);
            }
        }
    }
}