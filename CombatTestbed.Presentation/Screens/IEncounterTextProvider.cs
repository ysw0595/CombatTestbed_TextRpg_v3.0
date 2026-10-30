#nullable enable

using CombatTestbed;

namespace CombatTestbed.Presentation.Screens
{
    /// <summary>
    /// 전투의 등장/처치 연출 문구를 공급하는 계약입니다.
    /// 연출 문구는 있으면 좋은 선택 사항이라, 없으면 null을 돌려줍니다.
    /// </summary>
    public interface IEncounterTextProvider
    {
        /// <param name="actorId">등장하는 적의 정의 Id (예: "farmer")</param>
        string? GetAppearText(string actorId);

        /// <param name="actorId">처치한 적의 정의 Id</param>
        string? GetSlainText(string actorId);
    }
}