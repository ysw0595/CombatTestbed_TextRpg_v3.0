namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 스탯에 적용되는 개별 수정치(버프/디버프 쪽지) 데이터 객체입니다.
    /// </summary>
    
    public class StatModifier
    {
        public StatModType Type { get; }  // Flat(고정합)인지 Percent(퍼센트)인지
        public float Value { get; }        // 수치 (+10, 0.2f 등)
        public object Source { get; }      // 이 버프를 붙인 주체(스킬 객체, 아이템, 상태이상 등)

        public StatModifier(StatModType type, float value, object source)
        {
            Type = type;
            Value = value;
            Source = source;
        }
    }
}