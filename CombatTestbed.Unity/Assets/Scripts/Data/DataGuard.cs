using System;
using System.Collections.Generic;

/// <summary>
/// SO의 참조 목록을 정의 객체로 바꿀 때, 빈 칸(None)이 있으면 어디가 비었는지 알려 주는 도우미입니다.
/// </summary>
public static class DataGuard
{
    public static List<TOut> ConvertAll<TIn, TOut>(
        UnityEngine.Object owner, string fieldName, IEnumerable<TIn> items, Func<TIn, TOut> convert)
        where TIn : UnityEngine.Object
    {
        var result = new List<TOut>();
        int index = 0;

        foreach (TIn item in items)
        {
            // Unity 오브젝트의 == 는 "삭제되었거나 비어 있는 참조"도 null로 취급합니다.
            if (item == null)
            {
                throw new InvalidOperationException($"[{owner.name}] '{fieldName}'의 {index}번 칸이 비어 있습니다.");
            }

            result.Add(convert(item));
            index++;
        }

        return result;
    }
}