/*
 * SensorTactics.cs
 *
 * Desc: The class dynamically evaluates and assigns tactics to bots based on the conditions defined in the TacticalSensor objects.
 * It ensures that tactics are only updated periodically and respects a commitment duration to avoid frequent changes.
 *
 * cf. TacticalSensor inheritors
 *
 * Author: Omar Guendeli
 */

using Photon.Deterministic;

namespace Quantum
{
	[System.Serializable]
	public unsafe partial class SensorTactics : Sensor
	{
		public AssetRef<TacticalSensor>[] TacticalSensors;

		private TacticalSensor[] _tacticalSensors;

		public override void Execute(Frame frame, EntityRef entity)
		{
			var bot = frame.Unsafe.GetPointer<Bot>(entity);
			if (bot->TacticalCommitment > 0)
			{
				bot->TacticalCommitment -= frame.DeltaTime;
				return;
			}

			if (frame.Number % TickRate != 0)
			{
				return;
			}

			for (int i = 0; i < _tacticalSensors.Length; i++)
			{
				bool succeeded = _tacticalSensors[i].TrySetTactic(frame, entity, bot);

				if (succeeded == true)
				{
					bot->TacticalCommitment = _tacticalSensors[i].CommitmentValue;
					//Log.Info($"Tactical choice was: {_tacticalSensors[i].Path}");
					return;
				}
			}
		}

		public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
		{
			base.Loaded(resourceManager, allocator);

			_tacticalSensors = new TacticalSensor[TacticalSensors == null ? 0 : TacticalSensors.Length];
			if (TacticalSensors != null)
			{
				for (int i = 0; i < TacticalSensors.Length; i++)
				{
					_tacticalSensors[i] = (TacticalSensor)resourceManager.GetAsset(TacticalSensors[i].Id);
				}
			}
		}
	}
}
