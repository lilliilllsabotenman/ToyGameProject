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
public class VisualScriptingGetter : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }
}

// ノード探索対象クラス(INodeActionSource実装)に付けて、エディタ上の表示名を指定する。
[AttributeUsage(AttributeTargets.Class)]
public class VisualScriptingSourceAttribute : Attribute, IDisplayNameAttribute
{
    public string DisplayName { get; set; }

    // true: NodeRunnerが事前生成する単一インスタンスを実行対象にする(Targetポート不要)。
    // false: 非staticメソッドはノードにTargetポートが生え、配線されたインスタンスを実行対象にする。
    public bool IsDefault { get; set; }
}
