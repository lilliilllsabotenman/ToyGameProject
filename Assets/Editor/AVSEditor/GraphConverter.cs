// Editor/GraphConverter.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

// GraphView上のノード/エッジ ⇔ VisualScriptingGraphData の純粋な変換だけを担う。
// どこに保存するか・履歴・検証はVisualScriptingGraphViewの責務で、こちらは一切関知しない。
public static class GraphConverter
{
    // View(BaseNode)→保存データ(BaseNodeData)への変換テーブル。ActionNode/GetterNodeはどちらもActionNodeDataへ変換される。
    private static readonly Dictionary<Type, Func<BaseNode, BaseNodeData>> _saveHandlers = new Dictionary<Type, Func<BaseNode, BaseNodeData>>
    {
        [typeof(ActionNode)] = node =>
        {
            ActionNode actionNode = (ActionNode)node;
            return new ActionNodeData { MethodKey = actionNode.ActionKey, Params = CloneParams(actionNode.Params), SourceTypeName = actionNode.SourceTypeName };
        },
        [typeof(GetterNode)] = node =>
        {
            GetterNode getterNode = (GetterNode)node;
            return new ActionNodeData { MethodKey = getterNode.ActionKey, Params = CloneParams(getterNode.Params), SourceTypeName = getterNode.SourceTypeName };
        },
        [typeof(GetMemberNode)] = node => new GetMemberNodeData { MemberName = ((GetMemberNode)node).MemberName },
        [typeof(SetMemberNode)] = node =>
        {
            SetMemberNode setMemberNode = (SetMemberNode)node;
            return new SetMemberNodeData { MemberName = setMemberNode.MemberName, Params = CloneParams(setMemberNode.Params) };
        },
        [typeof(IfNode)] = node => new IfNodeData { Params = CloneParams(((IfNode)node).Params) },
        [typeof(ForNode)] = node => new ForNodeData { Params = CloneParams(((ForNode)node).Params) },
        [typeof(StartNode)] = _ => new StartNodeData()
    };

    // 保存データ(BaseNodeData)→View(BaseNode)への変換テーブル。第2引数はResolveMemberType用のVisualScriptingGraphData。
    // ActionNodeDataだけはNodeMethodOptions.IsGetterの実行時判定でActionNode/GetterNodeのどちらを作るか分かれるため、ハンドラ内でif分岐する。
    private static readonly Dictionary<Type, Func<BaseNodeData, VisualScriptingGraphData, BaseNode>> _rebuildHandlers = new Dictionary<Type, Func<BaseNodeData, VisualScriptingGraphData, BaseNode>>
    {
        [typeof(StartNodeData)] = (nodeData, data) => new StartNode(),
        [typeof(ActionNodeData)] = (nodeData, data) =>
        {
            ActionNodeData actionData = (ActionNodeData)nodeData;
            Type sourceType = ResolveSourceType(actionData.SourceTypeName);
            return NodeMethodOptions.IsGetter(sourceType, actionData.MethodKey)
                ? new GetterNode(sourceType, actionData.MethodKey, actionData.Params)
                : new ActionNode(sourceType, actionData.MethodKey, actionData.Params);
        },
        [typeof(GetMemberNodeData)] = (nodeData, data) =>
        {
            GetMemberNodeData getMemberData = (GetMemberNodeData)nodeData;
            return new GetMemberNode(getMemberData.MemberName, ResolveMemberType(data, getMemberData.MemberName));
        },
        [typeof(SetMemberNodeData)] = (nodeData, data) =>
        {
            SetMemberNodeData setMemberData = (SetMemberNodeData)nodeData;
            return new SetMemberNode(setMemberData.MemberName, ResolveMemberType(data, setMemberData.MemberName), setMemberData.Params);
        },
        [typeof(IfNodeData)] = (nodeData, data) => new IfNode(((IfNodeData)nodeData).Params),
        [typeof(ForNodeData)] = (nodeData, data) => new ForNode(((ForNodeData)nodeData).Params)
    };

