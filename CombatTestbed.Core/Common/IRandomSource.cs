namespace CombatTestbed.Core.Common
{
    /// <summary>
    /// 난수 공급자입니다. 난수를 직접 쓰지 않고 이 인터페이스로 받으면 테스트에서 값을 고정할 수 있습니다.
    /// </summary>
    public interface IRandomSource
    {
        /// <returns>minInclusive 이상 maxExclusive 미만의 정수</returns>
        int Next(int minInclusive, int maxExclusive);
    }
}