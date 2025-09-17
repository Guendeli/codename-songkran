using Photon.Deterministic;

namespace Quantum
{

	[System.Serializable]
	public struct Effect
	{
		public FP Value;
		public EAttributeType AttributeType;
		public EModifierAppliance Appliance;
		public EModifierOperation Operation;
		public FP Duration;
		public bool ApplyOnSummonParent;
	}

	public static unsafe class EffectsHelper
	{
		public static void OnApply(Frame frame, EntityRef source, EntityRef targetCharacter, Effect effect)
		{
			FP multiplier = 1;
			if (effect.AttributeType == EAttributeType.Health)
			{
				multiplier = AttributesHelper.GetCurrentValue(frame, source, EAttributeType.Power);
			}

			FP value = effect.Value * multiplier;
			if (effect.AttributeType == EAttributeType.Stun)
			{
				FP stun = AttributesHelper.GetCurrentValue(frame, targetCharacter, EAttributeType.Stun);
				value = value - stun;
			}

			if (value < 0)
			{
				return;
			}

			AttributesHelper.ChangeAttribute(frame, targetCharacter, effect.AttributeType, effect.Appliance, effect.Operation, value, effect.Duration);
		}
	}
}
