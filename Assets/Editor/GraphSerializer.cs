// Editor/GraphSerializer.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

// GraphView上のノード/エッジ ⇔ VisualScriptingGraphDataアセット の変換を担う。
// 状態を持たないstaticクラス(呼び出しごとにGraphViewの参照を渡してもらう)。
public static class GraphSerializer
{
    public static void Save(IEnumerable<Node> nodes, IEnumerable<Edge> edges)
    {
        List<Edge> edgeList = edges.ToList();

        if (!PortCompatibility.AreAllCompatible(edgeList, out List<string> incompatibleEdges))
        {
            Debug.LogWarning("型が一致しないエッジがあるため保存を中断しました: " + string.Join(", ", incompatibleEdges));
            return;
        }

        if (!CycleDetector.HasNoCycles(edgeList, out List<string> cycles))
        {
            Debug.LogWarning("循環参照があるため保存を中断しました(実行時にクラッシュする可能性があります): " + string.Join(", ", cycles));
            return;
        }

        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(VisualScriptingGraphView.GraphDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<VisualScriptingGraphData>();
            AssetDatabase.CreateAsset(data, VisualScriptingGraphView.GraphDataPath);
        }

        data.Nodes.Clear();
        data.Edges.Clear();

        foreach (BaseNode node in nodes.ToList().Cast<BaseNode>())
        {
            BaseNodeData nodeData = node switch
            {
                ActionNode actionNode => new ActionNodeData { MethodKey = actionNode.ActionKey, Params = actionNode.Params },
                ConditionNode conditionNode => new ConditionNodeData { MethodKey = conditionNode.ConditionKey, Params = conditionNode.Params },
                GetterNode getterNode => new ActionNodeData { MethodKey = getterNode.ActionKey, Params = getterNode.Params },
                GetMemberNode getMemberNode => new GetMemberNodeData { MemberName = getMemberNode.MemberName },
                SetMemberNode setMemberNode => new SetMemberNodeData { MemberName = setMemberNode.MemberName },
                StartNode => new StartNodeData(),
                _ => null
            };
            if (nodeData == null) continue;

            nodeData.Guid = node.NodeGuid;
            nodeData.Title = node.title;
            nodeData.Position = node.GetPosition();
            data.Nodes.Add(nodeData);
        }

        foreach (Edge edge in edgeList)
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

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        WarnMissingParameters(data);
    }

    public static void Load(VisualScriptingGraphView graphView)
    {
        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(VisualScriptingGraphView.GraphDataPath);
        if (data == null) return;

        Type sourceType = !string.IsNullOrEmpty(data.TargetTypeName) ? Type.GetType(data.TargetTypeName) : null;

        graphView.DeleteElements(graphView.graphElements.ToList());

        Dictionary<string, BaseNode> nodeMap = new Dictionary<string, BaseNode>();

        foreach (BaseNodeData nodeData in data.Nodes)
        {
            BaseNode node = nodeData switch
            {
                StartNodeData => new StartNode(),
                ActionNodeData actionData when NodeMethodOptions.IsGetter(sourceType, actionData.MethodKey) => new GetterNode(
                    actionData.MethodKey,
                    NodeMethodOptions.GetMethodParams(sourceType, actionData.MethodKey),
                    NodeMethodOptions.GetReturnType(sourceType, actionData.MethodKey),
                    actionData.Params,
                    NodeMethodOptions.GetDisplayName(sourceType, actionData.MethodKey)),
                ActionNodeData actionData => new ActionNode(
                    actionData.MethodKey,
                    NodeMethodOptions.GetMethodParams(sourceType, actionData.MethodKey),
                    NodeMethodOptions.GetReturnType(sourceType, actionData.MethodKey),
                    actionData.Params,
                    NodeMethodOptions.GetActionDisplayName(sourceType, actionData.MethodKey)),
                ConditionNodeData conditionData => new ConditionNode(
                    conditionData.MethodKey,
                    NodeMethodOptions.GetMethodParams(sourceType, conditionData.MethodKey),
                    conditionData.Params,
                    NodeMethodOptions.GetConditionDisplayName(sourceType, conditionData.MethodKey)),
                GetMemberNodeData getMemberData => new GetMemberNode(getMemberData.MemberName, ResolveMemberType(data, getMemberData.MemberName)),
                SetMemberNodeData setMemberData => new SetMemberNode(setMemberData.MemberName, ResolveMemberType(data, setMemberData.MemberName)),
                _ => null
            };
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

    private static Type ResolveMemberType(VisualScriptingGraphData data, string memberName)
    {
        MemberVariableData member = data.Members.Find(m => m.Name == memberName);
        if (member == null || string.IsNullOrEmpty(member.TypeName)) return null;
        return Type.GetType(member.TypeName);
    }

    // Action/Conditionノードでキー(MethodKey)が空のまま保存された場合に警告する。
    // 実行時のDictionaryキー不一致(未登録キー)はここでは検出できない(実行側の登録内容をEditorは知らないため)。
    private static void WarnMissingParameters(VisualScriptingGraphData data)
    {
        List<string> missing = new List<string>();
        foreach (BaseNodeData nodeData in data.Nodes)
        {
            bool isMissing = nodeData switch
            {
                ActionNodeData actionData => string.IsNullOrWhiteSpace(actionData.MethodKey),
                ConditionNodeData conditionData => string.IsNullOrWhiteSpace(conditionData.MethodKey),
                _ => false
            };
            if (isMissing)
            {
                missing.Add(nodeData.GetType().Name + " (" + nodeData.Guid.Substring(0, 8) + ")");
            }
        }

        if (missing.Count > 0)
        {
            Debug.LogWarning("キー未入力のノードがあります: " + string.Join(", ", missing));
        }
    }
}
