// Script/Animation/VisualScriptingGraphData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeData
{
    public string Guid;
    public string Type;       // "StartNode", "ActionNode" ...
    public string Title;
    public Rect Position;
    public string Parameter;  // GraphExecutorのonAction/onConditionへ渡されるキー（ActionのキーやConditionのキー）
}

[Serializable]
public class EdgeData
{
    public string OutputNodeGuid;
    public string OutputPortName;
    public string InputNodeGuid;
    public string InputPortName;
}

[CreateAssetMenu(fileName = "GraphData", menuName = "VS/Graph Data")]
public class VisualScriptingGraphData : ScriptableObject
{
    public List<NodeData> Nodes = new();
    public List<EdgeData> Edges = new();
}