// Editor/VisualScriptingGraphView.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class VisualScriptingGraphView : GraphView
{
    private const string GraphDataPath = "Assets/Editor/GraphData/GraphData.asset";

    public VisualScriptingGraphView()
    {
        // ズーム・ドラッグ・選択を有効化
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // グリッド背景
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // 右クリックコンテキストメニュー
        this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

        // Startノードを最初から配置
        AddElement(CreateStartNode());
    }

    // 接続ルール：OutputからInputにのみ繋げる
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports
            .Where(p => p.direction != startPort.direction && p.node != startPort.node)
            .ToList();
    }

    private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        foreach (string actionName in NodeMethodOptions.GetActionNames())
        {
            evt.menu.AppendAction("Add Action/" + actionName, _ => AddNode(new ActionNode(actionName)));
        }

        foreach (string conditionName in NodeMethodOptions.GetConditionNames())
        {
            evt.menu.AppendAction("Add Condition/" + conditionName, _ => AddNode(new ConditionNode(conditionName)));
        }
    }

    private void AddNode(BaseNode node)
    {
        AddElement(node);
    }

    private StartNode CreateStartNode()
    {
        StartNode node = new StartNode();
        node.SetPosition(new UnityEngine.Rect(100, 200, 0, 0));
        return node;
    }

#region セーブ＆ロード

    public void SaveGraph()
    {
        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(GraphDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<VisualScriptingGraphData>();
            AssetDatabase.CreateAsset(data, GraphDataPath);
        }

        data.Nodes.Clear();
        data.Edges.Clear();

        foreach (BaseNode node in nodes.ToList().Cast<BaseNode>())
        {
            string parameter = string.Empty;
            if (node is ActionNode actionNode) parameter = actionNode.ActionKey;
            if (node is ConditionNode conditionNode) parameter = conditionNode.ConditionKey;

            data.Nodes.Add(new NodeData
            {
                Guid = node.NodeGuid,
                Type = node.GetType().Name,
                Title = node.title,
                Position = node.GetPosition(),
                Parameter = parameter
            });
        }


        foreach (Edge edge in edges.ToList())
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

    // Action/Conditionノードでキー(Parameter)が空のまま保存された場合に警告する。
    // 実行時のDictionaryキー不一致(未登録キー)はここでは検出できない(実行側の登録内容をEditorは知らないため)。
    private void WarnMissingParameters(VisualScriptingGraphData data)
    {
        List<string> missing = new List<string>();
        foreach (NodeData nodeData in data.Nodes)
        {
            bool needsParameter = nodeData.Type == nameof(ActionNode) || nodeData.Type == nameof(ConditionNode);
            if (needsParameter && string.IsNullOrWhiteSpace(nodeData.Parameter))
            {
                missing.Add(nodeData.Type + " (" + nodeData.Guid.Substring(0, 8) + ")");
            }
        }

        if (missing.Count > 0)
        {
            Debug.LogWarning("キー未入力のノードがあります: " + string.Join(", ", missing));
        }
    }

    public void LoadGraph()
    {
        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(GraphDataPath);
        if (data == null) return;

        DeleteElements(graphElements.ToList());

        Dictionary<string, BaseNode> nodeMap = new Dictionary<string, BaseNode>();

        foreach (NodeData nodeData in data.Nodes)
        {
            BaseNode node = nodeData.Type switch
            {
                nameof(StartNode) => new StartNode(),
                nameof(ActionNode) => new ActionNode(),
                nameof(ConditionNode) => new ConditionNode(),
                _ => null
            };
            if (node == null) continue;

            node.NodeGuid = nodeData.Guid;
            node.SetPosition(nodeData.Position);
            if (node is ActionNode actionNode) actionNode.ActionKey = nodeData.Parameter;
            if (node is ConditionNode conditionNode) conditionNode.ConditionKey = nodeData.Parameter;
            AddElement(node);
            nodeMap[nodeData.Guid] = node;
        }

        foreach (EdgeData edgeData in data.Edges)
        {
            BaseNode outputNode;
            BaseNode inputNode;
            if (!nodeMap.TryGetValue(edgeData.OutputNodeGuid, out outputNode)) continue;
            if (!nodeMap.TryGetValue(edgeData.InputNodeGuid, out inputNode)) continue;

            Port outputPort = outputNode.outputContainer.Query<Port>().ToList()
                .Find(p => p.portName == edgeData.OutputPortName);
            Port inputPort = inputNode.inputContainer.Query<Port>().ToList()
                .Find(p => p.portName == edgeData.InputPortName);
            if (outputPort == null || inputPort == null) continue;

            AddElement(outputPort.ConnectTo(inputPort));
        }
    }
}

#endregion