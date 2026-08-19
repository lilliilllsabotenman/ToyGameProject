// Editor/VisualScriptingGraphView.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

        // グリッド背景
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // 右クリックコンテキストメニュー
        this.AddManipulator(new ContextualMenuManipulator(PopulateContextualMenu));

        // Startノードを最初から配置
        AddElement(CreateStartNode());

        // AutoSave: ノード生成・削除・移動、エッジ接続・解消のたびに保存
        graphViewChanged = OnGraphViewChanged;
        // AutoSave: テキストフィールドの編集完了(isDelayed)のたびに保存。ChangeEventはバブリングするので子ノード側の登録は不要。
        this.RegisterCallback<ChangeEvent<string>>(_ => TriggerAutoSave());
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
            string sourceTypeName = sourceType.AssemblyQualifiedName;
            string sourceDisplayName = NodeMethodOptions.GetSourceDisplayName(sourceType);

            foreach (string actionName in NodeMethodOptions.GetMethodNames<VisualScriptingActionAttribute>(sourceType))
            {
                string actionDisplayName = NodeMethodOptions.GetDisplayName<VisualScriptingActionAttribute>(sourceType, actionName);
                evt.menu.AppendAction($"アクション追加/{sourceDisplayName}/{actionDisplayName}", _ => AddElement(new ActionNode(actionName, NodeMethodOptions.GetMethodParams(sourceType, actionName), NodeMethodOptions.GetReturnType(sourceType, actionName), displayName: actionDisplayName, sourceTypeName: sourceTypeName)));
            }

            foreach (string conditionName in NodeMethodOptions.GetMethodNames<VisualScriptingConditionAttribute>(sourceType))
            {
                string conditionDisplayName = NodeMethodOptions.GetDisplayName<VisualScriptingConditionAttribute>(sourceType, conditionName);
                evt.menu.AppendAction($"条件追加/{sourceDisplayName}/{conditionDisplayName}", _ => AddElement(new ConditionNode(conditionName, NodeMethodOptions.GetMethodParams(sourceType, conditionName), displayName: conditionDisplayName, sourceTypeName: sourceTypeName)));
            }

            foreach (string getterName in NodeMethodOptions.GetMethodNames<VisualScriptingGetter>(sourceType))
            {
                string getterDisplayName = NodeMethodOptions.GetDisplayName<VisualScriptingGetter>(sourceType, getterName);
                evt.menu.AppendAction($"取得追加/{sourceDisplayName}/{getterDisplayName}", _ => AddElement(new GetterNode(getterName, NodeMethodOptions.GetMethodParams(sourceType, getterName), NodeMethodOptions.GetReturnType(sourceType, getterName), displayName: getterDisplayName, sourceTypeName: sourceTypeName)));
            }
        }

        evt.menu.AppendAction("制御追加/繰り返し(For)", _ => AddElement(new ForNode()));

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