using System;
using System.Collections.Generic;

[Serializable]
public class ParameterHistoryData
{
    public string ParameterName;
    public List<VisualScriptingGraphData> Data = new();
    // Undoで退避した内容を一時保存するBuffer。新しい編集(AddHistory)が来ると破棄される。
    public List<VisualScriptingGraphData> RedoData = new();

    public ParameterHistoryData(string parameterName, VisualScriptingGraphData data)
    {
        ParameterName = parameterName;
        Data.Add(data);
    }
}

[Serializable]
public class EditHistoryData
{
    public List<ParameterHistoryData> HistoryData = new();

    public ParameterHistoryData GetData(string parameterName) => HistoryData.Find(h => h.ParameterName == parameterName);

    public void AddHistory(string parameterName, VisualScriptingGraphData data)
    {
        ParameterHistoryData history = GetData(parameterName);
        if (history == null)
        {
            HistoryData.Add(new ParameterHistoryData(parameterName, data));
            return;
        }

        history.Data.Add(data);
        history.RedoData.Clear();
    }

    // 直近(=今の状態)を捨ててRedoBufferへ退避し、その1つ前のスナップショットの中身をtargetへ直接コピーする。
    // それ以上遡れなければ何もせずfalseを返す。
    public bool Undo(string parameterName, VisualScriptingGraphData target)
    {
        ParameterHistoryData history = GetData(parameterName);
        if (history == null || history.Data.Count <= 1) return false;

        VisualScriptingGraphData undone = history.Data[^1];
        history.Data.RemoveAt(history.Data.Count - 1);
        history.RedoData.Add(undone);

        ApplyTo(target, history.Data[^1]);
        return true;
    }       

    // RedoBufferから1件戻して、その中身をtargetへ直接コピーする。Bufferが空なら何もせずfalseを返す。
    public bool Redo(string parameterName, VisualScriptingGraphData target)
    {
        ParameterHistoryData history = GetData(parameterName);
        if (history == null || history.RedoData.Count == 0) return false;

        VisualScriptingGraphData redone = history.RedoData[^1];
        history.RedoData.RemoveAt(history.RedoData.Count - 1);
        history.Data.Add(redone);

        ApplyTo(target, redone);
        return true;
    }

    // sourceの中身を、参照を共有せず別の入れ物にコピーしてtargetへ反映する。
    private static void ApplyTo(VisualScriptingGraphData target, VisualScriptingGraphData source)
    {
        target.Nodes = new List<BaseNodeData>(source.Nodes);
        target.Edges = new List<EdgeData>(source.Edges);
    }
}