// Editor/Nodes/StartNode.cs
using UnityEditor.Experimental.GraphView;

public class StartNode : BaseNode
{
    public StartNode() : base("Start")
    {
        // Startはインプットなし、アウトプットのみ
        var execOut = CreateExecPort(Direction.Output, Port.Capacity.Multi);
        outputContainer.Add(execOut);

        capabilities &= ~Capabilities.Deletable; // 削除不可
        RefreshExpandedState();
        RefreshPorts();
    }
}