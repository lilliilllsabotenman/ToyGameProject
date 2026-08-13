// Editor/GetMemberNode.cs
using System;

public class GetMemberNode : BaseNode
{
    public string MemberName;

    public GetMemberNode(string memberName, Type portType) : base("Get " + memberName)
    {
        MemberName = memberName;
        AddOutput("Value", portType ?? typeof(object));

        RefreshExpandedState();
        RefreshPorts();
    }
}
