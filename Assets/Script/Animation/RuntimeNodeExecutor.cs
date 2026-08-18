// Script/Animation/RuntimeNodeExecutor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum ConditionParameter
{
    True,
    False
}

public class RuntimeNodeExecutor
{
    private GraphExecutor _executor;
    private VisualScriptingGraphData _graphData;
    private INodeActionSource _target;
    private Dictionary<string, MethodInfo> _methodCache;
    private Dictionary<string, object> _members;
    private Dictionary<string, object> _actionResultCache = new Dictionary<string, object>();
    // ForNodeData.Guid → 本体(Bodyポート先)専用のGraphExecutor。構築時に1回だけ作る。
    private Dictionary<string, GraphExecutor> _forExecutors;

    public RuntimeNodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        _graphData = graphData;
        _target = target;

        _methodCache = target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .GroupBy(m => m.Name)
            .ToDictionary(g => g.Key, g => g.First());

        _members = BuildMembers(graphData, target);
        _forExecutors = BuildForExecutors(graphData);

        _executor = new GraphExecutor(
            graphData,
            actionData => InvokeAction(actionData),
            conditionData => InvokeCondition(conditionData),
            setMemberData => ExecuteSetMember(setMemberData),
            forData => RunFor(forData));
    }

    // グラフ内の全ForNodeData(入れ子含め、フラットなNodesリストを舐めるだけで自然に拾える)について、
    // Bodyポート先のノードを開始点とする専用GraphExecutorを1回だけ構築しておく。
    private Dictionary<string, GraphExecutor> BuildForExecutors(VisualScriptingGraphData graphData)
    {
        Dictionary<string, GraphExecutor> executors = new Dictionary<string, GraphExecutor>();

        foreach (BaseNodeData nodeData in graphData.Nodes)
        {
            if (nodeData is not ForNodeData forData) continue;

            EdgeData bodyEdge = graphData.Edges
                .Find(e => e.OutputNodeGuid == forData.Guid && e.OutputPortName == "Body");
            if (bodyEdge == null) continue;

            executors[forData.Guid] = new GraphExecutor(
                graphData,
                bodyEdge.InputNodeGuid,
                actionData => InvokeAction(actionData),
                conditionData => InvokeCondition(conditionData),
                setMemberData => ExecuteSetMember(setMemberData),
                innerForData => RunFor(innerForData));
        }

        return executors;
    }

    private MethodInfo GetMethod(string methodName)
    {
        _methodCache.TryGetValue(methodName, out MethodInfo method);
        return method;
    }

    private static Dictionary<string, object> BuildMembers(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        Dictionary<string, object> members = new Dictionary<string, object>();

        foreach (MemberVariableData member in graphData.Members)
        {
            Type type = string.IsNullOrEmpty(member.TypeName) ? null : Type.GetType(member.TypeName);
            if (type == null)
            {
                Debug.LogWarning($"メンバ '{member.Name}' の型を解決できません: {member.TypeName}");
                members[member.Name] = null;
                continue;
            }

            if (member.Kind == MemberKind.Component)
            {
                Component resolved = target.Owner.GetComponent(type);
                if (resolved == null)
                {
                    Debug.LogWarning($"メンバ '{member.Name}' に対応する {type.Name} が {target.Owner.name} に見つかりません");
                }

                members[member.Name] = resolved;
            }
            else
            {
                members[member.Name] = ConvertLiteral(member.DefaultValue, type);
            }
        }

        return members;
    }

    private object ResolveMember(string name)
    {
        if (!_members.TryGetValue(name, out object value))
        {
            throw new KeyNotFoundException($"未定義のメンバです: {name}");
        }

        return value;
    }

    private void SetMember(string name, object value)
    {
        if (!_members.ContainsKey(name))
        {
            throw new KeyNotFoundException($"未定義のメンバです: {name}");
        }

        _members[name] = value;
    }

    private void ExecuteSetMember(SetMemberNodeData setMemberData)
    {
        EdgeData incomingEdge = _graphData.Edges
            .Find(e => e.InputNodeGuid == setMemberData.Guid && e.InputPortName == "Value");
        if (incomingEdge == null) return;

        BaseNodeData sourceNode = _graphData.Nodes.Find(n => n.Guid == incomingEdge.OutputNodeGuid);
        object value = sourceNode switch
        {
            ActionNodeData sourceActionData => InvokeAction(sourceActionData),
            GetMemberNodeData sourceMemberData => ResolveMember(sourceMemberData.MemberName),
            _ => null
        };

        SetMember(setMemberData.MemberName, value);
    }

    // Countポート(未配線ならParamsのリテラル値)を解決し、その回数だけ本体を実行する。
    // 1周ごとに_actionResultCacheをクリアし、副作用を持つActionが周回ごとにちゃんと再実行されるようにする。
    private void RunFor(ForNodeData forData)
    {
        int count = ResolveForCount(forData);
        if (count <= 0) return;

        if (!_forExecutors.TryGetValue(forData.Guid, out GraphExecutor bodyExecutor)) return;

        for (int i = 0; i < count; i++)
        {
            _actionResultCache.Clear();
            bodyExecutor.Run();
        }
    }

    private int ResolveForCount(ForNodeData forData)
    {
        EdgeData incomingEdge = _graphData.Edges
            .Find(e => e.InputNodeGuid == forData.Guid && e.InputPortName == "Count");

        object value;
        if (incomingEdge != null)
        {
            BaseNodeData sourceNode = _graphData.Nodes.Find(n => n.Guid == incomingEdge.OutputNodeGuid);
            value = sourceNode switch
            {
                ActionNodeData sourceActionData => InvokeAction(sourceActionData),
                GetMemberNodeData sourceMemberData => ResolveMember(sourceMemberData.MemberName),
                _ => null
            };
        }
        else
        {
            value = ConvertLiteral(forData.GetParam("Count"), typeof(int));
        }

        return value is int intValue ? intValue : 0;
    }

    public void Run()
    {
        _actionResultCache.Clear();
        _executor.Run();
    }

    //呼び出し本体、
    private object InvokeAction(ActionNodeData actionData)
    {
        if (_actionResultCache.TryGetValue(actionData.Guid, out object cachedResult))
        {
            return cachedResult;
        }

        MethodInfo method = GetMethod(actionData.MethodKey);
        if (method == null) return null;

        ParameterInfo[] parameters = method.GetParameters();
        object[] args = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = ResolveArgument(parameters[i], actionData);
        }

        object result = method.Invoke(_target, args);
        _actionResultCache[actionData.Guid] = result;
        return result;
    }

    private string InvokeCondition(ConditionNodeData conditionData)
    {
        MethodInfo method = GetMethod(conditionData.MethodKey);
        if (method == null) return null;

        ParameterInfo[] parameters = method.GetParameters();
        object[] args = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = ResolveArgument(parameters[i], conditionData);
        }

        return ((ConditionParameter)method.Invoke(_target, args)).ToString();
    }

    // 入力ポートに配線があれば配線元ノードをその場で実行して戻り値を使い、
    // 無ければParamsの直打ち値を使う。
    private object ResolveArgument(ParameterInfo parameter, BaseNodeData nodeData)
    {
        EdgeData incomingEdge = _graphData.Edges
            .Find(e => e.InputNodeGuid == nodeData.Guid && e.InputPortName == parameter.Name);

        if (incomingEdge != null)
        {
            BaseNodeData sourceNode = _graphData.Nodes.Find(n => n.Guid == incomingEdge.OutputNodeGuid);
            if (sourceNode is ActionNodeData sourceActionData)
            {
                return InvokeAction(sourceActionData);
            }
            if (sourceNode is GetMemberNodeData sourceMemberData)
            {
                return ResolveMember(sourceMemberData.MemberName);
            }
        }

        return ConvertParam(nodeData.Params, parameter);
    }

    private static object ConvertParam(List<NodeParamEntry> paramEntries, ParameterInfo parameter)
    {
        NodeParamEntry entry = paramEntries?.Find(p => p.Key == parameter.Name);
        return ConvertLiteral(entry?.Value, parameter.ParameterType);
    }

    private static object ConvertLiteral(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        if (type == typeof(bool))
        {
            return bool.TryParse(value, out bool result) && result;
        }

        return Convert.ChangeType(value, type);
    }
}