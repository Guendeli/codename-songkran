using System;
using System.Collections.Generic;
using Quantum.Collections;
using Quantum.Physics2D;

namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PayloadMoveSystem : SystemMainThreadFilter<PayloadMoveSystem.Filter>, ISignalOnComponentAdded<Payload>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            Transform2D* payloadTransform = filter.Transform;
            PayloadData data = frame.FindAsset<PayloadData>(filter.Payload->PayloadData.Id);

            var layerMask = frame.Layers.GetLayerMask(AIConstants.LAYER_CHARACTER);
            HitCollection hits = PhysicsHelper.OverlapShape(frame, payloadTransform, layerMask, data.Shape);
            if (hits.Count == 0)
            {
                return;
            }
            
            Int32 objectiveIndex = GetObjectiveInfo(frame, hits);

            // Team Conflict within the payload area, return
            if (objectiveIndex == -1)
                return;
            
            var allObjectivePoints = frame.Filter<PayloadObjectivePoint, Transform2D>();
            FPVector2 destination = default;
            while (allObjectivePoints.NextUnsafe(out var objectivePointEntity, out var objectivePoint, out var transform))
            {
                if (objectivePoint->TeamId == objectiveIndex)
                {
                    destination = transform->Position;
                    break;
                }
            }

            if (destination != default && FPVector2.DistanceSquared(destination, payloadTransform->Position) > FP.Epsilon)
            {
                FPVector2 pos = payloadTransform->Position;
                FPVector2 dir = destination - pos;
                
                FPVector2 newPos = pos + dir.Normalized * data.MoveSpeed * frame.DeltaTime;
                
                payloadTransform->Position = newPos;
            }
        }
        
        private Int32 GetObjectiveInfo(Frame frame, HitCollection hits)
        {
            Int32 objectiveIndex = -1;
            for (var i = 0; i < hits.Count; i++)
            {
                var target = hits[i].Entity;
                if (frame.Exists(target) == true && frame.TryGet(target, out TeamInfo teamInfo) == true)
                {
                    if (objectiveIndex == -1)
                    {
                        objectiveIndex = teamInfo.Index;
                    }
                    else
                    {
                        if (objectiveIndex != teamInfo.Index)
                        {
                            // Team Conflict within the payload area, return
                            return -1;
                        }
                    }

                }
            }

            return objectiveIndex;
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Payload* Payload;
            public Transform2D* Transform;
        }

        public void OnAdded(Frame f, EntityRef entity, Payload* component)
        {
            // Setup physics2D range based on payload settings
        }
    }
}
