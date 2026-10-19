using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Rewards;
using CombatTestbed.Core.Skills;

namespace CombatTestbed.Core.Content
{
    /// <summary>
    /// 원본 게임의 몬스터별 처치 보상을 데이터로 옮긴 기본 콘텐츠입니다.
    /// (이후 Unity의 ScriptableObject로 같은 데이터를 편집할 수 있게 확장합니다.)
    /// </summary>
    public static class DefaultRewards
    {
        public static readonly RewardDefinition Farmer = new RewardDefinition("reward_farmer", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.MaxHp, 1)
        });

        public static readonly RewardDefinition Hunter = new RewardDefinition("reward_hunter", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 1)
        });

        public static readonly RewardDefinition Wolf = new RewardDefinition("reward_wolf", new[]
        {
            SkillEffectData.FlatHeal(EffectTarget.Self, 20)
        });

        public static readonly RewardDefinition Knight = new RewardDefinition("reward_knight", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.MaxHp, 10)
        });

        public static readonly RewardDefinition Werewolf = new RewardDefinition("reward_werewolf", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 4),
            SkillEffectData.FlatHeal(EffectTarget.Self, 30)
        });

        public static readonly RewardDefinition Paladin = new RewardDefinition("reward_paladin", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.MaxHp, 20)
        });

        public static readonly RewardDefinition Lycan = new RewardDefinition("reward_lycan", new[]
        {
            SkillEffectData.FlatStatGain(EffectTarget.Self, StatType.Attack, 8),
            SkillEffectData.FlatHeal(EffectTarget.Self, 40)
        });
    }
}