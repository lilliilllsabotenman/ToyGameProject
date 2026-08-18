// Script/Animation/GraphExecutor.cs
using System;
using System.Collections.Generic;

public class GraphExecutor
{
    private Dictionary<string, BaseNodeData> _nodeMap = new Dictionary<string, BaseNodeData>();
    private Dictionary<string, string> _edgeMap = new Dictionary<string, string>();
    private string _startGuid;
    private Action<ActionNodeData> _onAction;
    private Func<ConditionNodeData, string> _onCondition;
    private Action<SetMemberNodeData> _onSetMember;
    private Action<ForNodeData> _onFor;

    // メイングラフ用: StartNodeDataを自動検出して開始点にする。
    public GraphExecutor(VisualScriptingGraphData data, Action<ActionNodeData> onAction, Func<ConditionNodeData, string> onCondition, Action<SetMemberNodeData> onSetMember, Action<ForNodeData> onFor)
        : this(data, null, onAction, onCondition, onSetMember, onFor)
    {
    }

    // Forの本体用: StartNodeDataを持たないため、開始点(Bodyポート先のノード)を明示的に渡す。
    public GraphExecutor(VisualScriptingGraphData data, string startGuid, Action<ActionNodeData> onAction, Func<ConditionNodeData, string> onCondition, Action<SetMemberNodeData> onSetMember, Action<ForNodeData> onFor)
    {
        _onAction = onAction;
        _onCondition = onCondition;
        _onSetMember = onSetMember;
        _onFor = onFor;
        _startGuid = startGuid;

        foreach (BaseNodeData nodeData in data.Nodes)
        {
            _nodeMap[nodeData.Guid] = nodeData;
            if (_startGuid == null && nodeData is StartNodeData)
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
            BaseNodeData currentNode;
            if (!_nodeMap.TryGetValue(currentGuid, out currentNode)) break;

            string outputPort = ExecuteNode(currentNode);
            if (string.IsNullOrEmpty(outputPort)) break;

            string edgeKey = currentGuid + ":" + outputPort;
            string nextGuid;
            if (!_edgeMap.TryGetValue(edgeKey, out nextGuid)) break;

            currentGuid = nextGuid;
        }
    }

    private string ExecuteNode(BaseNodeData node)
    {
        switch (node)
        {
            case StartNodeData:
                return "Out";
            case ActionNodeData actionData:
                _onAction?.Invoke(actionData);
                return "Out";
            case SetMemberNodeData setMemberData:
                _onSetMember?.Invoke(setMemberData);
                return "Out";
            case ConditionNodeData conditionData:
                return _onCondition?.Invoke(conditionData);
            case ForNodeData forData:
                _onFor?.Invoke(forData);
                return "Complete";
            default:
                return null;
        }
    }
}