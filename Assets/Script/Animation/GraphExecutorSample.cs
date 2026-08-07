// Script/Animation/GraphExecutorSample.cs
using UnityEngine;

public class GraphExecutorSample : MonoBehaviour, INodeActionSource
{
    // Inspectorで、Save済みの GraphData.asset をここにドラッグ&ドロップする
    public VisualScriptingGraphData GraphData;
    private RuntimeNodeExecutor executor;

    private void Start()
    {
        executor = new RuntimeNodeExecutor(GraphData, this);
    }

    private void Update()
    {
        executor.Run();
    }

    [VisualScriptingAction]
    public void Test()
    {
        Debug.Log("ジャンプした");
    }

    [VisualScriptingAction]
    public void Attack()
    {
        Debug.Log("攻撃した");
    }

    [VisualScriptingCondition]
    public bool AAAA()
    {
        return true;
    }
}
