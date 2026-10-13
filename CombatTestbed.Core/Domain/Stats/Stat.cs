using System;
using System.Collections.Generic;

namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 단일 스탯(예: 공격력 하나)의 기본값과 버프들을 보관하고 최종값을 계산하는 클래스입니다.
    /// </summary>
    
    public class Stat
    {
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private bool _isDirty = true;       // 값이 변경되었는지 확인하는 깃발 (캐싱용)
        private int _cachedValue;           // 계산해 둔 최종값

        public StatType Type { get; }       // 어떤 스탯인지 (Attack, MaxHp 등)
        public int BaseValue { get; private set; } // 순수 기본 스탯

        // 외부에서 스탯을 조회할 때 사용하는 프로퍼티
        public int Value
        {
            get
            {
                // 버프가 추가/삭제되어 계산이 다시 필요할 때만 계산기 작동
                if (_isDirty)
                {
                    _cachedValue = CalculateFinalValue();
                    _isDirty = false;       // 계산 끝났으니 깃발 내림
                }
                return _cachedValue;        // 평소에는 이미 계산된 값만 즉시 반환
            }
        }

        public Stat(StatType type, int baseValue)
        {
            Type = type;
            BaseValue = baseValue;
        }

        // 영구 성장 (레벨업, 처치 보상 영구 스탯 증가 등)
        public void AddBaseValue(int amount)
        {
            BaseValue += amount;
            _isDirty = true; // 기본값이 바뀌었으니 다시 계산하라고 깃발 올림
        }

        // 버프/디버프 쪽지 부착
        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _isDirty = true;
        }

        // 특정 주체가 건 버프들만 떼어내기
        public bool RemoveModifiersFromSource(object source)
        {
            int removedCount = _modifiers.RemoveAll(mod => mod.Source == source);
            if (removedCount > 0)
            {
                _isDirty = true; // 버프가 빠졌으니 다시 계산하라고 깃발 올림
                return true;
            }
            return false;
        }

        // 모든 버프 일괄 제거 (전투 종료 시 등)
        public void ClearModifiers()
        {
            if (_modifiers.Count > 0)
            {
                _modifiers.Clear();
                _isDirty = true;
            }
        }

        // 실제 공식에 맞춰 최종 수치를 연산하는 내부 메서드
        private int CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float percentSum = 0f;

            // 1단계: 고정치(Flat)는 기본값에 먼저 전부 더하고, 퍼센트는 따로 합산
            foreach (var mod in _modifiers)
            {
                if (mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.Percent)
                {
                    percentSum += mod.Value;
                }
            }

            // 2단계: 합산된 퍼센트를 최종 곱연산
            // 공식: (기본값 + Flat 총합) * (1 + Percent 총합)
            finalValue *= (1f + percentSum);

            // 3단계: 반올림하여 최종 정수형 스탯 반환
            return (int)Math.Round(finalValue, MidpointRounding.AwayFromZero);
        }
    }
}