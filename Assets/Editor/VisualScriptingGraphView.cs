// Editor/VisualScriptingGraphView.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class VisualScriptingGraphView : GraphView
{
    public const string GraphDataPath = "Assets/Editor/GraphData/GraphData.asset";

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
        this.AddManipulator(new ContextualMenuManipulator(PopulateContextualMenu));

        // Startノードを最初から配置
        AddElement(CreateStartNode());
    }

    // 接続ルール：OutputからInputにのみ繋げる
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports
            .Where(p => p.direction != startPort.direction && p.node != startPort.node && PortCompatibility.IsCompatible(startPort, p))
            .ToList();
    }
    
    private void PopulateContextualMenu(ContextualMenuPopulateEvent evt)
    {
        VisualScriptingGraphData graphData = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(GraphDataPath);
        Type sourceType = graphData != null && !string.IsNullOrEmpty(graphData.TargetTypeName)
            ? Type.GetType(graphData.TargetTypeName)
            : null;

        foreach (string actionName in NodeMethodOptions.GetActionNames(sourceType))
        {
            string actionDisplayName = NodeMethodOptions.GetActionDisplayName(sourceType, actionName);
            evt.menu.AppendAction("アクション追加/" + actionDisplayName, _ => AddElement(new ActionNode(actionName, NodeMethodOptions.GetMethodParams(sourceType, actionName), NodeMethodOptions.GetReturnType(sourceType, actionName), displayName: actionDisplayName)));
        }

        foreach (string conditionName in NodeMethodOptions.GetConditionNames(sourceType))
        {
            string conditionDisplayName = NodeMethodOptions.GetConditionDisplayName(sourceType, conditionName);
            evt.menu.AppendAction("条件追加/" + conditionDisplayName, _ => AddElement(new ConditionNode(conditionName, NodeMethodOptions.GetMethodParams(sourceType, conditionName), displayName: conditionDisplayName)));
        }

        foreach (string getterName in NodeMethodOptions.GetGetterNames(sourceType))
        {
            string getterDisplayName = NodeMethodOptions.GetDisplayName(sourceType, getterName);
            evt.menu.AppendAction("取得追加/" + getterDisplayName, _ => AddElement(new GetterNode(getterName, NodeMethodOptions.GetMethodParams(sourceType, getterName), NodeMethodOptions.GetReturnType(sourceType, getterName), displayName: getterDisplayName)));
        }

        if (graphData != null)
        {
            foreach (MemberVariableData member in graphData.Members)
            {
                string memberName = member.Name;
                Type memberType = string.IsNullOrEmpty(member.TypeName) ? null : Type.GetType(member.TypeName);
                evt.menu.AppendAction("メンバー追加/取得: " + memberName, _ => AddElement(new GetMemberNode(memberName, memberType)));
                evt.menu.AppendAction("メンバー追加/設定: " + memberName, _ => AddElement(new SetMemberNode(memberName, memberType)));
            }
        }
    }

    private StartNode CreateStartNode()
    {
        StartNode node = new StartNode();
        node.SetPosition(new UnityEngine.Rect(100, 200, 0, 0));
        return node;
    }
}