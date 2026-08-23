// Script/Animation/GraphExecutor.cs
using System;
using System.Collections.Generic;

public class GraphExecutor
{
    private Dictionary<string, BaseNodeData> _nodeMap = new Dictionary<string, BaseNodeData>();
    private Dictionary<string, string> _edgeMap = new Dictionary<string, string>();
    private string _startGuid;
    private Action<ActionNodeData> _onAction;
    private Func<IfNodeData, string> _onIf;
    private Action<SetMemberNodeData> _onSetMember;
    private Action<ForNodeData> _onFor;
    private Dictionary<Type, Func<BaseNodeData, string>> _handlers;

    // メイングラフ用: StartNodeDataを自動検出して開始点にする。
    public GraphExecutor(VisualScriptingGraphData data, Action<ActionNodeData> onAction, Func<IfNodeData, string> onIf, Action<SetMemberNodeData> onSetMember, Action<ForNodeData> onFor)
        : this(data, null, onAction, onIf, onSetMember, onFor)
    {
    }

    // Forの本体用: StartNodeDataを持たないため、開始点(Bodyポート先のノード)を明示的に渡す。
    public GraphExecutor(VisualScriptingGraphData data, string startGuid, Action<ActionNodeData> onAction, Func<IfNodeData, string> onIf, Action<SetMemberNodeData> onSetMember, Action<ForNodeData> onFor)
    {
        _onAction = onAction;
        _onIf = onIf;
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

        _handlers = new Dictionary<Type, Func<BaseNodeData, string>>
        {
            [typeof(StartNodeData)] = _ => "Out",
            [typeof(ActionNodeData)] = node => { _onAction?.Invoke((ActionNodeData)node); return "Out"; },
            [typeof(SetMemberNodeData)] = node => { _onSetMember?.Invoke((SetMemberNodeData)node); return "Out"; },
            [typeof(IfNodeData)] = node => _onIf?.Invoke((IfNodeData)node),
            [typeof(ForNodeData)] = node => { _onFor?.Invoke((ForNodeData)node); return "Complete"; }
        };
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
        return _handlers.TryGetValue(node.GetType(), out Func<BaseNodeData, string> handler) ? handler(node) : null;
    }
}