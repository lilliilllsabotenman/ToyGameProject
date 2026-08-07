// Script/Animation/NodeMethodAttributes.cs
using System;

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingActionAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingConditionAttribute : Attribute
{
}
