// Editor/NodeMethodOptions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class MethodParamInfo
{
    public string Name;
    public Type Type;
}

public static class NodeMethodOptions
{
    public static List<string> GetActionNames(Type sourceType)
    {
        return GetMethodNames(sourceType, typeof(VisualScriptingActionAttribute));
    }

    public static List<string> GetGetterNames(Type sourceType)
    {
        return GetMethodNames(sourceType, typeof(VisualScriptingGetter));
    }

    public static List<string> GetConditionNames(Type sourceType)
    {
        return GetMethodNames(sourceType, typeof(VisualScriptingConditionAttribute));
    }

    private static List<string> GetMethodNames(Type sourceType, Type attributeType)
    {
        if (sourceType == null) return new List<string>();

        return sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(attributeType) != null)
            .Select(m => m.Name)
            .ToList();
    }

    public static List<MethodParamInfo> GetMethodParams(Type sourceType, string methodName)
    {
        if (sourceType == null) return new List<MethodParamInfo>();

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        if (method == null) return new List<MethodParamInfo>();

        return method.GetParameters()
            .Select(p => new MethodParamInfo { Name = p.Name, Type = p.ParameterType })
            .ToList();
    }

    public static Type GetReturnType(Type sourceType, string methodName)
    {
        if (sourceType == null) return null;

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        return method?.ReturnType;
    }

    public static bool IsGetter(Type sourceType, string methodName)
    {
        if (sourceType == null) return false;

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        return method != null && method.GetCustomAttribute(typeof(VisualScriptingGetter)) != null;
    }

    // GetterのDisplayNameが設定されていればそれを、無ければメソッド名そのものを返す。
    public static string GetDisplayName(Type sourceType, string methodName)
    {
        if (sourceType == null) return methodName;

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        string displayName = method?.GetCustomAttribute<VisualScriptingGetter>()?.DisplayName;
        return string.IsNullOrEmpty(displayName) ? methodName : displayName;
    }   

    // ActionのDisplayNameが設定されていればそれを、無ければメソッド名そのものを返す。
    public static string GetActionDisplayName(Type sourceType, string methodName)
    {
        if (sourceType == null) return methodName;

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        string displayName = method?.GetCustomAttribute<VisualScriptingActionAttribute>()?.DisplayName;
        return string.IsNullOrEmpty(displayName) ? methodName : displayName;
    }

    // ConditionのDisplayNameが設定されていればそれを、無ければメソッド名そのものを返す。
    public static string GetConditionDisplayName(Type sourceType, string methodName)
    {
        if (sourceType == null) return methodName;

        MethodInfo method = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);

        string displayName = method?.GetCustomAttribute<VisualScriptingConditionAttribute>()?.DisplayName;
        return string.IsNullOrEmpty(displayName) ? methodName : displayName;
    }
}
