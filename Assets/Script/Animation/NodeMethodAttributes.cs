// Script/Animation/NodeMethodAttributes.cs
using System;

public interface IDisplayNameAttribute
{
    string DisplayName { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingActionAttribute : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingConditionAttribute : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class VisualScriptingGetter : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }
}

// ノード探索対象クラス(INodeActionSource実装)に付けて、エディタ上の表示名を指定する。
[AttributeUsage(AttributeTargets.Class)]
public class VisualScriptingSourceAttribute : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }
}
