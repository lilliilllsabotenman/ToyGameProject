// Script/Animation/GraphExecutorSample.cs
using System.Collections.Generic;
using UnityEngine;

public class NodeRunner : MonoBehaviour, INodeActionSource
{
    // Inspectorで、Save済みの GraphData.asset をここにドラッグ&ドロップする
    public VisualScriptingGraphDataBase GraphData;
    private readonly List<RuntimeNodeExecutor> executors = new();

    public GameObject Owner => this.gameObject;

    [SerializeField]private Rigidbody rgidbody;
    [SerializeField] private Animator animator;

    private void Start()
    {
        if (GraphData == null)
        {
            Debug.LogError("GraphDataがSetされていません");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animatorがセットされていません");
            return;
        }

        // 実行するパラメーターの一覧は手動で持たず、Animatorが実際に持つパラメーターから直接引く。
        DefaultNode target = new DefaultNode(Owner);
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            VisualScriptingGraphData data = GraphData.GetData(parameter.name);
            if (data == null) continue;
            executors.Add(new RuntimeNodeExecutor(data, target));
        }
    }

    private void Update()
    {
        foreach (RuntimeNodeExecutor executor in executors)
        {
            executor.Run();
        }
    }
}