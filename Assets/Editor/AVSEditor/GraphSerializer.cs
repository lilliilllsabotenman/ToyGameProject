// Editor/GraphSerializer.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.Collections.Generic;

// VisualScriptingGraphDataBase アセットへの永続化(保存先の決定・検証・履歴)を担う。
// View⇔Dataの変換自体はGraphConverterの責務で、このクラスはそれをどう格納するかだけを扱う。
public static class GraphSerializer
{
    public static void Save(VisualScriptingGraphView graphView)
    {
        // 条件を満たさない場合だけ警告を出す。呼び出し側はok自体をif判定に使う(中断するかは呼び出し側が決める)。
        static bool CheckOrWarn(bool ok, string message)
        {
            if (!ok) Debug.LogWarning(message);
            return ok;
        }

        Debug.Log("Save");
        if (!CheckOrWarn(!string.IsNullOrEmpty(graphView.CurrentParameterName), "パラメーターが選択されていないため保存を中断しました。")) return;

        List<Edge> edgeList = graphView.edges.ToList();

        if (!CheckOrWarn(PortCompatibility.AreAllCompatible(edgeList, out List<string> incompatibleEdges),
            "型が一致しないエッジがあるため保存を中断しました: " + string.Join(", ", incompatibleEdges))) return;

        if (!CheckOrWarn(CycleDetector.HasNoDataCycles(edgeList, out List<string> dataCycles),
            "循環参照(データ配線)があるため保存を中断しました(実行時にクラッシュする可能性があります): " + string.Join(", ", dataCycles))) return;

        if (!CheckOrWarn(CycleDetector.HasNoExecCycles(edgeList, out List<string> execCycles),
            "循環参照(Exec配線)があるため保存を中断しました(実行時に無限ループでフリーズする可能性があります): " + string.Join(", ", execCycles))) return;

        VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
        VisualScriptingGraphData data = baseData.GetData(graphView.CurrentParameterName);
        if (!CheckOrWarn(data != null, "保存対象のデータが見つかりませんでした(先にパラメーターを選択してください)。")) return;

        VisualScriptingGraphData converted = GraphConverter.ToData(graphView, graphView.CurrentParameterName);
        data.Nodes = converted.Nodes;
        data.Edges = converted.Edges;

        EditorUtility.SetDirty(baseData);
        AssetDatabase.SaveAssets();

        CheckOrWarn(data.HasNoMissingParameters(out List<string> missing), "キー未入力のノードがあります: " + string.Join(", ", missing));

        graphView.History?.AddHistory(graphView.CurrentParameterName, BuildSnapshot(data));
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

    public static void Load(VisualScriptingGraphView graphView, AnimatorParameterInfo info)
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

        // このセッションで初めて開くパラメーターなら、今の状態を履歴の起点として積んでおく(でないと最初の編集がUndo不可になる)。
        if (graphView.History != null && graphView.History.GetData(info.Name) == null)
        {
            graphView.History.AddHistory(info.Name, BuildSnapshot(data));
        }

        GraphConverter.RebuildView(graphView, data);
    }

    // 履歴を1つ遡って復元する。これ以上遡れなければ何もしない。
    public static void Undo(VisualScriptingGraphView graphView)
    {
        if (string.IsNullOrEmpty(graphView.CurrentParameterName)) return;
        if (graphView.History == null) return;

        VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
        VisualScriptingGraphData data = baseData.GetData(graphView.CurrentParameterName);
        if (data == null) return;

        if (!graphView.History.Undo(graphView.CurrentParameterName, data)) return;

        EditorUtility.SetDirty(baseData);
        AssetDatabase.SaveAssets();

        GraphConverter.RebuildView(graphView, data);
    }

    // Undoで退避した内容を1つ戻す。退避が無ければ何もしない。
    public static void Redo(VisualScriptingGraphView graphView)
    {
        if (string.IsNullOrEmpty(graphView.CurrentParameterName)) return;
        if (graphView.History == null) return;

        VisualScriptingGraphDataBase baseData = LoadOrCreateBase();
        VisualScriptingGraphData data = baseData.GetData(graphView.CurrentParameterName);
        if (data == null) return;

        if (!graphView.History.Redo(graphView.CurrentParameterName, data)) return;

        EditorUtility.SetDirty(baseData);
        AssetDatabase.SaveAssets();

        GraphConverter.RebuildView(graphView, data);
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
        VisualScriptingGraphData data = new() { Name = parameterName, ParameterType = type };

        StartNodeData startNode = new StartNodeData
        {
            Guid = Guid.NewGuid().ToString(),
            Title = "Start",
            Position = new Rect(100, 200, 0, 0)
        };
        data.Nodes.Add(startNode);

        string methodKey = null;
        if (type == AnimatorControllerParameterType.Int) methodKey = "SetOwnerInteger";
        else if (type == AnimatorControllerParameterType.Float) methodKey = "SetOwnerFloat";
        else if (type == AnimatorControllerParameterType.Bool) methodKey = "SetOwnerBool";
        else if (type == AnimatorControllerParameterType.Trigger) methodKey = "SetOwnerTrigger";

        if (methodKey != null)
        {
            ActionNodeData setParameterNode = new ActionNodeData
            {
                Guid = Guid.NewGuid().ToString(),
                                               Title = methodKey,
                Position = new Rect(320, 200, 0, 0),
                MethodKey = methodKey,
                SourceTypeName = typeof(DefaultNode).AssemblyQualifiedName
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
}
