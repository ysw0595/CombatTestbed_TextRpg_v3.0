using UnityEngine;

/// <summary>
/// Reset()에서 자식 오브젝트를 이름 경로로 찾아 컴포넌트를 돌려주는 도우미입니다.
/// </summary>
public static class ViewWiring
{
    public static T Find<T>(Transform root, string path) where T : Component
    {
        Transform child = root.Find(path);
        return child != null ? child.GetComponent<T>() : null;
    }
}