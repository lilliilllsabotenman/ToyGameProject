// Script/Animation/GraphExecutorSample.cs
using UnityEngine;

public class NodeRunner : MonoBehaviour, INodeActionSource
{
    // Inspectorで、Save済みの GraphData.asset をここにドラッグ&ドロップする
    public VisualScriptingGraphData GraphData;
    private RuntimeNodeExecutor executor;

    public GameObject Owner => this.gameObject;
    
    [SerializeField]private Rigidbody rgidbody;
    [SerializeField] private Animator animator;

    private void Start()
    {
        executor = new RuntimeNodeExecutor(GraphData, new DefaultNode(Owner));
    }

    private void Update()
    {
        executor.Run();
    }

    [VisualScriptingAction(DisplayName = "ログ出力")]
    public void Debug_Log(string Message)
    {
        Debug.Log(Message);
    }

    [VisualScriptingGetter(DisplayName = "プレイヤー速さ取得")]
    public float GetPlayerSpeed()
    {
        float speed = rgidbody.linearVelocity.magnitude;
        return Mathf.Clamp01(speed);
    }

    [VisualScriptingGetter(DisplayName = "数値→文字列")]
    public string FloatToString(float value)
    {
        return value.ToString();
    }

    [VisualScriptingAction(DisplayName = "アニメーションパラメーター(Float)を設定")]
    public void SetFloat(string parameterName, float value)
    {
        animator.SetFloat(parameterName, value);
    }

    [VisualScriptingAction(DisplayName = "攻撃")]
    public string Attack(string value)
    {
        return value;
    }
}