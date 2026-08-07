// Editor/Nodes/ActionNode.cs
using UnityEditor.Experimental.GraphView;

public class ActionNode : BaseNode
{
    private string _actionKey;

    public string ActionKey
    {
        get { return _actionKey; }
        set
        {
            _actionKey = value;
            if (!string.IsNullOrEmpty(value)) title = value;
        }
    }

    public ActionNode() : this(null)
    {
    }

    public ActionNode(string initialMethodName) : base("Action")
    {
        // Exec In/Out
        inputContainer.Add(CreateExecPort(Direction.Input, Port.Capacity.Multi));
        outputContainer.Add(CreateExecPort(Direction.Output, Port.Capacity.Multi));

        ActionKey = initialMethodName;

        RefreshExpandedState();
        RefreshPorts();
    }
}
