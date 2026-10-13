using System.Collections.Generic;

namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 액터가 보유한 모든 스탯(Stat)을 딕셔너리로 보관하고 관리하는 컨테이너입니다.
    /// </summary>
    
    public class StatSheet
    {
        // StatType(Key)으로 해당 스탯 계산기(Value)를 보관하는 저장소
        private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

        /// <summary>
        /// 특정 스탯의 기본값을 등록하거나 수정합니다.
        /// </summary>
        
        public void SetStat(StatType type, int baseValue)
        {
            if (_stats.TryGetValue(type, out var existing))
            {
                // 이미 등록된 스탯이라면 기본값의 차이만큼만 가감하여 갱신
                existing.AddBaseValue(baseValue - existing.BaseValue);
            }
            else
            {
                // 새로 등록하는 스탯이면 인스턴스를 생성해 딕셔너리에 추가
                _stats[type] = new Stat(type, baseValue);
            }
        }

        /// <summary>
        /// 스탯 계산기(Stat) 객체 자체를 가져옵니다. (버프/디버프를 붙이거나 뗄 때 사용)
        /// </summary>
       
        public Stat GetStat(StatType type)
        {
            // 아직 등록되지 않은 스탯을 요청하면 기본값 0의 스탯을 안전하게 생성해서 반환 (Null 방지)
            if (!_stats.TryGetValue(type, out var stat))
            {
                stat = new Stat(type, 0);
                _stats[type] = stat;
            }
            return stat;
        }

        /// <summary>
        /// 최종 계산된 스탯 수치(int)만 즉시 꺼내올 때 사용하는 편의 메서드입니다.
        /// </summary>
        
        public int GetValue(StatType type)
        {
            return GetStat(type).Value;
        }
    }
}