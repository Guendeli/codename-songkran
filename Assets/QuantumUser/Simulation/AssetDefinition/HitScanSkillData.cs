//

using Photon.Deterministic;
using System;

namespace Quantum
{
    /// <summary>
    /// A Hit Scan Skill Data should be a Fire and Forget skill, calls OnDeactivate the moment OnAction is done
    /// </summary>
    [System.Serializable]
    public unsafe partial class HitScanSkillData : SkillData
    {
        public override EntityRef OnAction(Frame frame, EntityRef source, EntityRef skillEntity, Skill* skill)
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
            OnDeactivate(frame, skillEntity, skill);
            return attackEntity;
        }
    }
}