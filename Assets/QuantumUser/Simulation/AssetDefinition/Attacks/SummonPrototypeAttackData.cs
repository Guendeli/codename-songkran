using Photon.Deterministic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quantum
{

    /// <summary>
    /// This Attack will spawn an Entity Prototype upon cast, for a TTL duration then despawn
    /// </summary>
    public unsafe partial class SummonPrototypeAttackData : AttackData
    {
        [Header("Summon Settings")] 
        public FP SummonDistance;
        public AssetRef<EntityPrototype> SpawnPrototype;

        public override void OnCreate(Frame frame, EntityRef attackEntity, EntityRef source, Attack* attack)
        {
            base.OnCreate(frame, attackEntity, source, attack);
            SummonAttackRD* attackRuntimeData = attack->AttackRuntimeData.SummonAttackRD;
            Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(attackEntity);
            FPVector2 spawnPos = attackTransform->Position + attackTransform->Up * SummonDistance;

            var hits = PhysicsHelper.HitScanCollision(frame, attackTransform, SummonDistance);
            if (hits.Count != 0)
            {
                spawnPos = hits[0].Point;
            }
            
            EntityRef summonEntity = frame.Create(SpawnPrototype);
            frame.Unsafe.GetPointer<Transform2D>(summonEntity)->Position = spawnPos;

            // Cache the Summonned Entity Reference to despawn it
            attackRuntimeData->SummonedEntity = summonEntity;
        }

        public override void OnUpdate(Frame frame, EntityRef attackEntity, Attack* attack)
        {
            base.OnUpdate(frame, attackEntity, attack);
            if (attack->TTL >= TTL)
            {
                OnDeactivate(frame, attackEntity);
            }
        }

        public override void OnDeactivate(Frame frame, EntityRef attackEntity)
        {
            Attack* attack = frame.Unsafe.GetPointer<Attack>(attackEntity);
            SummonAttackRD* attackRuntimeData = attack->AttackRuntimeData.SummonAttackRD;
            frame.Destroy(attackRuntimeData->SummonedEntity);

            base.OnDeactivate(frame, attackEntity);
        }
        
        
        
        
    }
}