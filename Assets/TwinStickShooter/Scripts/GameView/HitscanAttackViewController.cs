using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace TwinStickShooter
{
    /// <summary>
    ///  Systemù that listen to On Create Hitscan Attack frame events, instantiate a view
    ///  and sets up the start / endpos
    /// </summary>
    public class HitscanAttackViewController : QuantumSceneViewComponent<CustomViewContext>
    {
        public override void OnActivate(Frame frame)
        {
            base.OnActivate(frame);
            QuantumEvent.Subscribe<EventOnCreateHitscanAttack>(this, OnHitscanAttack);
        }

        private void OnHitscanAttack(EventOnCreateHitscanAttack callback)
        {
            AttackData attackData = QuantumUnityDB.GetGlobalAsset<AttackData>(callback.skillData);
            if (attackData != null && attackData.CosmeticData != null)
            {
                if (attackData.CosmeticData.Shape != AttackShape.LineRenderer)
                    return;
                
                HitscanAttackCosmeticData cosmeticData = attackData.CosmeticData as HitscanAttackCosmeticData;
                if (cosmeticData != null)
                {
                    GameObject visualPrefab = cosmeticData.VisualPrefab;
                    FPVector2 startPosition = callback.startPosition;
                    FPVector2 endPosition = callback.endPosition;
                    FP height = cosmeticData.HeightOffset;
                    
                    SetupVisual(visualPrefab, startPosition, endPosition, height);
                    
                }
            }
        }

        private void SetupVisual(GameObject prefab, FPVector2 start, FPVector2 end, FP heightOffset)
        {
            Vector3 startPos = new Vector3(start.X.AsFloat, heightOffset.AsFloat, start.Y.AsFloat);
            Vector3 endPos = new Vector3(end.X.AsFloat, heightOffset.AsFloat, end.Y.AsFloat);
            
            GameObject vfxObj = Instantiate(prefab, startPos, Quaternion.identity);
            LineRenderer lineRenderer = vfxObj.GetComponentInChildren<LineRenderer>();
            if(lineRenderer != null)
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);
            }
            else
            {
                Debug.LogError(string.Format("Visual Prefab {0} does not have a LineRenderer component.", prefab.name));
                Destroy(vfxObj);
            }
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            QuantumEvent.UnsubscribeListener<EventOnCreateHitscanAttack>(this);
        }
    }
}