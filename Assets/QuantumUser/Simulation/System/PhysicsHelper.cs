using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
	public unsafe static class PhysicsHelper
	{
		private static int _layerMask = -1;

		public static HitCollection RaycastCollision(Frame frame, Transform2D* projectileTransform, FP velocity)
		{
			if(_layerMask == -1)
			{
				_layerMask = frame.Layers.GetLayerMask(AIConstants.LAYER_STATIC, AIConstants.LAYER_CHARACTER);
			}

			var nextPosition = projectileTransform->Position + projectileTransform->Up * frame.DeltaTime * velocity;
			if (FPVector2.DistanceSquared(projectileTransform->Position, nextPosition) == 0)
			{
				return default;
			}

			var hits = frame.Physics2D.LinecastAll(projectileTransform->Position, nextPosition, _layerMask,
				QueryOptions.ComputeDetailedInfo | QueryOptions.HitKinematics | QueryOptions.HitStatics);
			return hits;
			
		}
		
		public static HitCollection HitScanCollision(Frame frame, Transform2D* hitScanTransform, FP distance)
		{
			if(_layerMask == -1)
			{
				_layerMask = frame.Layers.GetLayerMask(AIConstants.LAYER_STATIC, AIConstants.LAYER_CHARACTER);
			}

			var nextPosition = hitScanTransform->Position + hitScanTransform->Up * distance;
			if (FPVector2.DistanceSquared(hitScanTransform->Position, nextPosition) == 0)
			{
				return default;
			}
			
			var hitScan = frame.Physics2D.RaycastAll(hitScanTransform->Position, hitScanTransform->Up, distance, _layerMask,
				QueryOptions.ComputeDetailedInfo | QueryOptions.HitKinematics | QueryOptions.HitStatics);
			
			return hitScan;
		}

		public static HitCollection OverlapShape(Frame frame, Transform2D* transform, int layerMask, Shape2DConfig shape) {
			var hits = frame.Physics2D.OverlapShape(*transform, shape.CreateShape(frame), layerMask,
				QueryOptions.ComputeDetailedInfo | QueryOptions.HitKinematics | QueryOptions.HitStatics);
			return hits;
		}
	}
}
