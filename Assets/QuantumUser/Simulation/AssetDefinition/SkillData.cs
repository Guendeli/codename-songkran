using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
	public enum EPreviewType : int
	{
		None = 0,
		Linear,
		Ballistic,
		Angle
	}
	
	public abstract unsafe  partial class SkillData : AssetObject
	{
#if QUANTUM_UNITY
		[Header("View Configuration", order = 9)]
		public Sound SFX;
		public EPreviewType AttackPreviewType;
		[Header("Quantum Simulation Configuration", order = 10)]
#endif
		public AssetRef<EntityPrototype> SkillPrototype;
		public AssetRef<EntityPrototype> AttackPrototype;

		public bool HasTTL;
		public FP ActionInterval;
		public int ActionAmount;
		public FP RotationLockDuration;
		public FP MovementLockDuration;
		public bool AutoAimCheckSight = true;
		public FP Cost = 1;
		public EAttributeType CostType;
		public FP AutoAimRadius = 10;

		public virtual EntityRef OnCreate(Frame frame, EntityRef source, SkillData data,
			FPVector2 characterPos, FPVector2 actionVector)
		{
			EntityRef skillEntity = frame.Create(SkillPrototype);
			Skill* skill = frame.Unsafe.GetPointer<Skill>(skillEntity);
			skill->SkillData = data;
			skill->Source = source;
			skill->ActionTimer = ActionInterval;
			skill->ActionVector = actionVector;

			Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillEntity);
			transform->Position = characterPos;

			if (Cost != 0)
			{
				AttributesHelper.ChangeAttribute(frame, source, CostType, EModifierAppliance.OneTime, EModifierOperation.Subtract, Cost, 0);
			}
			return skillEntity;
		}

		public virtual void OnUpdate(Frame frame, EntityRef source, EntityRef skillEntity, Skill* skill)
		{
			if (skill->ActionTimer >= ActionInterval)
			{
				skill->ActionTimer = 0;
				OnAction(frame, source, skillEntity, skill);
			}
			skill->TTL += frame.DeltaTime;
			skill->ActionTimer += frame.DeltaTime;
		}

		public virtual EntityRef OnAction(Frame frame, EntityRef source, EntityRef skillEntity, Skill* skill)
		{
			
			EntityRef attackEntity = frame.Create(AttackPrototype);
			Transform2D* attackTransform = frame.Unsafe.GetPointer<Transform2D>(attackEntity);

			if (source != default)
			{
				Transform2D sourceTransform = frame.Get<Transform2D>(source);

				attackTransform->Position = sourceTransform.Position;
				attackTransform->Rotation = sourceTransform.Rotation;
			}
			else
			{
				Transform2D skillTransform = frame.Get<Transform2D>(skillEntity);

				attackTransform->Position = skillTransform.Position;
				attackTransform->Rotation = skillTransform.Rotation;
			}

			Attack* attack = frame.Unsafe.GetPointer<Attack>(attackEntity);
			attack->Source = source;
			AttackData data = frame.FindAsset<AttackData>(attack->AttackData.Id);

			data.OnCreate(frame, attackEntity, source, attack);
			frame.Events.SkillAction(skill->SkillData.Id);

			return attackEntity;
		}

		public virtual void OnDeactivate(Frame frame, EntityRef skillEntity, Skill* skill)
		{
			var skillData = frame.FindAsset<SkillData>(skill->SkillData.Id);
			frame.Signals.OnDisableSkill(skill->Source, skillData);

			frame.Destroy(skillEntity);
		}
	}
}
