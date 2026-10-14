using System;
using System.Collections.Generic;
using CombatTestbed.Core.Domain.Actors;

namespace CombatTestbed.Core.Domain.StatusEffects
{
    /// <summary>
    /// 액터가 보유한 모든 상태 이상을 관리하고, 턴 생명주기에 맞춰 발동 및 제거를 담당하는 컨테이너입니다.
    /// 변화는 이벤트로 외부에 알립니다. (핸들러는 읽기 전용 관찰자여야 하며, 핸들러 안에서 컨테이너를 변경하면 안 됩니다.)
    /// </summary>
    public class StatusEffectContainer
    {
        private readonly List<IStatusEffect> _effects = new List<IStatusEffect>();

        /// <summary>
        /// 새로운 종류의 상태 이상이 처음 걸렸을 때 발행됩니다. (이미 걸린 효과의 중첩은 StacksChanged)
        /// </summary>
        public event Action<IStatusEffect>? EffectApplied;

        /// <summary>
        /// 이미 걸린 상태 이상의 스택이 바뀌었을 때 발행됩니다. (추가 부여, 턴 틱)
        /// 스택이 0이 되어 소멸하는 경우에는 발행하지 않고 EffectRemoved만 발행합니다.
        /// </summary>
        public event Action<IStatusEffect>? EffectStacksChanged;

        /// <summary>
        /// 상태 이상이 컨테이너에서 제거된 직후 발행됩니다. (만료, 정화, 일괄 제거)
        /// </summary>
        public event Action<IStatusEffect>? EffectRemoved;

        /// <summary>
        /// 새로운 상태 이상을 부여하거나 기존 상태 이상의 중첩(Stack)을 증가시킵니다.
        /// </summary>
        public void ApplyEffect(BattleActor target, IStatusEffect newEffect)
        {
            if (newEffect == null || newEffect.IsExpired) return;

            // 이미 동일한 타입의 상태 이상이 걸려 있는지 검색
            IStatusEffect existing = _effects.Find(e => e.Type == newEffect.Type);

            if (existing != null)
            {
                // 이미 있다면 기존 효과에 새로 들어온 효과의 스택만큼 합산
                int stacksBefore = existing.Stacks;
                existing.OnApply(target, newEffect.Stacks);
                NotifyIfStacksChanged(existing, stacksBefore);
            }
            else
            {
                // 처음 걸리는 효과라면 이미 newEffect 내부에 초기 스택이 있으므로 그대로 리스트에 등록
                _effects.Add(newEffect);
                EffectApplied?.Invoke(newEffect);
            }
        }

        /// <summary>
        /// 턴 시작 시 모든 상태 이상의 턴 시작 효과를 실행합니다.
        /// </summary>
        public void ProcessTurnStart(BattleActor target)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                IStatusEffect effect = _effects[i];
                int stacksBefore = effect.Stacks;

                effect.OnTurnStart(target);

                NotifyIfStacksChanged(effect, stacksBefore);
            }

            CleanupExpiredEffects(target);
        }

        /// <summary>
        /// 턴 종료 시 모든 상태 이상의 턴 종료 효과(독/출혈 틱 데미지 등)를 실행합니다.
        /// </summary>
        public void ProcessTurnEnd(BattleActor target)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                IStatusEffect effect = _effects[i];
                int stacksBefore = effect.Stacks;

                effect.OnTurnEnd(target);

                NotifyIfStacksChanged(effect, stacksBefore);
            }

            CleanupExpiredEffects(target);
        }

        /// <summary>
        /// 수명이 다한(스택이 0이 된) 상태 이상들을 안전하게 일괄 제거합니다.
        /// </summary>
        private void CleanupExpiredEffects(BattleActor target)
        {
            // 뒤에서부터 순회하며 만료된 효과 제거 (리스트 순회 중 안전한 삭제 기법)
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].IsExpired)
                {
                    RemoveAtAndNotify(i, target);
                }
            }
        }

        /// <summary>
        /// 특정 상태 이상이 현재 걸려 있는지 확인
        /// </summary>
        public bool HasEffect(StatusEffectType type)
        {
            return _effects.Exists(e => e.Type == type && !e.IsExpired);
        }

        /// <summary>
        /// 특정 상태 이상의 현재 중첩 수를 조회
        /// </summary>
        public int GetStacks(StatusEffectType type)
        {
            IStatusEffect effect = _effects.Find(e => e.Type == type && !e.IsExpired);
            return effect?.Stacks ?? 0;
        }

        /// <summary>
        /// 전투 종료 시 모든 상태 이상 초기화
        /// </summary>
        public void Clear(BattleActor target)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                RemoveAtAndNotify(i, target);
            }
        }

        /// <summary>
        /// 특정 상태 이상을 강제로 주머니에서 제거(정화)합니다.
        /// </summary>
        public bool RemoveEffect(BattleActor target, StatusEffectType type)
        {
            int index = _effects.FindIndex(e => e.Type == type);
            if (index < 0) return false;

            RemoveAtAndNotify(index, target);
            return true;
        }

        /// <summary>
        /// 효과를 목록에서 빼고 정리(OnRemove)한 뒤, 그 사실을 알립니다. (제거 경로의 공통 처리)
        /// </summary>
        private void RemoveAtAndNotify(int index, BattleActor target)
        {
            IStatusEffect effect = _effects[index];
            _effects.RemoveAt(index);
            effect.OnRemove(target);

            EffectRemoved?.Invoke(effect);
        }

        /// <summary>
        /// 스택이 바뀌었고 아직 살아 있는 효과에 대해서만 StacksChanged를 발행합니다.
        /// (만료되는 효과는 곧 Removed가 발행되므로 중복 통보하지 않음)
        /// </summary>
        private void NotifyIfStacksChanged(IStatusEffect effect, int stacksBefore)
        {
            if (effect.Stacks != stacksBefore && !effect.IsExpired)
            {
                EffectStacksChanged?.Invoke(effect);
            }
        }
    }
}