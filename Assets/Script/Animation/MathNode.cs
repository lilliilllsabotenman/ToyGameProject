// Script/Animation/MathNode.cs
using UnityEngine;

// 算術・比較系の汎用ゲッター群。GameObjectの状態には依存しない純粋関数のみ。
[VisualScriptingSource(DisplayName = "計算")]
public class MathNode : INodeActionSource
{
    public GameObject Owner { get; }

    public MathNode(GameObject obj)
    {
        Owner = obj;
    }

    [VisualScriptingGetter(DisplayName = "+")]
    public static float Add(float a, float b) => a + b;

    [VisualScriptingGetter(DisplayName = "-")]
    public static float Subtract(float a, float b) => a - b;

    [VisualScriptingGetter(DisplayName = "×")]
    public static float Multiply(float a, float b) => a * b;

    [VisualScriptingGetter(DisplayName = "÷")]
    public static float Divide(float a, float b) => a / b;

    [VisualScriptingGetter(DisplayName = "絶対値")]
    public static float Abs(float f) => Mathf.Abs(f);

    [VisualScriptingGetter(DisplayName = "範囲内に収める")]
    public static float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);

    [VisualScriptingGetter(DisplayName = "0〜1に収める")]
    public static float Clamp01(float value) => Mathf.Clamp01(value);

    [VisualScriptingGetter(DisplayName = "線形補間")]
    public static float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);

    [VisualScriptingGetter(DisplayName = "補間率を求める")]
    public static float InverseLerp(float a, float b, float value) => Mathf.InverseLerp(a, b, value);

    [VisualScriptingGetter(DisplayName = "小さい方")]
    public static float Min(float a, float b) => Mathf.Min(a, b);

    [VisualScriptingGetter(DisplayName = "大きい方")]
    public static float Max(float a, float b) => Mathf.Max(a, b);

    [VisualScriptingGetter(DisplayName = "四捨五入")]
    public static float Round(float f) => Mathf.Round(f);

    [VisualScriptingGetter(DisplayName = "切り捨て")]
    public static float Floor(float f) => Mathf.Floor(f);

    [VisualScriptingGetter(DisplayName = "切り上げ")]
    public static float Ceil(float f) => Mathf.Ceil(f);

    [VisualScriptingGetter(DisplayName = "平方根")]
    public static float Sqrt(float f) => Mathf.Sqrt(f);

    [VisualScriptingGetter(DisplayName = "べき乗")]
    public static float Pow(float f, float p) => Mathf.Pow(f, p);

    [VisualScriptingGetter(DisplayName = "符号")]
    public static float Sign(float f) => Mathf.Sign(f);

    [VisualScriptingGetter(DisplayName = "値に近づける")]
    public static float MoveTowards(float current, float target, float maxDelta) => Mathf.MoveTowards(current, target, maxDelta);

    [VisualScriptingGetter(DisplayName = "==")]
    public static bool Equal(float a, float b) => a == b;

    [VisualScriptingGetter(DisplayName = "!=")]
    public static bool NotEqual(float a, float b) => a != b;

    [VisualScriptingGetter(DisplayName = ">")]
    public static bool Greater(float a, float b) => a > b;

    [VisualScriptingGetter(DisplayName = ">=")]
    public static bool GreaterOrEqual(float a, float b) => a >= b;

    [VisualScriptingGetter(DisplayName = "<")]
    public static bool Less(float a, float b) => a < b;

    [VisualScriptingGetter(DisplayName = "<=")]
    public static bool LessOrEqual(float a, float b) => a <= b;

    [VisualScriptingGetter(DisplayName = "X成分取得")]
    public static float GetX(Vector3 v) => v.x;

    [VisualScriptingGetter(DisplayName = "Y成分取得")]
    public static float GetY(Vector3 v) => v.y;

    [VisualScriptingGetter(DisplayName = "Z成分取得")]
    public static float GetZ(Vector3 v) => v.z;
}
