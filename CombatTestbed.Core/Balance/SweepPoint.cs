namespace CombatTestbed.Core.Balance
{
    /// <summary>
    /// 스윕에서 값 하나를 시험한 결과입니다.
    /// </summary>
    public sealed class SweepPoint
    {
        public int Value { get; }
        public SimulationReport Report { get; }

        public SweepPoint(int value, SimulationReport report)
        {
            Value = value;
            Report = report;
        }
    }
}