// Script/Animation/NodeMethodAttributes.cs
using System;

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingActionAttribute : Attribute
{
    public string DisplayName;
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingConditionAttribute : Attribute
{
    public string DisplayName;
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingGetter : Attribute
{
    public string DisplayName;
}