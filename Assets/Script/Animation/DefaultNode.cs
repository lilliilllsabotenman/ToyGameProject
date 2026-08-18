// Script/Animation/DefaultNode.cs
using UnityEngine;

// ノードエディタで標準的に使える汎用アクション/ゲッター群。
// GameObjectにアタッチしてTargetTypeとして選択する想定。
public class DefaultNode : INodeActionSource
{
    public GameObject Owner { get; }

    private Rigidbody rigidbody;
    private Animator animator;
    private BoxCollider collision;

    public DefaultNode(GameObject obj)
    {
        Owner = obj;

        if(Owner == null) return;

        rigidbody = Owner.GetComponent<Rigidbody>();
        animator = Owner.GetComponent<Animator>();
        collision = Owner.GetComponent<BoxCollider>();

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

    #endregion

    #region RigidBody

    [VisualScriptingGetter(DisplayName = "Rigidbody取得")]
    public Rigidbody GetRigidBody(GameObject go) => go.GetComponent<Rigidbody>();


    [VisualScriptingGetter(DisplayName = "速さ取得")]
    public float GetSpeed(Rigidbody rb) => rb.linearVelocity.magnitude;

    [VisualScriptingGetter(DisplayName = "自分の速さ取得")]
    public float GetOwnerSpeed() => rigidbody.linearVelocity.magnitude;

    #endregion

    #region Collision

    [VisualScriptingGetter(DisplayName = "接地判定")]
    public bool IsGrounded(GameObject go, float rayDistance) => Physics.Raycast(go.transform.position, Vector3.down, rayDistance);

    [VisualScriptingGetter(DisplayName = "自分の接地判定")]
    public bool IsOwnerGrounded(float rayDistance) => Physics.Raycast(Owner.transform.position, Vector3.down, rayDistance);

    #endregion

    #region Animator

    [VisualScriptingGetter(DisplayName = "Animator取得")]
    public Animator GetAnimator(GameObject go) => go.GetComponent<Animator>();

    [VisualScriptingAction(DisplayName = "アニメーションパラメーター(Float)を設定")]
    public void SetFloat(Animator animator, string name, float value) => animator.SetFloat(name, value);

    [VisualScriptingAction(DisplayName = "アニメーションパラメーター(Int)を設定")]
    public void SetInteger(Animator animator, string name, int value) => animator.SetInteger(name, value);

    [VisualScriptingAction(DisplayName = "アニメーションパラメーター(Bool)を設定")]
    public void SetBool(Animator animator, string name, bool value) => animator.SetBool(name, value);

    [VisualScriptingAction(DisplayName = "アニメーションパラメーター(Trigger)を実行")]
    public void SetTrigger(Animator animator, string name) => animator.SetTrigger(name);

    [VisualScriptingAction(DisplayName = "自分のアニメーションパラメーター(Float)を設定")]
    public void SetOwnerFloat(string name, float value) => animator.SetFloat(name, value);

    [VisualScriptingAction(DisplayName = "自分のアニメーションパラメーター(Int)を設定")]
    public void SetOwnerInteger(string name, int value) => animator.SetInteger(name, value);

    [VisualScriptingAction(DisplayName = "自分のアニメーションパラメーター(Bool)を設定")]
    public void SetOwnerBool(string name, bool value) => animator.SetBool(name, value);

    [VisualScriptingAction(DisplayName = "自分のアニメーションパラメーター(Trigger)を実行")]
    public void SetOwnerTrigger(string name) => animator.SetTrigger(name);

    #endregion

    #region GameObject

    [VisualScriptingGetter(DisplayName = "名前で検索")]
    public GameObject Find(string name) => GameObject.Find(name);

    [VisualScriptingGetter(DisplayName = "タグで検索")]
    public GameObject FindWithTag(string tag) => GameObject.FindWithTag(tag);

    #endregion

    #region Time

    [VisualScriptingGetter(DisplayName = "経過時間(1フレーム)")]
    public float DeltaTime() => Time.deltaTime;

    #endregion

    #region Debug

    [VisualScriptingAction(DisplayName = "ログ出力")]
    public void _Debug(string Value) => Debug.Log(Value);

    #endregion

    #region Condition

    [VisualScriptingCondition(DisplayName = "もし〜なら")]
    public ConditionParameter IF(bool condition)
    {
        if(condition) return ConditionParameter.True;
        else return ConditionParameter.False;
    }

    #endregion

    #region Convert

    [VisualScriptingGetter(DisplayName = "数値→文字列")]
    public string FloatToString(float value) => value.ToString();

    [VisualScriptingGetter(DisplayName = "整数→文字列")]
    public string IntToString(int value) => value.ToString();

    [VisualScriptingGetter(DisplayName = "真偽値→文字列")]
    public string BoolToString(bool value) => value.ToString();

    #endregion
}