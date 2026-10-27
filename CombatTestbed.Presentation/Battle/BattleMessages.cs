using CombatTestbed.Core.Actions;
using CombatTestbed.Core.Campaign;
using CombatTestbed.Core.Domain.Actors;
using CombatTestbed.Core.Domain.StatusEffects;

namespace CombatTestbed.Presentation.Battle
{
    /// <summary>
    /// 전투 화면에 나오는 문구를 한 곳에 모은 클래스입니다. (문구를 바꾸거나 다국어로 만들 때 여기만 고치면 됨)
    /// </summary>
    public static class BattleMessages
    {
        public static string Header(StageInfo stage, int round)
        {
            return $"챕터 {stage.ChapterIndex + 1} : {stage.Chapter.Name} (stage {stage.StageIndex + 1}) - 라운드 {round}";
        }

        public static string ActionFailed(ActionResult result)
        {
            return $"{result.Source.Name}은(는) 행동할 수 없습니다.";
        }

        public static string TurnSkipped(BattleActor actor)
        {
            return $"{actor.Name}은(는) 마비 상태라서 행동할 수 없다!";
        }

        public static string StatusDamage(BattleActor actor, int amount)
        {
            return $"{actor.Name}이(가) 상태이상으로 {amount}의 피해를 입었다.";
        }

        public static string Healed(BattleActor actor, int amount)
        {
            return $"{actor.Name}이(가) {amount} 회복했다.";
        }

        public static string Died(BattleActor actor)
        {
            return $"{actor.Name}이(가) 쓰러졌다.";
        }

        public static string StatusApplied(BattleActor owner, IStatusEffect effect)
        {
            return $"{owner.Name}에게 {effect.Name} {effect.Stacks}이(가) 걸렸다.";
        }
    }
}