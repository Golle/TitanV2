using System.Diagnostics;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Platform.Win32.D3D;

namespace Titan.Graphics.Pipeline;

internal static class RenderGraphBuilder
{
    public static int Build(Span<D3D12RenderPass> passesOut, Span<RenderPassGroup> groupsOut, RenderPipelinePass[] passesIn)
    {
        //NOTE(Jens): This method allocates some garbage. It's fine for now, but we should revisit it when we have a proper Map implementation in the engine.
        var graph = passesIn.ToDictionary(static c => c.Identifier, static _ => new List<string>());
        var inDegree = passesIn.ToDictionary(static c => c.Identifier, _ => 0);

        foreach (var pass in passesIn)
        {
            foreach (var output in pass.Outputs)
            {
                foreach (var dependantPass in passesIn)
                {
                    if (dependantPass.Inputs.Any(i => i.Identifier == output.Identifier))
                    {
                        graph[pass.Identifier].Add(dependantPass.Identifier);
                        inDegree[dependantPass.Identifier]++;
                    }
                }
            }
        }

        TitanList<D3D12RenderPass> sortedPasses = passesOut;
        TitanList<RenderPassGroup> groups = groupsOut;

        // Kahns algoritm to sort a graph
        var queue = new Queue<string>(inDegree.Where(static a => a.Value == 0).Select(static a => a.Key));
        while (queue.Count > 0)
        {
            var passCount = queue.Count;
            var offset = sortedPasses.Count;
            for (var i = 0; i < passCount; i++)
            {
                var node = queue.Dequeue();

                var pass = passesIn.First(pass => pass.Identifier == node);
                sortedPasses.Add(new D3D12RenderPass
                {
                    Identifier = StringRef.Create(pass.Identifier),
                    Topology = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST,
                    RenderPass = { Type = pass.Type }
                });
                foreach (var neighbor in graph[node])
                {
                    var count = --inDegree[neighbor];
                    if (count == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            groups.Add(new RenderPassGroup
            {
                Offset = (byte)offset,
                Count = (byte)passCount
            });
        }

        Debug.Assert(sortedPasses.Count == passesIn.Length);
        return (int)groups.Count;
    }
}
