/*
 * Sensor.cs
 *
 * Desc: Base class for "Sensors", Asset objects that can alter the AI Blackboard or AIMemory
 *
 * Author: Omar Guendeli
 */
using Photon.Deterministic;

namespace Quantum
{
	public unsafe abstract partial class Sensor : AssetObject
	{
		public int TickRate = 1;

		public virtual void Execute(Frame frame, EntityRef entity)
		{
		}
	}
}
