using Photon.Deterministic;
using Quantum.Physics2D;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
	// Base class for all attack assets
	
	public unsafe abstract partial class AttackData : AssetObject
	{
#if QUANTUM_UNITY
		[Header("View Configuration", order = 9)]
		public BaseAttackCosmeticData CosmeticData;
#endif
		
		// For how many time the Attack is activated
		public FP TTL;

		// Should this attack ignored it's owner
		public bool IgnoreOwner;

		// Should this attack hit enemies
		public bool HitEnemies;

		// Should this attack hit allies
		public bool HitAlies;

		// Is this attack destroyed upon contact to any Dynamic collider?
		public bool DestroyOnHitDynamic;

		// Is this attack destroyed upon contact to any Static collider?
		public bool DestroyOnHitStatic;

		// The effects that are applied to OTHERS
		public Effect[] EffectsOnOthers;

		// The effects that are applied to the attack's OWNER whtn it hits another entity
		public Effect[] EffectsOnSource;

		// Polymorphic OnCreate logic
		public virtual void OnCreate(Frame frame, EntityRef attackEntity, EntityRef source, Attack* attack)
		{
			frame.Signals.OnCreateAttack(attackEntity, attack);
			frame.Events.OnCreateAttack(Guid);
		}

		// Polymorphic OnUpdate logic
		public virtual void OnUpdate(Frame frame, EntityRef attackEntity, Attack* attack)
		{
			attack->TTL += frame.DeltaTime;
		}

		// Polymorphic OnDeactivate logic
		public virtual void OnDeactivate(Frame frame, EntityRef attackEntity)
		{
			frame.Signals.OnDisableAttack(attackEntity);
			frame.Destroy(attackEntity);
		}

		// Apply the effects, defined in the Unity inspector
		public virtual void OnApplyEffectAll(Frame frame, EntityRef source, EntityRef target)
		{
			foreach (var effect in EffectsOnOthers)
			{
				EffectsHelper.OnApply(frame, source, target, effect);
			}

			for (int i = 0; i < EffectsOnSource.Length; i++)
			{
				Effect effect = EffectsOnSource[i];
				EffectsHelper.OnApply(frame, source, source, effect);
			}
		}
		
		public virtual void OnApplyEffectSource(Frame frame, EntityRef source)
		{
			
			for (int i = 0; i < EffectsOnSource.Length; i++)
			{
				Effect effect = EffectsOnSource[i];
				EffectsHelper.OnApply(frame, source, source, effect);
			}
		}
		
		public virtual void OnApplyEffectTarget(Frame frame, EntityRef source, EntityRef target)
		{
			
			for (int i = 0; i < EffectsOnOthers.Length; i++)
			{
				Effect effect = EffectsOnOthers[i];
				EffectsHelper.OnApply(frame, source, target, effect);
			}
		}

		// Collision logic, investigating what on the hits collection should be affected
		protected EntityRef CheckHits(Frame frame, HitCollection hits, EntityRef attackEntity,
			Attack* attack, Transform2D* attackTransform, out bool wasDisabled)
		{
			wasDisabled = false;

			for (var i = 0; i < hits.Count; i++)
			{
				if (hits[i].IsDynamic == false)
				{
					if (hits[i].IsTrigger == true)
					{
						continue;
					}
					else if (DestroyOnHitStatic == true)
					{
						OnDeactivate(frame, attackEntity);
						wasDisabled = true;
						return default;
					}
				}

				var target = hits[i].Entity;
				if (frame.Exists(target) == true && frame.TryGet(target, out Health targetHealth) == true)
				{
					if (IgnoreOwner && target == attack->Source)
					{
						continue;
					}

					if (targetHealth.IsDead)
					{
						continue;
					}

					TeamInfo sourceTeamInfo = frame.Get<TeamInfo>(attack->Source);
					if (frame.TryGet<TeamInfo>(target, out var teamInfo))
					{
						if (HitEnemies == false && sourceTeamInfo.Index != teamInfo.Index)
						{
							continue;
						}

						if (HitAlies == false && sourceTeamInfo.Index == teamInfo.Index)
						{
							continue;
						}
					}

					if (DestroyOnHitDynamic == true)
					{
						attackTransform->Position = hits[i].Point;
						OnDeactivate(frame, attackEntity);
						wasDisabled = true;
					}
					return target;
				}
			}

			return default;
		}
	}
}
