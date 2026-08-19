// Editor/Nodes/BaseNode.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

// Execポート専用の型マーカー。実データ型(boolなど)と衝突させず、Exec配線とDataポートをPortCompatibilityで別物として扱うために使う。
public class ExecFlow
{
}

public abstract class BaseNode : Node
{
    public string NodeGuid;

    public List<NodeParamEntry> Params = new();
    protected VisualElement ParamsContainer;

    protected BaseNode(string title)
    {
        this.title = title;
        NodeGuid = System.Guid.NewGuid().ToString();

        ParamsContainer = new VisualElement();
        extensionContainer.Add(ParamsContainer);
    }

    // Execポートを生成するヘルパー
    protected Port CreateExecPort(Direction direction, Port.Capacity capacity)
    {
        var port = Port.Create<Edge>(
            Orientation.Horizontal,
            direction,
            capacity,
            typeof(ExecFlow) // Exec接続専用の型。bool型のDataポートと誤接続できないよう区別する
        );
        port.portName = direction == Direction.Input ? "In" : "Out";
        return port;
    }

    // BaseNode
    protected Port CreateDataPort(Direction direction, string portName, Type dataType, Port.Capacity capacity = Port.Capacity.Single)
    {
        var port = Port.Create<Edge>(
            Orientation.Horizontal,
            direction,
            capacity,
            dataType
        );
        port.portName = portName;
        return port;
    }


    protected void AddInput(string key, Type type, string initialValue = null)
    {
        NodeParamEntry entry = new NodeParamEntry { Key = key, Value = initialValue };
        Params.Add(entry);

        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;

        Port port = CreateDataPort(Direction.Input, key, type);
        row.Add(port);

        TextField field = new TextField(key) { value = initialValue ?? string.Empty, isDelayed = true };
        field.RegisterValueChangedCallback(evt => entry.Value = evt.newValue);
        row.Add(field);

        ParamsContainer.Add(row);

        RefreshExpandedState();
        RefreshPorts();
    }

    protected void AddOutput(string key, Type type)
    {
        Port port = CreateDataPort(Direction.Output, key, type, Port.Capacity.Multi);
        outputContainer.Add(port);

        RefreshExpandedState();
        RefreshPorts();
    }
}