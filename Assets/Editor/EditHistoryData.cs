using System;
using System.Collections.Generic;

[Serializable]
public class ParameterHistoryData
{
    public string ParameterName;
    public List<VisualScriptingGraphData> Data = new();

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
    }
}