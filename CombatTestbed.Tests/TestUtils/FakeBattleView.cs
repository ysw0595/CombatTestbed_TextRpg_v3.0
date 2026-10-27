using System.Collections.Generic;
using CombatTestbed.Presentation.Battle;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 전투 화면의 가짜 View입니다. 받은 값을 기록해 두고, 테스트가 나중에 확인합니다.
    /// </summary>
    public class FakeBattleView : IBattleView
    {
        public string Header { get; private set; } = "";
        public Dictionary<ActorSide, ActorViewModel> Actors { get; } = new Dictionary<ActorSide, ActorViewModel>();
        public int ShowActorCount { get; private set; }
        public List<string> Log { get; } = new List<string>();
        public int ClearLogCount { get; private set; }

        public void SetHeader(string text)
        {
            Header = text;
        }

        public void ShowActor(ActorSide side, ActorViewModel actor)
        {
            Actors[side] = actor;
            ShowActorCount++;
        }

        public void AppendLog(string line)
        {
            Log.Add(line);
        }

        public void ClearLog()
        {
            Log.Clear();
            ClearLogCount++;
        }
    }
}