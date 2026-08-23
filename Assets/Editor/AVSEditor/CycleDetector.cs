// Editor/CycleDetector.cs
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;
using System.Linq;

// データ配線(Result → パラメータ)とExec配線(実行順)、それぞれの循環を検査する。
// データ循環は実行時にInvokeActionが無限再帰し、StackOverflowException(catch不能)でクラッシュする。
// Exec循環はGraphExecutor.Run()のwhileループが無限ループになり、フリーズする。
public static class CycleDetector
{
    // GetterNodeもActionNodeDataとして同じ実行経路(InvokeAction)を通るため、ActionNodeだけでなくGetterNodeも対象にする必要がある。
    public static bool HasNoDataCycles(IEnumerable<Edge> edges, out List<string> cycleDescriptions)
    {
        return HasNoCycles(edges, edge =>
            edge.output.node is IDataResultNode &&
            edge.input.node is IDataResultNode &&
            edge.output.portName == "Result", out cycleDescriptions);
    }

    // Exec入力ポートはBaseNode.CreateExecPortで必ず"In"固定(出力側はOut/True/False/Body/Completeとノードごとに異なる)。
    // そのためinput.portName=="In"だけで、ノード種別を問わずExec配線を判別できる。
    public static bool HasNoExecCycles(IEnumerable<Edge> edges, out List<string> cycleDescriptions)
    {
        return HasNoCycles(edges, edge => edge.input.portName == "In", out cycleDescriptions);
    }

    private static bool HasNoCycles(IEnumerable<Edge> edges, Func<Edge, bool> isTargetEdge, out List<string> cycleDescriptions)
    {
        Dictionary<Node, List<Node>> dependsOn = new Dictionary<Node, List<Node>>();

        foreach (Edge edge in edges)
        {
            if (!isTargetEdge(edge)) continue;

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
