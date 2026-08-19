// Script/Animation/DefaultNode.cs
using UnityEngine;

// ノードエディタで標準的に使える汎用アクション/ゲッター群。
// GameObjectにアタッチしてTargetTypeとして選択する想定。
[VisualScriptingSource(DisplayName = "基本")]
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