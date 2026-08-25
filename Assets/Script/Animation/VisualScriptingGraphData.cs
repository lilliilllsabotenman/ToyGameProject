// Script/Animation/VisualScriptingGraphData.cs
using System;
using System.Collections.Generic;
using System.Linq;
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

    // MemberwiseClone()は実行時の具象型ごとコピーされるため、Params以外は非virtualな一括対応で全サブクラスに使える。
    // Params(参照型リスト)だけは中身までコピーして独立させる。
    public BaseNodeData Clone()
    {
        BaseNodeData clone = (BaseNodeData)MemberwiseClone();
        clone.Params = Params.Select(p => new NodeParamEntry { Key = p.Key, Value = p.Value }).ToList();
        return clone;
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

// プリセット一覧表示など、読み取り専用で見せたい相手に渡すための契約。
public interface IPresetDisplayInfo
{
    PresetDisplayInfo GetDisplayInfo();
}

// AVSエディターなど、書き込みを許可したい相手にだけ渡すための契約。
public interface IPresetDisplayInfoWriter
{
    void SetDisplayInfo(PresetDisplayInfo displayInfo);
}

// DisplayName+Colorをまとめて受け渡すための入れ物(PresetNameDialog→PresetSerializerなど)。
// VisualScriptingGraphDataのシリアライズフィールドとしても使うため、readonlyにはしない(Unityはreadonlyフィールドをシリアライズできない)。
[Serializable]
public struct PresetDisplayInfo
{
    public string DisplayName;
    public Color Color;

    public PresetDisplayInfo(string displayName, Color color)
    {
        DisplayName = displayName;
        Color = color;
    }
}

[Serializable]
public class VisualScriptingGraphData : IPresetDisplayInfo, IPresetDisplayInfoWriter
{
    public string Name;
    public AnimatorControllerParameterType ParameterType;

    // プリセットテンプレート自身のユニークな識別子。Nameは適用先パラメータ名を表すだけで一意ではないため、
    // 「このプリセットが今適用されているか」の判定にはこちらを使う。
    public string PresetId;

    [SerializeField]
    private PresetDisplayInfo _displayInfo;

    [SerializeReference]
    public List<BaseNodeData> Nodes = new();
    public List<EdgeData> Edges = new();
    public List<MemberVariableData> Members = new();

    //GetterとSetter機能をインターフェース経由以外で触らせないようにするための特殊実装↓
    PresetDisplayInfo IPresetDisplayInfo.GetDisplayInfo() => _displayInfo;
    void IPresetDisplayInfoWriter.SetDisplayInfo(PresetDisplayInfo displayInfo) => _displayInfo = displayInfo;

    // Action/Conditionノードでキー(MethodKey)が空のまま保存された場合に検出する。
    // 実行時のDictionaryキー不一致(未登録キー)はここでは検出できない(実行側の登録内容をこの型は知らないため)。
    public bool HasNoMissingParameters(out List<string> missing)
    {
        missing = new List<string>();
        foreach (BaseNodeData nodeData in Nodes)
        {
            bool isMissing = nodeData switch
            {
                ActionNodeData actionData => string.IsNullOrWhiteSpace(actionData.MethodKey),
                _ => false
            };
            if (isMissing)
            {
                missing.Add(nodeData.GetType().Name + " (" + nodeData.Guid.Substring(0, 8) + ")");
            }
        }

        return missing.Count == 0;
    }

    // プリセットのテンプレートを壊さず使うための独立コピー。Nodes/Edges/Membersの中身までコピーする。
    public VisualScriptingGraphData Clone()
    {
        return new VisualScriptingGraphData
        {
            Name = Name,
            ParameterType = ParameterType,
            PresetId = PresetId,
            _displayInfo = _displayInfo,
            Nodes = Nodes.Select(n => n.Clone()).ToList(),
            Edges = Edges.Select(e => new EdgeData
            {
                OutputNodeGuid = e.OutputNodeGuid,
                OutputPortName = e.OutputPortName,
                InputNodeGuid = e.InputNodeGuid,
                InputPortName = e.InputPortName
            }).ToList(),
            Members = Members.Select(m => new MemberVariableData
            {
                Name = m.Name,
                Kind = m.Kind,
                TypeName = m.TypeName,
                DefaultValue = m.DefaultValue
            }).ToList()
        };
    }
}