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
    public float Add(float a, float b) => a + b;

    [VisualScriptingGetter(DisplayName = "-")]
    public float Subtract(float a, float b) => a - b;

    [VisualScriptingGetter(DisplayName = "×")]
    public float Multiply(float a, float b) => a * b;

    [VisualScriptingGetter(DisplayName = "÷")]
    public float Divide(float a, float b) => a / b;

    [VisualScriptingGetter(DisplayName = "絶対値")]
    public float Abs(float f) => Mathf.Abs(f);

    [VisualScriptingGetter(DisplayName = "範囲内に収める")]
    public float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);

    [VisualScriptingGetter(DisplayName = "0〜1に収める")]
    public float Clamp01(float value) => Mathf.Clamp01(value);

    [VisualScriptingGetter(DisplayName = "線形補間")]
    public float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);

    [VisualScriptingGetter(DisplayName = "補間率を求める")]
    public float InverseLerp(float a, float b, float value) => Mathf.InverseLerp(a, b, value);

    [VisualScriptingGetter(DisplayName = "小さい方")]
    public float Min(float a, float b) => Mathf.Min(a, b);

    [VisualScriptingGetter(DisplayName = "大きい方")]
    public float Max(float a, float b) => Mathf.Max(a, b);

    [VisualScriptingGetter(DisplayName = "四捨五入")]
    public float Round(float f) => Mathf.Round(f);

    [VisualScriptingGetter(DisplayName = "切り捨て")]
    public float Floor(float f) => Mathf.Floor(f);

    [VisualScriptingGetter(DisplayName = "切り上げ")]
    public float Ceil(float f) => Mathf.Ceil(f);

    [VisualScriptingGetter(DisplayName = "平方根")]
    public float Sqrt(float f) => Mathf.Sqrt(f);

    [VisualScriptingGetter(DisplayName = "べき乗")]
    public float Pow(float f, float p) => Mathf.Pow(f, p);

    [VisualScriptingGetter(DisplayName = "符号")]
    public float Sign(float f) => Mathf.Sign(f);

    [VisualScriptingGetter(DisplayName = "値に近づける")]
    public float MoveTowards(float current, float target, float maxDelta) => Mathf.MoveTowards(current, target, maxDelta);

    [VisualScriptingGetter(DisplayName = "==")]
    public bool Equal(float a, float b) => a == b;

    [VisualScriptingGetter(DisplayName = "!=")]
    public bool NotEqual(float a, float b) => a != b;

    [VisualScriptingGetter(DisplayName = ">")]
    public bool Greater(float a, float b) => a > b;

    [VisualScriptingGetter(DisplayName = ">=")]
    public bool GreaterOrEqual(float a, float b) => a >= b;

    [VisualScriptingGetter(DisplayName = "<")]
    public bool Less(float a, float b) => a < b;

    [VisualScriptingGetter(DisplayName = "<=")]
    public bool LessOrEqual(float a, float b) => a <= b;
}
