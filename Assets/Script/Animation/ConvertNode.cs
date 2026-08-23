// Script/Animation/ConvertNode.cs
using UnityEngine;

// 型変換系の汎用ゲッター群。GameObjectの状態には依存しない純粋関数のみ。
[VisualScriptingSource(DisplayName = "変換", IsDefault = true)]
public class ConvertNode : INodeActionSource
{
    public GameObject Owner { get; }

    public ConvertNode(GameObject obj)
    {
        Owner = obj;
    }

    [VisualScriptingGetter(DisplayName = "数値→文字列")]
    public static string FloatToString(float value) => value.ToString();

    [VisualScriptingGetter(DisplayName = "整数→文字列")]
    public static string IntToString(int value) => value.ToString();

    [VisualScriptingGetter(DisplayName = "真偽値→文字列")]
    public static string BoolToString(bool value) => value.ToString();
}
