// Script/Animation/NodeExecutor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// 各ノードを実際にどう実行するか(引数解決・メソッド呼び出し・Member読み書き)を担う。
// グラフ探索(次にどのノードへ進むか)はRuntimeNodeExecutorの責務で、こちらは1個のノードの実行だけを見る。
// Forループの本体だけは探索が必要なため、そこだけRuntimeNodeExecutorから渡されたrunFromに委譲する。
public class NodeExecutor
{
    private VisualScriptingGraphData _graphData;
    private INodeActionSource _target;
    private Dictionary<string, string> _forBodyStartGuids;
    private Action<string> _runFrom;

    // SourceTypeNameで指定された型ごとのインスタンス/メソッド一覧。ノードが実際に呼び出す対象は_targetとは限らないため型ごとに解決する。
    private Dictionary<Type, INodeActionSource> _sourceInstances;
    private Dictionary<Type, Dictionary<string, MethodInfo>> _methodCaches;
    private Dictionary<string, object> _members;
    private Dictionary<string, object> _actionResultCache = new Dictionary<string, object>();

    public NodeExecutor(
        VisualScriptingGraphData graphData,
        INodeActionSource target,
        Dictionary<string, string> forBodyStartGuids,
        Action<string> runFrom)
    {
        _graphData = graphData;
        _target = target;
        _forBodyStartGuids = forBodyStartGuids;
        _runFrom = runFrom;

        _sourceInstances = new Dictionary<Type, INodeActionSource> { [target.GetType()] = target };
        _methodCaches = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        // Member管理は作り直し中のため一時的に無効化(最下部にコメントアウトして退避済み)。
        // _members = BuildMembers(graphData, target);
    }

    #region 公開エントリーポイント(RuntimeNodeExecutorから呼ばれる)

    // 1回のグラフ実行(RuntimeNodeExecutor.Run)開始時に呼ばれる。Actionの戻り値キャッシュをクリアする。
    public void ClearActionResultCache()
    {
        _actionResultCache.Clear();
    }

    // Conditionポート(未配線ならParamsのリテラル値)を解決し、真偽に応じてTrue/Falseを返す。
    public string EvaluateIf(IfNodeData ifData)
    {
        if (!TryResolveNodeValue(ifData.Guid, "Condition", out object value))
        {
            value = ConvertLiteral(ifData.GetParam("Condition"), typeof(bool));
        }

        return value is bool boolValue && boolValue ? "True" : "False";
    }

    // Countポート(未配線ならParamsのリテラル値)を解決し、その回数だけ本体を実行する。
    // 1周ごとに_actionResultCacheをクリアし、副作用を持つActionが周回ごとにちゃんと再実行されるようにする。
    public void RunFor(ForNodeData forData)
    {
        if (!TryResolveNodeValue(forData.Guid, "Count", out object value))
        {
            value = ConvertLiteral(forData.GetParam("Count"), typeof(int));
        }
        int count = value is int intValue ? intValue : 0;
        if (count <= 0) return;

        if (!_forBodyStartGuids.TryGetValue(forData.Guid, out string bodyStartGuid)) return;

        for (int i = 0; i < count; i++)
        {
            _actionResultCache.Clear();
            _runFrom(bodyStartGuid);
        }
    }   

    //呼び出し本体、
    public object InvokeAction(ActionNodeData actionData)
    {
        if (_actionResultCache.TryGetValue(actionData.Guid, out object cachedResult))
        {
            return cachedResult;
        }

        // SourceTypeNameが未指定ならnull、指定されていればその型を解決する。
        Type sourceType = null;
        if (!string.IsNullOrEmpty(actionData.SourceTypeName))
        {
            sourceType = Type.GetType(actionData.SourceTypeName);
        }

        // 型が解決できなかった場合は、デフォルトのターゲット型にフォールバックする。
        if (sourceType == null)
        {
            sourceType = _target.GetType();
        }

        MethodInfo method = GetMethod(sourceType, actionData.MethodKey);
        if (method == null) return null;

        object source = null;

        //対象メソッドがStaticではない場合に自信が所有するノードソースからメソッドを引っ張れるかチェックする
        if (!method.IsStatic)
        {
            source = ResolveInvokeSource(sourceType, actionData);
            if (source == null)
            {
                Debug.LogWarning($"'{actionData.MethodKey}'の実行対象(Target)を解決できません: {sourceType.Name}");
                return null;
            }
        }

        // 各引数は、配線があればそれを優先、無ければParamsのリテラル値を型変換して使う。
        ParameterInfo[] parameters = method.GetParameters();
        object[] args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (TryResolveNodeValue(actionData.Guid, parameter.Name, out object wiredValue))
            {
                args[i] = wiredValue;
            }
            else
            {
                NodeParamEntry entry = actionData.Params?.Find(p => p.Key == parameter.Name);
                args[i] = ConvertLiteral(entry?.Value, parameter.ParameterType);
            }
        }

