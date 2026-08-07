// Editor/Nodes/BaseNode.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public abstract class BaseNode : Node
{
    public string NodeGuid;

    protected BaseNode(string title)
    {
        this.title = title;
        NodeGuid = System.Guid.NewGuid().ToString();
    }

    // Execポートを生成するヘルパー
    protected Port CreateExecPort(Direction direction, Port.Capacity capacity)
    {
        var port = Port.Create<Edge>(
            Orientation.Horizontal,
            direction,
            capacity,
            typeof(bool) // Exec接続はboolで型を統一
        );
        port.portName = direction == Direction.Input ? "In" : "Out";
        return port;
    }

    // データポートを生成するヘルパー
    protected Port CreateDataPort<T>(Direction direction, string portName)
    {
        var port = Port.Create<Edge>(
            Orientation.Horizontal,
            direction,
            Port.Capacity.Single,
            typeof(T)
        );
        port.portName = portName;
        return port;
    }
}