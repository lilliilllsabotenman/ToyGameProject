// Script/Animation/GraphExecutor.cs
using System;
using System.Collections.Generic;

// NodeData/EdgeDataを解釈してStartから辿るだけの層。
// ActionNode/ConditionNodeが実際に何を呼ぶかは呼び出し側がonAction/onConditionで渡す。
// Type文字列はEditor側のノードクラス名(StartNode/ActionNode/ConditionNode)と一致させる規約。
public class GraphExecutor
{
    private const string StartNodeType = "StartNode";
    private const string ActionNodeType = "ActionNode";
    private const string ConditionNodeType = "ConditionNode";

    private Dictionary<string, NodeData> _nodeMap = new Dictionary<string, NodeData>();
    private Dictionary<string, string> _edgeMap = new Dictionary<string, string>();
    private string _startGuid;
    private Action<string> _onAction;
    private Func<string, bool> _onCondition;

    public GraphExecutor(VisualScriptingGraphData data, Action<string> onAction, Func<string, bool> onCondition)
    {
        _onAction = onAction;
        _onCondition = onCondition;

        foreach (NodeData nodeData in data.Nodes)
        {
            _nodeMap[nodeData.Guid] = nodeData;
            if (nodeData.Type == StartNodeType)
            {
                _startGuid = nodeData.Guid;
            }
        }

        foreach (EdgeData edgeData in data.Edges)
        {
            string key = edgeData.OutputNodeGuid + ":" + edgeData.OutputPortName;
            _edgeMap[key] = edgeData.InputNodeGuid;
        }
    }

    public void Run()
    {
        string currentGuid = _startGuid;

        while (!string.IsNullOrEmpty(currentGuid))
        {
            NodeData currentNode;
            if (!_nodeMap.TryGetValue(currentGuid, out currentNode)) break;

            string outputPort = ExecuteNode(currentNode);
            if (string.IsNullOrEmpty(outputPort)) break;

            string edgeKey = currentGuid + ":" + outputPort;
            string nextGuid;
            if (!_edgeMap.TryGetValue(edgeKey, out nextGuid)) break;

            currentGuid = nextGuid;
        }
    }

    private string ExecuteNode(NodeData node)
    {
        switch (node.Type)
        {
            case StartNodeType:
                return "Out";
            case ActionNodeType:
                _onAction?.Invoke(node.Parameter);
                return "Out";
            case ConditionNodeType:
                bool result = _onCondition != null && _onCondition(node.Parameter);
                return result ? "True" : "False";
            default:
                return null;
        }
    }
}