    // graphView上のノード/エッジから新規のVisualScriptingGraphDataを作る(Name/Nodes/Edgesのみ埋める純粋変換)。
    // ParameterType/Membersはこの変換の対象外(呼び出し側がそれぞれ別途扱う)。
    public static VisualScriptingGraphData ToData(VisualScriptingGraphView graphView, string name)
    {
        VisualScriptingGraphData data = new() { Name = name };

        foreach (BaseNode node in graphView.nodes.ToList().Cast<BaseNode>())
        {
            if (!_saveHandlers.TryGetValue(node.GetType(), out Func<BaseNode, BaseNodeData> handler)) continue;
            BaseNodeData nodeData = handler(node);
            if (nodeData == null) continue;

            nodeData.Guid = node.NodeGuid;
            nodeData.Title = node.title;
            nodeData.Position = node.GetPosition();
            data.Nodes.Add(nodeData);
        }

        foreach (Edge edge in graphView.edges.ToList())
        {
            if (edge.output.node is not BaseNode outputNode) continue;
            if (edge.input.node is not BaseNode inputNode) continue;

            data.Edges.Add(new EdgeData
            {
                OutputNodeGuid = outputNode.NodeGuid,
                OutputPortName = edge.output.portName,
                InputNodeGuid = inputNode.NodeGuid,
                InputPortName = edge.input.portName
            });
        }

        return data;
    }

    // dataの内容に合わせてGraphViewの表示(ノード/エッジ)を作り直す。この間はAutoSaveを止める。
    public static void RebuildView(VisualScriptingGraphView graphView, VisualScriptingGraphData data)
    {
        graphView.IsLoading = true;
        try
        {
            graphView.DeleteElements(graphView.graphElements.ToList());

            Dictionary<string, BaseNode> nodeMap = new Dictionary<string, BaseNode>();

            foreach (BaseNodeData nodeData in data.Nodes)
            {
                if (!_rebuildHandlers.TryGetValue(nodeData.GetType(), out Func<BaseNodeData, VisualScriptingGraphData, BaseNode> handler)) continue;
                BaseNode node = handler(nodeData, data);
                if (node == null) continue;

                node.NodeGuid = nodeData.Guid;
                node.SetPosition(nodeData.Position);
                graphView.AddElement(node);
                nodeMap[nodeData.Guid] = node;
            }

            foreach (EdgeData edgeData in data.Edges)
            {
                BaseNode outputNode;
                BaseNode inputNode;
                if (!nodeMap.TryGetValue(edgeData.OutputNodeGuid, out outputNode)) continue;
                if (!nodeMap.TryGetValue(edgeData.InputNodeGuid, out inputNode)) continue;

                Port outputPort = outputNode.Query<Port>().ToList()
                    .Find(p => p.direction == Direction.Output && p.portName == edgeData.OutputPortName);
                Port inputPort = inputNode.Query<Port>().ToList()
                    .Find(p => p.direction == Direction.Input && p.portName == edgeData.InputPortName);
                if (outputPort == null || inputPort == null) continue;

                graphView.AddElement(outputPort.ConnectTo(inputPort));
            }
        }
        finally
        {
            graphView.IsLoading = false;
        }
    }

    // NodeParamEntryは編集中のライブなオブジェクトなので、保存データ側は中身までコピーして独立させる。
    private static List<NodeParamEntry> CloneParams(List<NodeParamEntry> source)
    {
        return source.Select(p => new NodeParamEntry { Key = p.Key, Value = p.Value }).ToList();
    }

    private static Type ResolveMemberType(VisualScriptingGraphData data, string memberName)
    {
        MemberVariableData member = data.Members.Find(m => m.Name == memberName);
        if (member == null || string.IsNullOrEmpty(member.TypeName)) return null;
        return Type.GetType(member.TypeName);
    }

    private static Type ResolveSourceType(string sourceTypeName)
    {
        return string.IsNullOrEmpty(sourceTypeName) ? null : Type.GetType(sourceTypeName);
    }
}
