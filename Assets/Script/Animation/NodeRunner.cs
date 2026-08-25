// Script/Animation/GraphExecutorSample.cs
using System.Collections.Generic;
using UnityEngine;

public class NodeRunner : MonoBehaviour
{
    // Inspectorで、Save済みの GraphData.asset をここにドラッグ&ドロップする
    public VisualScriptingGraphDataBase GraphData;
    private readonly List<RuntimeNodeExecutor> executors = new();

    public GameObject Owner => this.gameObject;

    private void Start()
    {
        Animator animator = this.GetComponent<Animator>();

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
        
        DefaultNode target = new DefaultNode(Owner);
        foreach (VisualScriptingGraphData data in GraphData.data)
        {
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