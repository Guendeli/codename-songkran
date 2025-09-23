using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    [CreateAssetMenu(fileName = "HitScanCosmeticData", menuName = "Attack Cosmetic Data/HitscanAttackCosmeticData", order = 1)]
    public class HitscanAttackCosmeticData : BaseAttackCosmeticData
    {
        #if QUANTUM_UNITY
        
        public FP HeightOffset = FP._1;
        
        #endif
    }
}