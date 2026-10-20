using CombatTestbed.Core.Definitions;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Content
{
    /// <summary>
    /// 원본 게임의 플레이어/몬스터/보스를 데이터로 옮긴 기본 콘텐츠입니다.
    /// (방어력은 원본에 없던 개념이라 모두 0으로 두었습니다.)
    /// </summary>
    public static class DefaultActors
    {
        // ---- 플레이어 ----
        // 주의: 원본의 공격력 300은 테스트용 값으로 보입니다. 원본 값을 그대로 옮겼으니 필요하면 여기서 조정하세요.
        public static readonly ActorDefinition Vampire = new ActorDefinition(
            "vampire", "흡혈귀", ActorRole.Player, maxHp: 100, attack: 300);

        // ---- 일반 몬스터 ----
        public static readonly ActorDefinition Farmer = new ActorDefinition(
            "farmer", "농부", ActorRole.Enemy, maxHp: 20, attack: 2, reward: DefaultRewards.Farmer);

        public static readonly ActorDefinition Hunter = new ActorDefinition(
            "hunter", "사냥꾼", ActorRole.Enemy, maxHp: 25, attack: 5, reward: DefaultRewards.Hunter);

        public static readonly ActorDefinition Wolf = new ActorDefinition(
            "wolf", "늑대", ActorRole.Enemy, maxHp: 30, attack: 3, reward: DefaultRewards.Wolf);

        public static readonly ActorDefinition Knight = new ActorDefinition(
            "knight", "기사", ActorRole.Enemy, maxHp: 80, attack: 17, reward: DefaultRewards.Knight);

        public static readonly ActorDefinition Werewolf = new ActorDefinition(
            "werewolf", "늑대인간", ActorRole.Enemy, maxHp: 60, attack: 15, reward: DefaultRewards.Werewolf);

        public static readonly ActorDefinition Paladin = new ActorDefinition(
            "paladin", "성기사", ActorRole.Enemy, maxHp: 150, attack: 20, reward: DefaultRewards.Paladin);

        public static readonly ActorDefinition Lycan = new ActorDefinition(
            "lycan", "라이칸", ActorRole.Enemy, maxHp: 200, attack: 20, reward: DefaultRewards.Lycan);

        // ---- 보스 (처치 보상 없음, 스킬을 순환) ----
        public static readonly ActorDefinition Priest = new ActorDefinition(
            "priest", "성직자", ActorRole.Enemy, maxHp: 50, attack: 10,
            behaviorSkills: new[]
            {
                DefaultSkills.PriestReadSpell, DefaultSkills.PriestCross, DefaultSkills.PriestPray
            });

        public static readonly ActorDefinition VanHelsing = new ActorDefinition(
            "vanhelsing", "반 헬싱", ActorRole.Enemy, maxHp: 150, attack: 20,
            behaviorSkills: new[]
            {
                DefaultSkills.VanHelsingSilverArrow, DefaultSkills.VanHelsingContinuousArrow,
                DefaultSkills.VanHelsingPoisonArrow
            });

        public static readonly ActorDefinition WereFenrir = new ActorDefinition(
            "werefenrir", "베어펜리르", ActorRole.Enemy, maxHp: 350, attack: 30,
            behaviorSkills: new[]
            {
                DefaultSkills.WereFenrirMoonLeap, DefaultSkills.WereFenrirScratch,
                DefaultSkills.WereFenrirBite
            });
    }
}