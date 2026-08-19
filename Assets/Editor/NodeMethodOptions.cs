// Editor/NodeMethodOptions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public class MethodParamInfo
{
    public string Name;
    public Type Type;
}

public static class NodeMethodOptions
{
    // TBaseを継承/実装した、abstractでもジェネリック定義でもない型を全探索して返す。
    public static List<Type> GetDerivedTypes<TBase>()
    {
        return TypeCache.GetTypesDerivedFrom<TBase>()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .OrderBy(t => t.Name)
            .ToList();
    }

    public static List<string> GetMethodNames<TAttribute>(Type sourceType) where TAttribute : Attribute
    {
        if (sourceType == null) return new List<string>();

        return sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<TAttribute>() != null)
            .Select(m => m.Name)
            .ToList();
    }

    public static List<MethodParamInfo> GetMethodParams(Type sourceType, string methodName)
    {
        MethodInfo method = FindMethod(sourceType, methodName);
        if (method == null) return new List<MethodParamInfo>();

        return method.GetParameters()
            .Select(p => new MethodParamInfo { Name = p.Name, Type = p.ParameterType })
            .ToList();
    }

    public static Type GetReturnType(Type sourceType, string methodName)
    {
        return FindMethod(sourceType, methodName)?.ReturnType;
    }

    public static bool IsGetter(Type sourceType, string methodName)
    {
        MethodInfo method = FindMethod(sourceType, methodName);
        return method != null && method.GetCustomAttribute(typeof(VisualScriptingGetter)) != null;
    }

    // TAttributeのDisplayNameが設定されていればそれを、無ければメソッド名そのものを返す。
    // Getter/Action/Conditionの各DisplayName取得を1本に集約したもの。
    public static string GetDisplayName<TAttribute>(Type sourceType, string methodName)
        where TAttribute : Attribute, IDisplayNameAttribute
    {
        MethodInfo method = FindMethod(sourceType, methodName);
        string displayName = method?.GetCustomAttribute<TAttribute>()?.DisplayName;
        return string.IsNullOrEmpty(displayName) ? methodName : displayName;
    }

    // VisualScriptingSourceのDisplayNameが設定されていればそれを、無ければクラス名そのものを返す。
    public static string GetSourceDisplayName(Type sourceType)
    {
        if (sourceType == null) return null;

        string displayName = sourceType.GetCustomAttribute<VisualScriptingSourceAttribute>()?.DisplayName;
        return string.IsNullOrEmpty(displayName) ? sourceType.Name : displayName;
    }

    private static MethodInfo FindMethod(Type sourceType, string methodName)
    {
        if (sourceType == null) return null;

        return sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName);
    }
}