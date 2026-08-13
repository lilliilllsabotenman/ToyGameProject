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

    public RuntimeNodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        _graphData = graphData;
        _target = target;

        _methodCache = target.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .GroupBy(m => m.Name)
            .ToDictionary(g => g.Key, g => g.First());

        _members = BuildMembers(graphData, target);

        _executor = new GraphExecutor(
            graphData,
            actionData => InvokeAction(actionData),
            conditionData => InvokeCondition(conditionData),
            setMemberData => ExecuteSetMember(setMemberData));
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