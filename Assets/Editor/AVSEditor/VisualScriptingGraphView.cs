// Editor/VisualScriptingGraphView.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class VisualScriptingGraphView : GraphView
{
    public const string GraphDataPath = "Assets/Resources/AnimationTransitionData/GraphData.asset";

    // 今このGraphViewが編集している対象そのもの(名前検索を介さない直接参照)。
    // CurrentDataBaseは保存(SetDirty/SaveAssets)対象のアセット、CurrentDataはその中の1エントリ。
    public VisualScriptingGraphDataBase CurrentDataBase { get; set; }
    public VisualScriptingGraphData CurrentData { get; set; }

    // Data(Data⇔Viewの再構築)が進行中はtrue。この間はAutoSaveを走らせない(再構築途中の不完全な状態を保存してしまうのを防ぐ)。
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

    // CurrentData/CurrentDataBaseが設定されていれば、検証→View内容をCurrentDataへ反映→アセット保存→履歴追加、まで行う。
    // 未設定(何も編集対象を持っていない)なら何もしない。
    public void TriggerAutoSave()
    {
        if (IsLoading) return;
        if (CurrentData == null || CurrentDataBase == null) return;

        // 条件を満たさない場合だけ警告を出す。呼び出し側はok自体をif判定に使う(中断するかは呼び出し側が決める)。
        static bool CheckOrWarn(bool ok, string message)
        {
            if (!ok) Debug.LogWarning(message);
            return ok;
        }

        List<Edge> edgeList = edges.ToList();

        if (!CheckOrWarn(PortCompatibility.AreAllCompatible(edgeList, out List<string> incompatibleEdges),
            "型が一致しないエッジがあるため保存を中断しました: " + string.Join(", ", incompatibleEdges))) return;

        if (!CheckOrWarn(CycleDetector.HasNoDataCycles(edgeList, out List<string> dataCycles),
            "循環参照(データ配線)があるため保存を中断しました(実行時にクラッシュする可能性があります): " + string.Join(", ", dataCycles))) return;

        if (!CheckOrWarn(CycleDetector.HasNoExecCycles(edgeList, out List<string> execCycles),
            "循環参照(Exec配線)があるため保存を中断しました(実行時に無限ループでフリーズする可能性があります): " + string.Join(", ", execCycles))) return;

        VisualScriptingGraphData converted = GraphConverter.ToData(this, CurrentData.Name);
        CurrentData.Nodes = converted.Nodes;
        CurrentData.Edges = converted.Edges;

        EditorUtility.SetDirty(CurrentDataBase);
        AssetDatabase.SaveAssets();

        CheckOrWarn(CurrentData.HasNoMissingParameters(out List<string> missing), "キー未入力のノードがあります: " + string.Join(", ", missing));

        History?.AddHistory(CurrentData.Name, BuildSnapshot(CurrentData));
    }

    // 履歴を1つ遡って復元する。これ以上遡れなければ何もしない。
    public void Undo()
    {
        if (CurrentData == null || History == null) return;
        if (!History.Undo(CurrentData.Name, CurrentData)) return;

        EditorUtility.SetDirty(CurrentDataBase);
        AssetDatabase.SaveAssets();

        GraphConverter.RebuildView(this, CurrentData);
    }

    // Undoで退避した内容を1つ戻す。退避が無ければ何もしない。
    public void Redo()
    {
        if (CurrentData == null || History == null) return;
        if (!History.Redo(CurrentData.Name, CurrentData)) return;

        EditorUtility.SetDirty(CurrentDataBase);
        AssetDatabase.SaveAssets();

        GraphConverter.RebuildView(this, CurrentData);
    }

    // Nodes/Edges中身は毎回新規生成されたオブジェクトなので、入れ物(List)だけ別に包めば安全なスナップショットになる。
    private static VisualScriptingGraphData BuildSnapshot(VisualScriptingGraphData data)
    {
        return new()
        {
            Name = data.Name,
            ParameterType = data.ParameterType,
            Nodes = new List<BaseNodeData>(data.Nodes),
            Edges = new List<EdgeData>(data.Edges),
            Members = new List<MemberVariableData>(data.Members)
        };
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
        VisualScriptingGraphData graphData = CurrentData;

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
