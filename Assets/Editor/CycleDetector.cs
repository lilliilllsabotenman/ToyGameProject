// Editor/CycleDetector.cs
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;

// ActionNode同士のデータ配線(Result → パラメータ)が循環していないかを検査する。
// 循環があると実行時にInvokeActionが無限再帰し、StackOverflowException(catch不能)でクラッシュするため。
public static class CycleDetector
{
    public static bool HasNoCycles(IEnumerable<Edge> edges, out List<string> cycleDescriptions)
    {
        Dictionary<Node, List<Node>> dependsOn = new Dictionary<Node, List<Node>>();

        foreach (Edge edge in edges)
        {
            if (edge.output.node is not ActionNode || edge.input.node is not ActionNode) continue;
            if (edge.output.portName != "Result") continue;

            Node consumer = edge.input.node;
            Node source = edge.output.node;

            if (!dependsOn.TryGetValue(consumer, out List<Node> sources))
            {
                sources = new List<Node>();
                dependsOn[consumer] = sources;
            }
            sources.Add(source);
        }

        List<string> cycles = new List<string>();
        HashSet<Node> visited = new HashSet<Node>();
        HashSet<Node> inStack = new HashSet<Node>();

        foreach (Node node in dependsOn.Keys)
        {
            if (!visited.Contains(node))
            {
                FindCycle(node, dependsOn, visited, inStack, new List<Node>(), cycles);
            }
        }

        cycleDescriptions = cycles;
        return cycles.Count == 0;
    }

    private static void FindCycle(Node node, Dictionary<Node, List<Node>> dependsOn, HashSet<Node> visited, HashSet<Node> inStack, List<Node> path, List<string> cycles)
    {
        visited.Add(node);
        inStack.Add(node);
        path.Add(node);

        if (dependsOn.TryGetValue(node, out List<Node> sources))
        {
            foreach (Node source in sources)
            {
                if (inStack.Contains(source))
                {
                    int startIndex = path.IndexOf(source);
                    IEnumerable<string> cycleNodes = path.Skip(startIndex).Select(n => n.title).Append(source.title);
                    cycles.Add(string.Join(" -> ", cycleNodes));
                }
                else if (!visited.Contains(source))
                {
                    FindCycle(source, dependsOn, visited, inStack, path, cycles);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        inStack.Remove(node);
    }
}