        object result = method.Invoke(source, args);
        _actionResultCache[actionData.Guid] = result;
        return result;
    }

    #endregion

    #region Action呼び出し補助

    // 非staticメソッドの実行対象インスタンスを解決する。
    // IsDefault指定のsourceTypeは登録済み/遅延生成した単一インスタンスを、それ以外はノードの"Target"ポートの配線を辿って解決する。
    private object ResolveInvokeSource(Type sourceType, ActionNodeData actionData)
    {
        if (_sourceInstances.TryGetValue(sourceType, out INodeActionSource cached))
        {
            return cached;
        }

        VisualScriptingSourceAttribute attribute = sourceType.GetCustomAttribute<VisualScriptingSourceAttribute>();
        if (attribute != null && attribute.IsDefault)
        {
            INodeActionSource created = (INodeActionSource)Activator.CreateInstance(sourceType, _target.Owner);
            _sourceInstances[sourceType] = created;
            return created;
        }

        TryResolveNodeValue(actionData.Guid, "Target", out object source);
        return source;
    }

    private MethodInfo GetMethod(Type sourceType, string methodName)
    {
        if (!_methodCaches.TryGetValue(sourceType, out Dictionary<string, MethodInfo> cache))
        {
            cache = sourceType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .GroupBy(m => m.Name)
                .ToDictionary(g => g.Key, g => g.First());
            _methodCaches[sourceType] = cache;
        }

        cache.TryGetValue(methodName, out MethodInfo method);
        return method;
    }

    #endregion

    #region 値・引数解決

    // 入力ポート(nodeGuid, portName)に配線があれば、配線元ノード(Action/GetMember)を辿って値を解決する。
    // 配線が無ければfalseを返すので、呼び出し側はリテラル値へのフォールバックを行える。
    private bool TryResolveNodeValue(string nodeGuid, string portName, out object value)
    {
        EdgeData incomingEdge = null;
        foreach (EdgeData edge in _graphData.Edges)
        {
            if (edge.InputNodeGuid == nodeGuid && edge.InputPortName == portName)
            {
                incomingEdge = edge;
                break;
            }
        }

        if (incomingEdge != null)
        {
            BaseNodeData sourceNode = _graphData.Nodes.Find(n => n.Guid == incomingEdge.OutputNodeGuid);
            switch (sourceNode)
            {
                case ActionNodeData sourceActionData:
                    value = InvokeAction(sourceActionData);
                    return true;
                // Member管理は作り直し中のため一時的に無効化(最下部にコメントアウトして退避済み)。
                // case GetMemberNodeData sourceMemberData:
                //     value = ResolveMember(sourceMemberData.MemberName);
                //     return true;
            }
        }

        value = null;
        return false;
    }

    // ノードの引数とかの枠に入れられた値を、指定した型の実際の値に変換する。全種別変換の最終着地点。
    private static object ConvertLiteral(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            // 未入力の場合。値型はその型のデフォルト値(0/false/0fなど)、参照型はnullを返す。
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        if (type == typeof(bool))
        {
            // boolだけ特別扱い。パースに失敗しても例外にはせず、falseとして扱う(&&の左辺がfalseなら右辺resultは見られない)。
            return bool.TryParse(value, out bool result) && result;
        }

        // それ以外(int/float/stringなど)は.NET標準の変換に任せる。変換できない文字列だと例外が飛ぶ。
        return Convert.ChangeType(value, type);
    }

    #endregion

    #region Member管理(作り直し中のため無効化、退避のみ)

    // RuntimeNodeExecutorの_handlersから外部で呼ばれる公開エントリーポイントなので、メソッド自体はコメントアウトできない(中身だけ空)。
    public void ExecuteSetMember(SetMemberNodeData setMemberData)
    {
        // if (!TryResolveNodeValue(setMemberData.Guid, "Value", out object value))
        // {
        //     Type memberType = ResolveMemberType(setMemberData.MemberName);
        //     value = ConvertLiteral(setMemberData.GetParam("Value"), memberType);
        // }
        //
        // SetMember(setMemberData.MemberName, value);
    }

    // private static Dictionary<string, object> BuildMembers(VisualScriptingGraphData graphData, INodeActionSource target)
    // {
    //     Dictionary<string, object> members = new Dictionary<string, object>();
    //
    //     foreach (MemberVariableData member in graphData.Members)
    //     {
    //         Type type = string.IsNullOrEmpty(member.TypeName) ? null : Type.GetType(member.TypeName);
    //         if (type == null)
    //         {
    //             Debug.LogWarning($"メンバ '{member.Name}' の型を解決できません: {member.TypeName}");
    //             members[member.Name] = null;
    //             continue;
    //         }
    //
    //         if (member.Kind == MemberKind.Component)
    //         {
    //             Component resolved = target.Owner.GetComponent(type);
    //             if (resolved == null)
    //             {
    //                 Debug.LogWarning($"メンバ '{member.Name}' に対応する {type.Name} が {target.Owner.name} に見つかりません");
    //             }
    //
    //             members[member.Name] = resolved;
    //         }
    //         else
    //         {
    //             members[member.Name] = ConvertLiteral(member.DefaultValue, type);
    //         }
    //     }
    //
    //     return members;
    // }
    //
    // private object ResolveMember(string name)
    // {
    //     if (!_members.TryGetValue(name, out object value))
    //     {
    //         throw new KeyNotFoundException($"未定義のメンバです: {name}");
    //     }
    //
    //     return value;
    // }
    //
    // private void SetMember(string name, object value)
    // {
    //     if (!_members.ContainsKey(name))
    //     {
    //         throw new KeyNotFoundException($"未定義のメンバです: {name}");
    //     }
    //
    //     _members[name] = value;
    // }
    //
    // private Type ResolveMemberType(string name)
    // {
    //     MemberVariableData member = _graphData.Members.Find(m => m.Name == name);
    //     return string.IsNullOrEmpty(member?.TypeName) ? null : Type.GetType(member.TypeName);
    // }

    #endregion
}