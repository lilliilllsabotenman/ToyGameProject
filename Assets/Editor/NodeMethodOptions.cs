// Editor/NodeMethodOptions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ノード候補メソッドをリフレクションで列挙するEditor専用ヘルパー。
// 対象型を変えたい場合はSourceTypeを差し替える。
public static class NodeMethodOptions
{
    private static readonly Type SourceType = typeof(GraphExecutorSample);

    public static List<string> GetActionNames()
    {
        return GetMethodNames(typeof(VisualScriptingActionAttribute), typeof(void));
    }

    public static List<string> GetConditionNames()
    {
        return GetMethodNames(typeof(VisualScriptingConditionAttribute), typeof(bool));
    }

    private static List<string> GetMethodNames(Type attributeType, Type returnType)
    {
        return SourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute(attributeType) != null)
            .Where(m => m.ReturnType == returnType && m.GetParameters().Length == 0)
            .Select(m => m.Name)
            .ToList();
    }
}
