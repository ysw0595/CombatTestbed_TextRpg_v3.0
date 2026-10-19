#nullable enable
using System;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.Formulas;
using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Core.Skills
{
    /// <summary>
    /// 효과 데이터(SkillEffectData)를 실제 전투 상태 변화로 바꾸는 실행기입니다.
    /// </summary>
    public static class SkillEffectExecutor
    {
        /// <param name="opponent">상대. 자신을 대상으로 하는 효과(보상 등)만 실행한다면 null이어도 됩니다.</param>
        public static EffectOutcome Execute(SkillEffectData effect, BattleActor source, BattleActor? opponent)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            if (source == null) throw new ArgumentNullException(nameof(source));

            BattleActor receiver;
            if (effect.Target == EffectTarget.Self)
            {
                receiver = source;
            }
            else
            {
                receiver = opponent
                    ?? throw new ArgumentNullException(nameof(opponent), "상대를 대상으로 하는 효과에는 상대가 필요합니다.");
            }

            switch (effect.Kind)
            {
                case SkillEffectKind.Damage:
                    return ExecuteDamage(effect, source, receiver);
                case SkillEffectKind.Heal:
                    return ExecuteHeal(effect, source, receiver);
                case SkillEffectKind.ApplyStatus:
                    return ExecuteApplyStatus(effect, receiver);
                case SkillEffectKind.StatGain:
                    return ExecuteStatGain(effect, receiver);
                default:
                    throw new NotSupportedException($"지원하지 않는 효과 종류입니다: {effect.Kind}");
            }
        }

        private static EffectOutcome ExecuteDamage(SkillEffectData effect, BattleActor source, BattleActor receiver)
        {
            int amount = CombatFormula.CalculateAmount(
                effect.FlatValue, source.Stats.GetValue(effect.ScalingStat), effect.Coefficient);

            int dealt = 0;
            if (amount > 0)
            {
                dealt = effect.DamageType == DamageType.True
                    ? receiver.ApplyTrueDamage(amount)
                    : receiver.ApplyDamage(amount);
            }

            return new EffectOutcome(SkillEffectKind.Damage, receiver, dealt);
        }

        private static EffectOutcome ExecuteHeal(SkillEffectData effect, BattleActor source, BattleActor receiver)
        {
            int amount = CombatFormula.CalculateAmount(
                effect.FlatValue, source.Stats.GetValue(effect.ScalingStat), effect.Coefficient);

            int healed = amount > 0 ? receiver.ApplyHeal(amount) : 0;

            return new EffectOutcome(SkillEffectKind.Heal, receiver, healed);
        }

        private static EffectOutcome ExecuteApplyStatus(SkillEffectData effect, BattleActor receiver)
        {
            IStatusEffect statusEffect = StatusEffectFactory.Create(effect.StatusType, effect.Stacks);
            receiver.StatusEffects.ApplyEffect(receiver, statusEffect);

            return new EffectOutcome(SkillEffectKind.ApplyStatus, receiver, effect.Stacks);
        }

        private static EffectOutcome ExecuteStatGain(SkillEffectData effect, BattleActor receiver)
        {
            // 증가량은 받는 쪽의 현재 스탯을 기준으로 계산
            int gain = CombatFormula.CalculateAmount(
                effect.FlatValue, receiver.Stats.GetValue(effect.ScalingStat), effect.Coefficient);

            if (gain > 0)
            {
                if (effect.ScalingStat == StatType.MaxHp)
                {
                    // 최대 체력 증가는 현재 체력도 함께 증가 (HealthResource가 정책을 소유)
                    receiver.Health.IncreaseMaxHp(gain);
                }
                else
                {
                    receiver.Stats.GetStat(effect.ScalingStat).AddBaseValue(gain);
                }
            }

            return new EffectOutcome(SkillEffectKind.StatGain, receiver, gain, effect.ScalingStat);
        }
    }
}