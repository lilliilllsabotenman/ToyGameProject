// Script/Animation/RuntimeNodeExecutor.cs
using System.Reflection;

public class RuntimeNodeExecutor
{
    private GraphExecutor _executor;

    public RuntimeNodeExecutor(VisualScriptingGraphData graphData, INodeActionSource target)
    {
        _executor = new GraphExecutor(
            graphData,
            key =>
            {
                MethodInfo method = target.GetType().GetMethod(key);
                method?.Invoke(target, null);
            },
            key =>
            {
                MethodInfo method = target.GetType().GetMethod(key);
                return method != null && (bool)method.Invoke(target, null);
            });
    }

    public void Run()
    {
        _executor.Run();
    }
}
