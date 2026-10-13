using System;

namespace CombatTestbed.Core.Domain.Stats
{
    /// <summary>
    /// 최대 체력(MaxHp Stat)과 연동되어 현재 체력의 증감, 클램핑(0 ~ MaxHp), 사망 판정을 전담 관리합니다.
    /// </summary>
    public class HealthResource
    {
        private readonly Stat _maxHpStat;
        private int _currentHp;

        public int MaxHp => _maxHpStat.Value;

        /// <summary>
        /// 현재 체력. 버프/디버프로 최대 체력이 줄어든 경우에도 항상 MaxHp를 넘지 않게 보입니다.
        /// </summary>
        public int CurrentHp => Math.Min(_currentHp, MaxHp);

        public bool IsDead => CurrentHp <= 0;

        public HealthResource(Stat maxHpStat)
        {
            _maxHpStat = maxHpStat ?? throw new ArgumentNullException(nameof(maxHpStat));

            // 생성 시점에 MaxHp가 설정되어 있지 않으면 시작부터 사망 상태가 되므로 즉시 차단
            if (_maxHpStat.Value <= 0)
            {
                throw new ArgumentException(
                    "최대 체력은 1 이상이어야 합니다. 액터 생성 전에 MaxHp 스탯을 설정하세요.",
                    nameof(maxHpStat));
            }

            _currentHp = _maxHpStat.Value; // 시작 시 풀피로 초기화
        }

        /// <summary>
        /// 피해를 입히고, '실제로 깎인 유효 피해량'을 반환합니다.
        /// </summary>
        public int TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return 0;

            int prevHp = CurrentHp;
            _currentHp = Math.Clamp(prevHp - damage, 0, MaxHp);

            return prevHp - _currentHp;
        }

        /// <summary>
        /// 체력을 회복하고, '실제로 회복된 유효 수치'를 반환합니다. (오버힐 제외)
        /// </summary>
        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead) return 0;

            int prevHp = CurrentHp;
            _currentHp = Math.Clamp(prevHp + amount, 0, MaxHp);

            return _currentHp - prevHp;
        }

        /// <summary>
        /// 최대 체력을 영구적으로 늘리고, 생존 중이면 현재 체력도 같은 양만큼 늘립니다.
        /// (처치 보상 등. 사망한 상태에서는 부활하지 않습니다.)
        /// </summary>
        public void IncreaseMaxHp(int amount)
        {
            if (amount <= 0) return;

            _maxHpStat.AddBaseValue(amount);

            if (!IsDead)
            {
                _currentHp = Math.Min(MaxHp, CurrentHp + amount);
            }
        }

        /// <summary>
        /// 사망 여부와 무관하게 최대 체력으로 복구합니다. (전투 후 완치, 부활 용도)
        /// </summary>
        public void FullHeal()
        {
            _currentHp = MaxHp;
        }
    }
}