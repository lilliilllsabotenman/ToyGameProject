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

    [VisualScriptingAction]
    public void Debug_Log(string Message)
    {
        Debug.Log(Message);
    }

    [VisualScriptingGetter]
    public float GetPlayerSpeed()
    {
        float speed = rgidbody.linearVelocity.magnitude;
        return Mathf.Clamp01(speed);
    }

    [VisualScriptingGetter]
    public string FloatToString(float value)
    {
        return value.ToString();
    }

    [VisualScriptingAction]
    public void SetFloat(string parameterName, float value)
    {
        animator.SetFloat(parameterName, value);
    }

    [VisualScriptingAction]
    public string Attack(string value)
    {
        return value;
    }
}