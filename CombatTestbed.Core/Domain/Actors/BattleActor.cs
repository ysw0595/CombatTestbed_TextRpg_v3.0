using CombatTestbed.Core.Domain.Stats;
using CombatTestbed.Core.Domain.StatusEffects;
using System;

namespace CombatTestbed.Core.Domain.Actors
{
    /// <summary>
    /// 전투에 참여하는 모든 개체(플레이어, 몬스터, 보스)의 공통 도메인 엔티티입니다.
    /// </summary>
    public class BattleActor
    {
        public string Id { get; }
        public string Name { get; }
        public ActorRole Role { get; }

        /// <summary>
        /// 사망했거나 행동 불능 상태이상(마비 등)에 걸려 있는지 검사합니다.
        /// </summary>
        public bool CanAct => !Health.IsDead && !StatusEffects.HasEffect(StatusEffectType.Paralysis);

        public StatSheet Stats { get; }
        public HealthResource Health { get; }

        // 상태 이상 컨테이너 주머니
        public StatusEffectContainer StatusEffects { get; } = new StatusEffectContainer();

        // 순수 C# 도메인 이벤트: 콘솔/UI 등 외부에 상태 변화를 알림
        public event Action<BattleActor, int>? OnDamageTaken;
        public event Action<BattleActor, int>? OnHealed;
        public event Action<BattleActor>? OnDied;

        public BattleActor(string id, string name, ActorRole role, StatSheet stats)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("액터 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            Id = id;
            Name = name;
            Role = role;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));

            // StatSheet에서 MaxHp 스탯 계산기를 꺼내 생명력 자원에 연결
            Health = new HealthResource(stats.GetStat(StatType.MaxHp));
        }

        /// <summary>
        /// 턴 시작 처리: 상태 이상의 턴 시작 효과를 발동합니다. 사망한 액터는 무시합니다.
        /// </summary>
        public void StartTurn()
        {
            if (Health.IsDead) return;
            StatusEffects.ProcessTurnStart(this);
        }

        /// <summary>
        /// 턴 종료 처리: 상태 이상의 턴 종료 효과를 발동합니다. 사망한 액터는 무시합니다.
        /// </summary>
        public void EndTurn()
        {
            if (Health.IsDead) return;
            StatusEffects.ProcessTurnEnd(this);
        }

        /// <summary>
        /// 일반 피격 처리: 방어력(Defense)을 차감해 피해를 적용하고, 실제로 깎인 피해량을 반환합니다.
        /// </summary>
        public int ApplyDamage(int rawDamage)
        {
            if (Health.IsDead) return 0;

            // 방어력 공식: [ 들어온 피해 - 방어력 ] (최소 1 피해 보장)
            int defense = Stats.GetValue(StatType.Defense);
            int finalDamage = Math.Max(1, rawDamage - defense);

            return ResolveDamage(finalDamage);
        }

        /// <summary>
        /// 고정 피해(방어 무시) 처리: 중독, 출혈 등 상태 이상 피해에 사용하며, 실제로 깎인 피해량을 반환합니다.
        /// </summary>
        public int ApplyTrueDamage(int exactDamage)
        {
            if (Health.IsDead || exactDamage <= 0) return 0;

            return ResolveDamage(exactDamage);
        }

        /// <summary>
        /// 체력 회복 처리: 오버힐을 제외한 실제 회복량을 반환합니다.
        /// </summary>
        public int ApplyHeal(int amount)
        {
            if (Health.IsDead) return 0;

            int actualHeal = Health.Heal(amount);
            if (actualHeal > 0)
            {
                OnHealed?.Invoke(this, actualHeal);
            }

            return actualHeal;
        }

        /// <summary>
        /// 최종 피해량을 체력에 반영하고 이벤트를 발행하는 공통 처리입니다.
        /// </summary>
        private int ResolveDamage(int finalDamage)
        {
            int actualDamage = Health.TakeDamage(finalDamage);
            if (actualDamage > 0)
            {
                OnDamageTaken?.Invoke(this, actualDamage);
            }

            if (Health.IsDead)
            {
                OnDied?.Invoke(this);
            }

            return actualDamage;
        }
    }
}