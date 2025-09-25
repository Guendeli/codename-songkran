using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Notes for refactor:
    /// Need an abstract class that applies SelfEffects on OnCreate and TargetEffects on Update
    /// Let's do this quick n dirty now to show something on monday.
    ///
    /// Attack Design:
    /// - On Cast, apply a buff to speed and switch on ForcedMovement
    /// - On Update do a checkhit using the source transform and applies effects on others on hit (a stun for ex)
    /// </summary>
    public unsafe class DashAttackData : AttackData
    {
        public Shape2DConfig Shape;
        
        public override void OnCreate(Frame frame, EntityRef attackEntity, EntityRef source, Attack* attack)
        {
            base.OnCreate(frame, attackEntity, source, attack);
            OnApplyEffectSource(frame, source);
            SetForcedMovement(frame,source,true);
        }

        public override void OnUpdate(Frame frame, EntityRef attackEntity, Attack* attack)
        {
            base.OnUpdate(frame, attackEntity, attack);
            EntityRef sourceEntity = frame.Unsafe.GetPointer<Attack>(attackEntity)->Source;

            EntityRef targetEntityRef = CheckHit(frame, sourceEntity, attack, out bool wasDisabled);

            if(wasDisabled == true)
            {
                return;
            }
            
            if (targetEntityRef != default)
            {
                OnApplyEffectTarget(frame, attack->Source, targetEntityRef);
            }
            
        }

        public override void OnDeactivate(Frame frame, EntityRef attackEntity)
        {
            EntityRef sourceEntity = frame.Unsafe.GetPointer<Attack>(attackEntity)->Source;
            SetForcedMovement(frame,sourceEntity,false);

            base.OnDeactivate(frame, attackEntity);
        }
        
        private EntityRef CheckHit(Frame frame, EntityRef attackEntity, Attack* attack, out bool wasDisabled)
        {
            wasDisabled = false;
            
            Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(attackEntity);
            
            var layerMask = frame.Layers.GetLayerMask(AIConstants.LAYER_STATIC, AIConstants.LAYER_CHARACTER);
            var hits = PhysicsHelper.OverlapShape(frame, attackTransform, layerMask, Shape);
            if (hits.Count == 0)
            {
                return default;
            }
            return CheckHits(frame, hits, attackEntity, attack, attackTransform, out wasDisabled);
        }
        
        private void SetForcedMovement(Frame frame, EntityRef entity, bool value)
        {
            MovementData* movement = frame.Unsafe.GetPointer<MovementData>(entity);
            movement->IsForcedMovement = value;
        }
    }
}