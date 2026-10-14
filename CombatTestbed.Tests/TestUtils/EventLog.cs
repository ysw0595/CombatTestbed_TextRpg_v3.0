using System.Collections.Generic;

namespace CombatTestbed.Tests.TestUtils
{
    /// <summary>
    /// 이벤트 발행 순서를 문자열로 기록해 두었다가 한 번에 비교하기 위한 테스트용 기록기입니다.
    /// </summary>
    public class EventLog
    {
        private readonly List<string> _entries = new List<string>();

        public IReadOnlyList<string> Entries => _entries;

        public void Add(string entry)
        {
            _entries.Add(entry);
        }
    }
}