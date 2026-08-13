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

    public GraphExecutor(VisualScriptingGraphData data, Action<ActionNodeData> onAction, Func<ConditionNodeData, string> onCondition, Action<SetMemberNodeData> onSetMember)
    {
        _onAction = onAction;
        _onCondition = onCondition;
        _onSetMember = onSetMember;

        foreach (BaseNodeData nodeData in data.Nodes)
        {
            _nodeMap[nodeData.Guid] = nodeData;
            if (nodeData is StartNodeData)
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
            default:
                return null;
        }
    }
}