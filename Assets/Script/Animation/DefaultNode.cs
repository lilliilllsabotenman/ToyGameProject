// Script/Animation/DefaultNode.cs
using UnityEngine;

// ノードエディタで標準的に使える汎用アクション/ゲッター群。
// GameObjectにアタッチしてTargetTypeとして選択する想定。
public class DefaultNode : INodeActionSource
{
    public GameObject Owner { get; }

    public DefaultNode(GameObject obj)
    {
        Owner = obj;
    }

    #region Math

    [VisualScriptingGetter(DisplayName = "+")]
    public float Add(float a, float b) => a + b;

    [VisualScriptingGetter(DisplayName = "-")]
    public float Subtract(float a, float b) => a - b;

    [VisualScriptingGetter(DisplayName = "×")]
    public float Multiply(float a, float b) => a * b;

    [VisualScriptingGetter(DisplayName = "÷")]
    public float Divide(float a, float b) => a / b;

    [VisualScriptingGetter]
    public float Abs(float f) => Mathf.Abs(f);

    [VisualScriptingGetter]
    public float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);

    [VisualScriptingGetter]
    public float Clamp01(float value) => Mathf.Clamp01(value);

    [VisualScriptingGetter]
    public float Lerp(float a, float b, float t) => Mathf.Lerp(a, b, t);

    [VisualScriptingGetter]
    public float InverseLerp(float a, float b, float value) => Mathf.InverseLerp(a, b, value);

    [VisualScriptingGetter]
    public float Min(float a, float b) => Mathf.Min(a, b);

    [VisualScriptingGetter]
    public float Max(float a, float b) => Mathf.Max(a, b);

    [VisualScriptingGetter]
    public float Round(float f) => Mathf.Round(f);

    [VisualScriptingGetter]
    public float Floor(float f) => Mathf.Floor(f);

    [VisualScriptingGetter]
    public float Ceil(float f) => Mathf.Ceil(f);

    [VisualScriptingGetter]
    public float Sqrt(float f) => Mathf.Sqrt(f);

    [VisualScriptingGetter]
    public float Pow(float f, float p) => Mathf.Pow(f, p);

    [VisualScriptingGetter]
    public float Sign(float f) => Mathf.Sign(f);

    [VisualScriptingGetter]
    public float MoveTowards(float current, float target, float maxDelta) => Mathf.MoveTowards(current, target, maxDelta);

    [VisualScriptingGetter]
    public float PingPong(float t, float length) => Mathf.PingPong(t, length);

    [VisualScriptingGetter]
    public float Repeat(float t, float length) => Mathf.Repeat(t, length);

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

    #endregion

    #region RigidBody

    [VisualScriptingGetter]
    public Rigidbody GetRigidBody(GameObject go) => go.GetComponent<Rigidbody>();

    [VisualScriptingGetter]
    public Vector3 GetVelocity(Rigidbody rb) => rb.linearVelocity;

    [VisualScriptingGetter]
    public float GetSpeed(Rigidbody rb) => rb.linearVelocity.magnitude;

    #endregion

    #region Collision

    [VisualScriptingGetter]
    public bool IsGrounded(GameObject go, float rayDistance) => Physics.Raycast(go.transform.position, Vector3.down, rayDistance);

    #endregion

    #region Animator

    [VisualScriptingGetter]
    public Animator GetAnimator(GameObject go) => go.GetComponent<Animator>();

    [VisualScriptingAction]
    public void SetFloat(Animator animator, string name, float value) => animator.SetFloat(name, value);

    [VisualScriptingAction]
    public void SetInteger(Animator animator, string name, int value) => animator.SetInteger(name, value);

    [VisualScriptingAction]
    public void SetBool(Animator animator, string name, bool value) => animator.SetBool(name, value);

    [VisualScriptingAction]
    public void SetTrigger(Animator animator, string name) => animator.SetTrigger(name);

    #endregion

    #region GameObject

    [VisualScriptingGetter]
    public GameObject Find(string name) => GameObject.Find(name);

    [VisualScriptingGetter]
    public GameObject FindWithTag(string tag) => GameObject.FindWithTag(tag);

    #endregion

    #region Time

    [VisualScriptingGetter]
    public float DeltaTime() => Time.deltaTime;

    #endregion

    #region Debug

    [VisualScriptingAction]
    public void _Debug(string Value) => Debug.Log(Value);

    #endregion

    #region Condition

    [VisualScriptingCondition]
    public ConditionParameter IF(bool condition)
    {
        if(condition) return ConditionParameter.True;
        else return ConditionParameter.False;
    }

    #endregion

    #region Convert

    [VisualScriptingGetter]
    public string FloatToString(float value) => value.ToString();

    [VisualScriptingGetter]
    public string IntToString(int value) => value.ToString();

    [VisualScriptingGetter]
    public string BoolToString(bool value) => value.ToString();

    #endregion
}