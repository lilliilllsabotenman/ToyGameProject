// Editor/PresetSerializer.cs
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

// プリセット専用アセット群(UserPreset.asset/DefaultPreset.assetなど、DataStateで種別分けされた
// VisualScriptingGraphDataBase)の検索・生成だけを担う。
// 保存(検証・View⇔Data反映・永続化)はVisualScriptingGraphView.TriggerAutoSaveの責務。
public static class PresetSerializer
{
    public const string PresetFolder = "Assets/Resources/AnimationTransitionData";
    public const string UserPresetPath = PresetFolder + "/UserPreset.asset";

    // 新規プリセットの書き込み先。無ければDataState=UserPresetで新規作成する。
    public static VisualScriptingGraphDataBase LoadOrCreateUserPresetBase()
    {
        VisualScriptingGraphDataBase presetBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(UserPresetPath);
        if (presetBase == null)
        {
            // 実ファイルはあるのにAssetDatabase側がまだ拾えていないだけ(コンパイル直後など)の可能性があるため、
            // ここでCreateAssetすると中身を空で上書きしてしまう。まず再インポートして取り直す。
            if (System.IO.File.Exists(UserPresetPath))
            {
                AssetDatabase.ImportAsset(UserPresetPath);
                presetBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(UserPresetPath);
            }

            if (presetBase == null)
            {
                presetBase = ScriptableObject.CreateInstance<VisualScriptingGraphDataBase>();
                presetBase.DataState = DataState.UserPreset;
                AssetDatabase.CreateAsset(presetBase, UserPresetPath);
            }
        }
        return presetBase;
    }

    // 実行時に適用されるデータ(GraphData.asset/DataState=ExecuteData)の読み込み先。
    // 無ければ新規作成する。UserPreset側と同じく、実ファイルはあるがAssetDatabase未反映のケースを
    // 先に再インポートで拾ってから、それでも無ければ新規作成する。
    public static VisualScriptingGraphDataBase LoadOrCreateExecuteDataBase()
    {
        string path = VisualScriptingGraphView.GraphDataPath;
        VisualScriptingGraphDataBase executeDataBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(path);
        if (executeDataBase == null)
        {
            if (System.IO.File.Exists(path))
            {
                AssetDatabase.ImportAsset(path);
                executeDataBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(path);
            }

            if (executeDataBase == null)
            {
                executeDataBase = ScriptableObject.CreateInstance<VisualScriptingGraphDataBase>();
                executeDataBase.DataState = DataState.ExecuteData;
                AssetDatabase.CreateAsset(executeDataBase, path);
            }
        }
        return executeDataBase;
    }

    // PresetFolder配下の全VisualScriptingGraphDataBaseのうち、DataStateがUserPreset/DefaultPresetのものだけを返す
    // (ExecuteData=GraphData.asset相当は除外)。AssetDatabase.FindAssets(検索インデックス依存)は
    // 外部でのファイル操作直後などにインデックスが古いまま0件を返すことがあったため、
    // フォルダを直接列挙する方式に変更した。読み込みはLoadAssetAtPathのみで完結する。
    public static List<VisualScriptingGraphDataBase> FindAllPresetBases()
    {
        List<VisualScriptingGraphDataBase> result = new List<VisualScriptingGraphDataBase>();

        if (!System.IO.Directory.Exists(PresetFolder)) return result;

        string[] files = System.IO.Directory.GetFiles(PresetFolder, "*.asset");
        foreach (string file in files)
        {
            string path = file.Replace('\\', '/');
            VisualScriptingGraphDataBase presetBase = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphDataBase>(path);
            if (presetBase == null) continue;
            if (presetBase.DataState != DataState.UserPreset && presetBase.DataState != DataState.DefaultPreset) continue;

            result.Add(presetBase);
        }

        return result;
    }

    // 名前でプリセットを引く(UserPreset/DefaultPresetの全アセットを横断)。見つからなければnull。
    public static VisualScriptingGraphData GetPreset(string presetName)
    {
        foreach (VisualScriptingGraphDataBase presetBase in FindAllPresetBases())
        {
            VisualScriptingGraphData found = presetBase.GetData(presetName);
            if (found != null) return found;
        }

        return null;
    }

    // 同名の既存プリセットがあるかは見ずに、常にUserPresetアセットへ新規作成する。
    // StartNode→SetOwnerXアクション(info.Typeに対応するもの)が繋がった状態で配置し、
    // アクションのnameパラメータもinfo.Nameで初期化しておく。
    // data.Name(検索キー)はinfo.Name、DisplayName/ColorはdisplayInfoから設定する。
    public static VisualScriptingGraphData CreatePreset(
        AnimatorParameterInfo info,
        PresetDisplayInfo displayInfo)
    {
        VisualScriptingGraphDataBase presetBase = LoadOrCreateUserPresetBase();

        VisualScriptingGraphData data = new VisualScriptingGraphData { Name = info.Name, ParameterType = info.Type, PresetId = Guid.NewGuid().ToString() };
        ((IPresetDisplayInfoWriter)data).SetDisplayInfo(displayInfo);

        StartNodeData startNode = new StartNodeData
        {
            Guid = Guid.NewGuid().ToString(),
            Title = "開始",
            Position = new Rect(100, 200, 0, 0)
        };

        ActionNodeData actionNode = new ActionNodeData
        {
            Guid = Guid.NewGuid().ToString(),
            Title = PresetParameterBinder.GetMethodKey(info.Type),
            Position = new Rect(320, 200, 0, 0),
            MethodKey = PresetParameterBinder.GetMethodKey(info.Type),
            SourceTypeName = typeof(DefaultNode).AssemblyQualifiedName
        };

        data.Nodes.Add(startNode);
        data.Nodes.Add(actionNode);
        data.Edges.Add(new EdgeData
        {
            OutputNodeGuid = startNode.Guid,
            OutputPortName = "Out",
            InputNodeGuid = actionNode.Guid,
            InputPortName = "In"
        });

        PresetParameterBinder.BindParameterName(data, info.Type, info.Name);

        presetBase.data.Add(data);
        EditorUtility.SetDirty(presetBase);
        AssetDatabase.SaveAssets();

        return data;
    }

    // 動作確認用: AnimatorControllerParameterTypeごとに空のプリセットを1個ずつ作り、DisplayName/Colorも設定する。
    // 既存分は上書きするので、何度実行しても最新のテスト内容に揃う。
    [MenuItem("Tools/AVS/Generate Test Presets")]
    private static void GenerateTestPresets()
    {
        VisualScriptingGraphDataBase presetBase = LoadOrCreateUserPresetBase();

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

            ((IPresetDisplayInfoWriter)data).SetDisplayInfo(new PresetDisplayInfo(name + "の表示名", color));
        }

        EditorUtility.SetDirty(presetBase);
        AssetDatabase.SaveAssets();
    }
}