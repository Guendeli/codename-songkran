using Photon.Deterministic;
using System;

namespace Quantum
{
    // Base class for any attack which shoots a simple line cast with no extra logic other than causing damage

    [System.Serializable]
    public unsafe abstract partial class HitscanAttackData : AttackData
    {
        public FP Distance;     // For LineCast attacks
        public override void OnCreate(Frame frame, EntityRef attackEntity, EntityRef source, Attack* attack)
        {
            base.OnCreate(frame, attackEntity, source, attack);
            bool wasDisabled = CheckHit(frame, attackEntity, attack);

            if(wasDisabled == true)
            {
                return;
            }
        }

        private bool CheckHit(Frame frame, EntityRef attackEntity, Attack* attack) {
            Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(attackEntity);
            var hits = PhysicsHelper.HitScanCollision(frame, attackTransform, Distance);
            if (hits.Count == 0)
            {
                frame.Events.OnCreateHitscanAttack(Guid, attackTransform->Position, 
                    attackTransform->Position + attackTransform->Up * Distance);
                return false;
            }

            EntityRef targetEntity = CheckHits(frame, hits, attackEntity, attack, attackTransform, out bool wasDisabled);
            if (targetEntity != default)
            {
                OnApplyEffect(frame, attack->Source, targetEntity);
            }
            
            
            frame.Events.OnCreateHitscanAttack(Guid, attackTransform->Position, hits[0].Point);
            
            return wasDisabled;
        }
        
    }
}