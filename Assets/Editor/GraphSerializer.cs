// Editor/GraphSerializer.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

// GraphView上のノード/エッジ ⇔ VisualScriptingGraphData の変換を担う。
// 保存の実体は1個の VisualScriptingGraphDataBase アセットで、GraphView.CurrentParameterNameで個々のグラフを引く。
// このクラス自体は状態を持たない(「今何を編集中か」はGraphView側が持つ)。
public static class GraphSerializer
{
    public static void Save(VisualScriptingGraphView graphView)
    {

        Debug.Log("Save");
        if (string.IsNullOrEmpty(graphView.CurrentParameterName))
        {
            Debug.LogWarning("パラメーターが選択されていないため保存を中断しました。");
            return;
        }

        List<Edge> edgeList = graphView.edges.ToList();

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

        VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
        VisualScriptingGraphData data = baseData.GetData(graphView.CurrentParameterName);
        if (data == null)
        {
            Debug.LogWarning("保存対象のデータが見つかりませんでした(先にパラメーターを選択してください)。");
            return;
        }

        data.Nodes.Clear();
        data.Edges.Clear();

        foreach (BaseNode node in graphView.nodes.ToList().Cast<BaseNode>())
        {
            BaseNodeData nodeData = node switch
            {
                ActionNode actionNode => new ActionNodeData { MethodKey = actionNode.ActionKey, Params = CloneParams(actionNode.Params) },
                ConditionNode conditionNode => new ConditionNodeData { MethodKey = conditionNode.ConditionKey, Params = CloneParams(conditionNode.Params) },
                GetterNode getterNode => new ActionNodeData { MethodKey = getterNode.ActionKey, Params = CloneParams(getterNode.Params) },
                GetMemberNode getMemberNode => new GetMemberNodeData { MemberName = getMemberNode.MemberName },
                SetMemberNode setMemberNode => new SetMemberNodeData { MemberName = setMemberNode.MemberName },
                ForNode forNode => new ForNodeData { Params = CloneParams(forNode.Params) },
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

        EditorUtility.SetDirty(baseData);
        AssetDatabase.SaveAssets();

        WarnMissingParameters(data);

        // Nodes/Edges中身は今回のSaveで新規生成されたオブジェクトなので、入れ物(List)だけ別に包めば安全なスナップショットになる。
        VisualScriptingGraphData snapshot = new()
        {
            Name = data.Name,
            ParameterType = data.ParameterType,
            TargetTypeName = data.TargetTypeName,
            Nodes = new List<BaseNodeData>(data.Nodes),
            Edges = new List<EdgeData>(data.Edges),
            Members = new List<MemberVariableData>(data.Members)
        };
        graphView.History?.AddHistory(graphView.CurrentParameterName, snapshot);
    }

    // NodeParamEntryは編集中のライブなオブジェクトなので、保存データ側は中身までコピーして独立させる。
    private static List<NodeParamEntry> CloneParams(List<NodeParamEntry> source)
    {
        return source.Select(p => new NodeParamEntry { Key = p.Key, Value = p.Value }).ToList();
    }

    public static void Load(VisualScriptingGraphView graphView, AnimatorParameterInfo info)
    {
        graphView.IsLoading = true;
        try
        {
            VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
            VisualScriptingGraphData data = baseData.GetData(info.Name);
            if (data == null)
            {
                data = CreateNewEntry(info.Name, info.Type);
                baseData.data.Add(data);
                EditorUtility.SetDirty(baseData);
                AssetDatabase.SaveAssets();
            }

            graphView.CurrentParameterName = info.Name;

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
                    ForNodeData forData => new ForNode(forData.Params),
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
        finally
        {
            graphView.IsLoading = false;
        }
    }

    // 今GraphViewが指しているパラメーターのデータを返す(未選択、またはまだLoadされていなければnull)。
    // 新規作成はLoadだけの責務(実パラメーターの型を持っているのはLoadだけのため)。
    public static VisualScriptingGraphData GetCurrent(VisualScriptingGraphView graphView)
    {
        if (string.IsNullOrEmpty(graphView.CurrentParameterName)) return null;
        return LoadOrCreateBase().GetData(graphView.CurrentParameterName);
    }

    // GetCurrentで取得したデータを外部で書き換えた後、変更をディスクへ反映するために呼ぶ。
    public static void PersistCurrent()
    {
        VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
        EditorUtility.SetDirty(baseData);
        AssetDatabase.SaveAssets();
    }

    private static VisualScriptingGraphDataBase LoadOrCreateBase()
    {
        VisualScriptingGraphDataBase baseData = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(VisualScriptingGraphView.GraphDataPath);
        if (baseData == null)
        {
            baseData = ScriptableObject.CreateInstance<VisualScriptingGraphDataBase>();
            AssetDatabase.CreateAsset(baseData, VisualScriptingGraphView.GraphDataPath);
        }
        return baseData;
    }

    // 新規パラメーター用のデータを作る。Startノードだけ最初から入れておく(ないとLoad時に消える)。
    private static VisualScriptingGraphData CreateNewEntry(string parameterName, AnimatorControllerParameterType type)
    {
        VisualScriptingGraphData data = new() { Name = parameterName, ParameterType = type, TargetTypeName = typeof(DefaultNode).AssemblyQualifiedName };

        StartNodeData startNode = new StartNodeData
        {
            Guid = Guid.NewGuid().ToString(),
            Title = "Start",
            Position = new Rect(100, 200, 0, 0)
        };
        data.Nodes.Add(startNode);

        string methodKey = type switch
        {
            AnimatorControllerParameterType.Int => "SetOwnerInteger",
            AnimatorControllerParameterType.Float => "SetOwnerFloat",
            AnimatorControllerParameterType.Bool => "SetOwnerBool",
            AnimatorControllerParameterType.Trigger => "SetOwnerTrigger",
            _ => null
        };

        if (methodKey != null)
        {
            ActionNodeData setParameterNode = new ActionNodeData
            {
                Guid = Guid.NewGuid().ToString(),
                                               Title = methodKey,
                Position = new Rect(320, 200, 0, 0),
                MethodKey = methodKey
            };
            setParameterNode.SetParam("name", parameterName);
            data.Nodes.Add(setParameterNode);

            data.Edges.Add(new EdgeData
            {
                OutputNodeGuid = startNode.Guid,
                OutputPortName = "Out",
                InputNodeGuid = setParameterNode.Guid,
                InputPortName = "In"
            });
        }

        return data;
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
