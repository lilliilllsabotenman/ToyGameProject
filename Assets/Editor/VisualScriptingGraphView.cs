// Editor/VisualScriptingGraphView.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class VisualScriptingGraphView : GraphView
{
    public const string GraphDataPath = "Assets/Resources/AnimationTransitionData/GraphData.asset";

    // 今このGraphViewが表示・編集しているパラメーター名。GraphSerializerはこれを読んで保存/読込対象を決める。
    public string CurrentParameterName { get; set; }

    // GraphSerializer.Loadが再構築中はtrue。この間はAutoSaveを走らせない(再構築途中の不完全な状態を保存してしまうのを防ぐ)。
    public bool IsLoading { get; set; }

    // 編集履歴。VisualScriptingEditorWindowが保持するインスタンスをここへ注入してもらう。  
    public EditHistoryData History { get; set; }    

    public VisualScriptingGraphView()
    {
        // ズーム・ドラッグ・選択を有効化
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        this.AddManipulator(new ContextualMenuManipulator(PopulateContextualMenu));

        this.RegisterCallback<ChangeEvent<string>>(_ => TriggerAutoSave());

        graphViewChanged = OnGraphViewChanged;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        TriggerAutoSave();
        return change;
    }

    private void TriggerAutoSave()
    {
        if (IsLoading) return;
        GraphSerializer.Save(this);
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
        VisualScriptingGraphData graphData = GraphSerializer.GetCurrent(this);

        foreach (Type sourceType in NodeMethodOptions.GetDerivedTypes<INodeActionSource>())
        {
            string sourceDisplayName = NodeMethodOptions.GetSourceDisplayName(sourceType);

            foreach (string actionName in NodeMethodOptions.GetMethodNames<VisualScriptingActionAttribute>(sourceType))
            {
                string actionDisplayName = NodeMethodOptions.GetDisplayName<VisualScriptingActionAttribute>(sourceType, actionName);
                evt.menu.AppendAction($"アクション追加/{sourceDisplayName}/{actionDisplayName}", _ => AddElement(new ActionNode(sourceType, actionName)));
            }

            foreach (string getterName in NodeMethodOptions.GetMethodNames<VisualScriptingGetter>(sourceType))
            {
                string getterDisplayName = NodeMethodOptions.GetDisplayName<VisualScriptingGetter>(sourceType, getterName);
                evt.menu.AppendAction($"取得追加/{sourceDisplayName}/{getterDisplayName}", _ => AddElement(new GetterNode(sourceType, getterName)));
            }
        }

        evt.menu.AppendAction("制御追加/もし〜なら(if)", _ => AddElement(new IfNode()));
        evt.menu.AppendAction("制御追加/繰り返し(for)", _ => AddElement(new ForNode()));

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
}