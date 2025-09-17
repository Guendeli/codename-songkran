using Photon.Deterministic;

namespace Quantum
{
	[System.Serializable]
	public unsafe class GetTimer : AIFunction<FP>
	{
		public override FP Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
		{
			return frame.Global->MatchTimer;
		}
	}
}
