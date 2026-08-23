// Editor/PresetSerializer.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// プリセット専用の永続化を担う。GraphSerializerと同型だが、保存先アセット(PresetDataPath)が別で、
// パラメーターグラフ用のGraphData.assetとは完全に独立した読み書き導線になっている。
// View⇔Dataの変換自体はGraphConverterに委譲する。
public static class PresetSerializer
{
    public const string PresetDataPath = "Assets/Resources/AnimationTransitionData/PresetData.asset";

    // 名前でプリセットを引く。見つからなければnull。
    public static VisualScriptingGraphData GetPreset(string presetName)
    {
        return LoadOrCreatePresetBase().GetData(presetName);
    }

    // 名前で引いたプリセットの内容でgraphViewを再構築する。見つからなければ何もしない。
    public static void LoadPreset(VisualScriptingGraphView graphView, string presetName)
    {
        VisualScriptingGraphData data = GetPreset(presetName);
        if (data == null)
        {
            Debug.LogWarning("プリセットが見つかりませんでした: " + presetName);
            return;
        }

        GraphConverter.RebuildView(graphView, data);
    }

    // 現在graphViewが表示している内容を、presetNameという名前のプリセットとして保存する(新規なら追加、既存なら上書き)。
    public static void SaveAsPreset(VisualScriptingGraphView graphView, string presetName)
    {
        // 条件を満たさない場合だけ警告を出す。呼び出し側はok自体をif判定に使う(中断するかは呼び出し側が決める)。
        static bool CheckOrWarn(bool ok, string message)
        {
            if (!ok) Debug.LogWarning(message);
            return ok;
        }

        if (!CheckOrWarn(!string.IsNullOrEmpty(presetName), "プリセット名が指定されていないため保存を中断しました。")) return;

        List<Edge> edgeList = graphView.edges.ToList();

        if (!CheckOrWarn(PortCompatibility.AreAllCompatible(edgeList, out List<string> incompatibleEdges),
            "型が一致しないエッジがあるため保存を中断しました: " + string.Join(", ", incompatibleEdges))) return;

        if (!CheckOrWarn(CycleDetector.HasNoDataCycles(edgeList, out List<string> dataCycles),
            "循環参照(データ配線)があるため保存を中断しました(実行時にクラッシュする可能性があります): " + string.Join(", ", dataCycles))) return;

        if (!CheckOrWarn(CycleDetector.HasNoExecCycles(edgeList, out List<string> execCycles),
            "循環参照(Exec配線)があるため保存を中断しました(実行時に無限ループでフリーズする可能性があります): " + string.Join(", ", execCycles))) return;

        VisualScriptingGraphDataBase presetBase = LoadOrCreatePresetBase();
        VisualScriptingGraphData converted = GraphConverter.ToData(graphView, presetName);

        VisualScriptingGraphData existing = presetBase.GetData(presetName);
        if (existing != null)
        {
            existing.Nodes = converted.Nodes;
            existing.Edges = converted.Edges;
        }
        else
        {
            presetBase.data.Add(converted);
        }

        EditorUtility.SetDirty(presetBase);
        AssetDatabase.SaveAssets();

        CheckOrWarn(converted.HasNoMissingParameters(out List<string> missing), "キー未入力のノードがあります: " + string.Join(", ", missing));
    }

    // 動作確認用: AnimatorControllerParameterTypeごとに空のプリセットを1個ずつ作り、DisplayName/Colorも設定する。
    // 既存分は上書きするので、何度実行しても最新のテスト内容に揃う。
    [MenuItem("Tools/AVS/Generate Test Presets")]
    private static void GenerateTestPresets()
    {
        VisualScriptingGraphDataBase presetBase = LoadOrCreatePresetBase();

        foreach (AnimatorControllerParameterType type in Enum.GetValues(typeof(AnimatorControllerParameterType)))
        {
            string name = "Test" + type;

            VisualScriptingGraphData data = presetBase.GetData(name);
            if (data == null)
            {
                data = new VisualScriptingGraphData { Name = name, ParameterType = type };
                presetBase.data.Add(data);
            }

            Color color = type switch
            {
                AnimatorControllerParameterType.Float => Color.red,
                AnimatorControllerParameterType.Int => Color.green,
                AnimatorControllerParameterType.Bool => Color.cyan,
                AnimatorControllerParameterType.Trigger => Color.yellow,
                _ => Color.white
            };

            IPresetDisplayInfoWriter writer = data;
            writer.SetDisplayName(name + "の表示名");
            writer.SetColor(color);
        }

        EditorUtility.SetDirty(presetBase);
        AssetDatabase.SaveAssets();
    }

    private static VisualScriptingGraphDataBase LoadOrCreatePresetBase()
    {
        VisualScriptingGraphDataBase presetBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(PresetDataPath);
        if (presetBase == null)
        {
            presetBase = ScriptableObject.CreateInstance<VisualScriptingGraphDataBase>();
            AssetDatabase.CreateAsset(presetBase, PresetDataPath);
        }
        return presetBase;
    }
}
