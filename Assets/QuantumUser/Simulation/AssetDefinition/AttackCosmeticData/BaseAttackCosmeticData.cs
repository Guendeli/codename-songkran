using UnityEngine;

namespace Quantum
{
    public enum AttackShape
    {
        LineRenderer,
        OverheadIcon
    }
    /// <summary>
    /// Base Class for Cosmetic Data for Attacks where Visual doesn't come from the EntityPrototype,
    /// Or needs more visual feedback than the Attack Entity Itself.
    /// describing how they should look:
    /// examples:
    /// - a hitscan is a single frame attack, but the visual should persist for more frames
    /// - need to show a world space UI or Icon on top of player
    /// - etc..
    /// </summary>
    public class BaseAttackCosmeticData : ScriptableObject
    {
        #if QUANTUM_UNITY
        
        public AttackShape Shape;
        public Sound SFX;
        public GameObject VisualPrefab;

        #endif
    }
}