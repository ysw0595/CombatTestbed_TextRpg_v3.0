using System.Collections.Generic;

namespace CombatTestbed.Presentation.Screens
{
    public interface IStageResultView
    {
        void ShowResult(string title, IReadOnlyList<string> lines);
    }
}