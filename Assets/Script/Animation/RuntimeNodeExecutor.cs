// Script/Animation/RuntimeNodeExecutor.cs
using System;
using System.Collections.Generic;

// グラフの探索(次にどのノードへ進むか)と、ノード種別→NodeExecutorの呼び出しの振り分けだけを担う「実行マネージャー」。
// 各ノードを実際にどう実行するか(引数解決・メソッド呼び出し・Member読み書き)はNodeExecutorの責務。
public class RuntimeNodeExecutor
{
    private NodeExecutor _nodeExecutor;
    private Dictionary<Type, Func<BaseNodeData, string>> _handlers;

    // グラフの制御フロー(次にどのノードへ進むか)。
    private Dictionary<string, BaseNodeData> _nodeMap = new Dictionary<string, BaseNodeData>();
    private Dictionary<string, string> _edgeMap = new Dictionary<string, string>();
    private string _startGuid;
    // ForNodeData.Guid → 本体(Bodyポート先)ノードのGuid。構築時に1回だけ解決しておく。
    private Dictionary<string, string> _forBodyStartGuids;

    public RuntimeNodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        foreach (BaseNodeData nodeData in graphData.Nodes)
        {
            _nodeMap[nodeData.Guid] = nodeData;
            if (_startGuid == null && nodeData is StartNodeData)
            {
                _startGuid = nodeData.Guid;
            }
        }

        foreach (EdgeData edgeData in graphData.Edges)
        {
            string key = edgeData.OutputNodeGuid + ":" + edgeData.OutputPortName;
            _edgeMap[key] = edgeData.InputNodeGuid;
        }

        _forBodyStartGuids = BuildForBodyStartGuids(graphData);
        _nodeExecutor = new NodeExecutor(graphData, target, _forBodyStartGuids, RunFrom);

        _handlers = new Dictionary<Type, Func<BaseNodeData, string>>
        {
            [typeof(StartNodeData)] = _ => "Out",
            [typeof(ActionNodeData)] = node => { _nodeExecutor.InvokeAction((ActionNodeData)node); return "Out"; },
            [typeof(SetMemberNodeData)] = node => { _nodeExecutor.ExecuteSetMember((SetMemberNodeData)node); return "Out"; },
            [typeof(IfNodeData)] = node => _nodeExecutor.EvaluateIf((IfNodeData)node),
            [typeof(ForNodeData)] = node => { _nodeExecutor.RunFor((ForNodeData)node); return "Complete"; }
        };
    }

    private string ExecuteNode(BaseNodeData node)
    {
        Type type = node.GetType();
        
        if(_handlers.TryGetValue(type, out Func<BaseNodeData, string> handler)) return handler(node);

        return null;
    }

    // startGuidから、次のノードが無くなるまでExecuteNodeを辿り続ける。
    // RunFor()もこれを呼ぶ(NodeExecutorのコンストラクタへdelegateとして渡している)。
    private void RunFrom(string startGuid)
    {
        string currentGuid = startGuid;

        while (!string.IsNullOrEmpty(currentGuid))
        {
            if (!_nodeMap.TryGetValue(currentGuid, out BaseNodeData currentNode)) break;

            string outputPort = ExecuteNode(currentNode);
            if (string.IsNullOrEmpty(outputPort)) break;

            string edgeKey = currentGuid + ":" + outputPort;
            if (!_edgeMap.TryGetValue(edgeKey, out string nextGuid)) break;

            currentGuid = nextGuid;
        }
    }

    // グラフ内の全ForNodeData(入れ子含め、フラットなNodesリストを舐めるだけで自然に拾える)について、
    // Bodyポート先のノードのGuidを1回だけ解決しておく。
    private Dictionary<string, string> BuildForBodyStartGuids(VisualScriptingGraphData graphData)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (BaseNodeData nodeData in graphData.Nodes)
        {
            if (nodeData is not ForNodeData forData) continue;

            EdgeData bodyEdge = graphData.Edges
                .Find(e => e.OutputNodeGuid == forData.Guid && e.OutputPortName == "Body");
            if (bodyEdge == null) continue;

            result[forData.Guid] = bodyEdge.InputNodeGuid;
        }

        return result;
    }

    public void Run()
    {
        _nodeExecutor.ClearActionResultCache();
        RunFrom(_startGuid);
    }
}