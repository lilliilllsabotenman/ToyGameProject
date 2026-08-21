// Script/Animation/VisualScriptingGraphData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeParamEntry
{
    public string Key;
    public string Value;
}

// ノード種別ごとのデータ(StartNodeData/ActionNodeData/IfNodeData)の共通基底。
// Params は汎用のキー・値ストア。メソッド引数など、種別固有フィールドで表現しきれない
// 追加データをここに載せる。
[Serializable]
public abstract class BaseNodeData
{
    public string Guid;
    public string Title;
    public Rect Position;

    public List<NodeParamEntry> Params = new();

    public void SetParam(string key, string value)
    {
        NodeParamEntry entry = Params.Find(p => p.Key == key);
        if (entry != null)
        {
            entry.Value = value;
            return;
        }
        Params.Add(new NodeParamEntry { Key = key, Value = value });
    }

    public string GetParam(string key)
    {
        NodeParamEntry entry = Params.Find(p => p.Key == key);
        return entry?.Value;
    }
}

[Serializable]
public class StartNodeData : BaseNodeData
{
}

[Serializable]
public class ActionNodeData : BaseNodeData
{
    // GraphExecutorのonActionへ渡されるキー(呼び出すメソッド名)
    public string MethodKey;

    // MethodKeyがどのクラス(INodeActionSource実装)のメソッドか。AssemblyQualifiedNameで保持する。
    public string SourceTypeName;
}

// 条件(Conditionポート、未配線ならParamsのリテラル値)を判定し、TrueならTrueポート、FalseならFalseポートへ進む。
[Serializable]
public class IfNodeData : BaseNodeData
{
}

[Serializable]
public class GetMemberNodeData : BaseNodeData
{
    // 参照するメンバ変数名(VisualScriptingGraphData.Membersのキー)
    public string MemberName;
}

[Serializable]
public class SetMemberNodeData : BaseNodeData
{
    // 書き込み先のメンバ変数名(VisualScriptingGraphData.Membersのキー)
    public string MemberName;
}

// 指定回数(Countポート、未配線ならParamsのリテラル値)だけBodyポート側の本体を繰り返し実行する。
// 完了後はCompleteポートから先に進む。
[Serializable]
public class ForNodeData : BaseNodeData
{
}

[Serializable]
public class EdgeData
{
    public string OutputNodeGuid;
    public string OutputPortName;
    public string InputNodeGuid;
    public string InputPortName;
}

public enum MemberKind
{
    Component,
    Value
}

// グラフが持つメンバ変数の宣言(Variablesパネルで管理する想定)。
// 型(Component型/プリミティブ型)はSystem.Typeを直接シリアライズできないため、AssemblyQualifiedNameで保持する。
[Serializable]
public class MemberVariableData
{
    public string Name;
    public MemberKind Kind;
    public string TypeName;
    public string DefaultValue; // Kind == Value の場合のみ使用
}

[Serializable]
public class VisualScriptingGraphData
{
    public string Name;
    public AnimatorControllerParameterType ParameterType;

    [SerializeReference]
    public List<BaseNodeData> Nodes = new();
    public List<EdgeData> Edges = new();
    public List<MemberVariableData> Members = new();
}

public class VisualScriptingGraphDataBase : ScriptableObject
{
    public List<VisualScriptingGraphData> data = new();

    public VisualScriptingGraphData GetData(string name)
        => data.Find(d => d.Name == name);
}