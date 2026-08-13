// Editor/PortCompatibility.cs
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;

public static class PortCompatibility
{
    public static bool IsCompatible(Port a, Port b) => a.portType == b.portType;

    public static bool AreAllCompatible(IEnumerable<Edge> edges, out List<string> incompatibleDescriptions)
    {
        incompatibleDescriptions = edges
            .Where(edge => !IsCompatible(edge.output, edge.input))
            .Select(edge => $"{edge.output.node.title}.{edge.output.portName} -> {edge.input.node.title}.{edge.input.portName}")
            .ToList();

        return incompatibleDescriptions.Count == 0;
    }
}
