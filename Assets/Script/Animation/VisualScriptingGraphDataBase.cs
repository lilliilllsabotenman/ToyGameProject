// Script/Animation/VisualScriptingGraphDataBase.cs
using System.Collections.Generic;
using UnityEngine;

public enum DataState
{
    ExecuteData,
    UserPreset,
    DefaultPreset
}

public class VisualScriptingGraphDataBase : ScriptableObject
{
    [Header("デフォルトプリセットに入れるかどうかのチェック")]
    public DataState DataState = DataState.ExecuteData;

    public List<VisualScriptingGraphData> data = new();

    public VisualScriptingGraphData GetData(string name)
        => data.Find(d => d.Name == name);

    // 新規追加前に、同名のエントリが既に存在するかを確認するための入口。
    public bool HasDuplicate(string name)
        => data.Exists(d => d.Name == name);

    // 特定のプリセットテンプレート(PresetId)が今このdata内に存在するかを確認する。
    // Nameは適用先パラメータ名を表すだけで一意ではないため、プリセット単位の判定にはこちらを使う。
    public bool HasPresetId(string presetId)
        => !string.IsNullOrEmpty(presetId) && data.Exists(d => d.PresetId == presetId);

    // 同名の重複が無ければ追加する。重複があれば何もせずfalseを返す(呼び出し側は都度data.Addせず、ここを唯一の注入口にする)。
    public bool SetData(VisualScriptingGraphData newData)
    {
        if (HasDuplicate(newData.Name)) return false;

        data.Add(newData);
        return true;
    }

    public void RemoveData(string Name)
    {
        VisualScriptingGraphData Data = data.Find(d => d.Name == Name);

        if(Data == null) return;
        data.Remove(Data);
    }
}