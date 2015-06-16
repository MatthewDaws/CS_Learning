using System;
using System.Collections.Generic;
using System.Linq;

using GraphPaths;

internal class TestGraph
{
    static public IEnumerable<List<T>> Product<T>(List<T> choices, int repeat)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < repeat; ++i) { indices.Add(0); }
        while (true)
        {
            var ret = new List<T>();
            foreach (int i in indices) { ret.Add(choices[i]); }
            yield return ret;
            int index = 0;
            ++indices[index];
            while (indices[index] >= choices.Count())
            {
                indices[index] = 0;
                ++index;
                if (index >= repeat) { break; }
                ++indices[index];
            }
            if (index >= repeat) { break; }
        }
    }

    // Enumerate all undirected graphs on `vertices` number of vertices
    // Takes no account of symmetry so e.g. returns a lot of graphs with one edge (though these
    // are all isomorphic).
    static IEnumerable<UndirectedGraph<int>> AllUndirectedGraphs(int vertices)
    {
        var choices = Product(new List<int> { 0, 1 }, vertices * (vertices - 1) / 2);
        foreach (var choice in choices)
        {
            var graph = new UndirectedGraph<int>();
            for (int v = 0, index = 0; v < vertices; ++v)
            {
                for (int u = v + 1; u < vertices; ++u, ++index)
                {
                    if (choice[index] == 1)
                    {
                        graph.AddEdge(v, u);
                    }
                }
            }
            yield return graph;
        }
    }

    // Enumerate all directed graphs on `vertices` number of vertices
    // As an edge u-->v is different from v-->u we have n*(n-1) choices for edges (no loops allowed!)
    static IEnumerable<DirectedGraph<int>> AllDirectedGraphs(int vertices)
    {
        var choices = Product(new List<int> { 0, 1 }, vertices * (vertices - 1));
        foreach (var choice in choices)
        {
            var graph = new DirectedGraph<int>();
            for (int v = 0, index = 0; v < vertices; ++v)
            {
                for (int u = 0; u < vertices; ++u)
                {
                    if (u == v) { continue; }
                    if (choice[index] == 1)
                    {
                        graph.AddEdge(v, u);
                    }
                    ++index;
                }
            }
            yield return graph;
        }
    }

    static public void UndirectedGraph_FindPath(int vertices = 5)
    {
        foreach (var graph in AllUndirectedGraphs(vertices))
        {
            // Find paths from all `from`, `to` choices and check.
            for (int v=0; v<vertices; ++v)
            {
                for (int u = 0; u < vertices; ++u)
                {
                    var path = ((IGraphEdges<int>)graph).FindPath(v, u);
                    if ( path.Count()==0 )
                    {
                        // So claim no path
                        //Console.WriteLine("No path from {0}--{1}: {2}", v, u, graph);
                    }
                    else if ( path.Count()==1 )
                    {
                        if ( v != u )
                        {
                            throw new Exception(string.Format("Path from {0}--{1} returned as singleton {2}",v,u,path[0]));
                        }
                    }
                    {
                        for (int i=0; i<path.Count()-1; ++i)
                        {
                            if ( ! ((IGraphEdges<int>)graph).IsNeighbour(path[i], path[i+1]) )
                            {
                                throw new Exception(string.Format("Path claims {0}, {1} neighbours in {2}.", path[i], path[i + 1], graph));
                            }
                        }
                        
                    }
                }
            }
        }
    }

    static public void DirectedGraph_FindPath(int vertices = 5)
    {
        foreach (var graph in AllDirectedGraphs(vertices))
        {
            // Find paths from all `from`, `to` choices and check.
            for (int v = 0; v < vertices; ++v)
            {
                for (int u = 0; u < vertices; ++u)
                {
                    var path = graph.FindPath(v, u);
                    if (path.Count() == 0)
                    {
                        // So claim no path
                        //Console.WriteLine("No path from {0}--{1}: {2}", v, u, graph);
                    }
                    else if (path.Count() == 1)
                    {
                        if (v != u)
                        {
                            throw new Exception(string.Format("Path from {0}--{1} returned as singleton {2}", v, u, path[0]));
                        }
                    }
                    {
                        for (int i = 0; i < path.Count() - 1; ++i)
                        {
                            if (!((IGraphEdges<int>)graph).IsNeighbour(path[i], path[i + 1]))
                            {
                                throw new Exception(string.Format("Path claims {0}, {1} neighbours in {2}.", path[i], path[i + 1], graph));
                            }
                        }

                    }
                }
            }
        }
    }

    private static void TestEdgeDisjointPath(UndirectedGraph<int> g, int from, int to)
    {
        Console.WriteLine("Example graph: {0}", g);
        Console.Write("Path from {0} to {1}: ", from, to);
        foreach (int v in g.FindPath(from, to))
        {
            Console.Write("{0}, ", v);
        }
        Console.Write("\n");

        Console.WriteLine("Number of edge-disjoint paths {0} to {1}: {2}", from, to, g.CountEdgeDisjointPaths(from, to));
        Console.WriteLine("They are:");
        foreach (var path in g.FindEdgeDisjointPaths(from, to))
        {
            foreach (int v in path)
            {
                Console.Write("{0}, ", v);
            }
            Console.Write("\n");
        }
        Console.Write("\n");
    }

    private static void TestEdgeDisjointPath(DirectedGraph<int> g, int from, int to)
    {
        Console.WriteLine("Example graph: {0}", g);
        Console.Write("Path from {0} to {1}: ", from, to);
        foreach (int v in g.FindPath(from, to))
        {
            Console.Write("{0}, ", v);
        }
        Console.Write("\n");

        Console.WriteLine("Number of edge-disjoint paths {0} to {1}: {2}", from, to, g.CountEdgeDisjointPaths(from, to));
        Console.WriteLine("They are:");
        foreach (var path in g.FindEdgeDisjointPaths(from, to))
        {
            foreach (int v in path)
            {
                Console.Write("{0}, ", v);
            }
            Console.Write("\n");
        }
        Console.Write("\n");
    }

    public static void TestEdgeDisjointPath1()
    {
        var g = new UndirectedGraph<int>();
        g.AddEdges(new int[] { 0, 1, 0, 2, 1, 3, 2, 3, 3, 4, 3, 5, 4, 6, 5, 6 });
        TestEdgeDisjointPath(g, 0, 6);
    }

    public static void TestEdgeDisjointPath2()
    {
        var g = new UndirectedGraph<int>();
        g.AddEdges(new int[] { 0, 1 });
        TestEdgeDisjointPath(g, 0, 1);
    }

    public static void TestEdgeDisjointPath3()
    {
        var g = new DirectedGraph<int>();
        g.AddEdges(new int[] { 0, 1 });
        TestEdgeDisjointPath(g, 0, 1);
        TestEdgeDisjointPath(g, 1, 0);
    }

    // Check list of paths are paths and that they are edge-disjoint.
    // Throws an exception, or if okay, returns a list of used (directed) edges.
    private static HashSet<Tuple<int, int>> CheckEDP(IGraphEdges<int> g, int startVertex, int endVertex, List<List<int>> paths, bool log)
    {
        if (log)
        {
            Console.Write("Edge-Disjoint paths: ");
            foreach (var path in paths)
            {
                foreach (var v in path) { Console.Write("{0} ", v); }
                Console.Write("\n");
            }
        }
        // Check are paths
        foreach (var path in paths)
        {
            if ( path[0] != startVertex || path[path.Count()-1] != endVertex )
            {
                throw new Exception("Path does not start/end at correct vertex.");
            }
            for (int i = 0; i < path.Count() - 1; ++i)
            {
                if ( !g.IsNeighbour(path[i], path[i+1]) )
                {
                    throw new Exception("Path uses an edge which doesn't exist.");
                }
            }
        }
        // Check are disjoint
        var usedEdges = new HashSet<Tuple<int, int>>();
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count() - 1; ++i)
            {
                var edge = Tuple.Create(path[i], path[i + 1]);
                if (!usedEdges.Add(edge))
                {
                    throw new Exception("Paths are not edge-disjoint!");
                }
            }
        }
        return usedEdges;
    }

    public static void TestEdgeDisjointPath4(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new UndirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                g.AddEdge(rnd.Next(vertices), rnd.Next(vertices));
            }
            if (log) { Console.WriteLine(g); }

            var paths = g.FindEdgeDisjointPaths(0, vertices - 1);
            if (paths.Count() > 0)
            {
                var usedEdges = CheckEDP(g, 0, vertices - 1, paths, log);
                var cut = g.FindCutEDP(0, usedEdges);
                if (log)
                {
                    Console.WriteLine("Size of cut: {0}", cut.Count());
                    Console.Write("Cut: ");
                    foreach (var v in cut) { Console.Write("{0} ", v); }
                    Console.Write("\n");
                }
                if (cut.Count() != paths.Count())
                {
                    throw new Exception("Cut is not the same size of set of edge-disjoint paths!");
                }
                foreach (var edge in cut)
                {
                    g.RemoveEdge(edge.Item1, edge.Item2);
                }
                if (g.FindPath(0, vertices - 1).Count() > 0)
                {
                    throw new Exception(String.Format("Graph {0} not disconnected by cut!", g));
                }
            }
            else
            {
                if (log) { Console.WriteLine("No path, apparently."); }
                var path = g.FindPath(0, vertices - 1);
                if (path.Count() > 0)
                {
                    throw new Exception("No path found, but there is a path!");
                }
            }
        }
    }

    // Hardly DRY, but this is a test, so cut'n'paste it is.
    public static void TestEdgeDisjointPath5(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new DirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                g.AddEdge(rnd.Next(vertices), rnd.Next(vertices));
            }
            if (log) { Console.WriteLine(g); }

            var paths = g.FindEdgeDisjointPaths(0, vertices - 1);
            if (paths.Count() > 0)
            {
                var usedEdges = CheckEDP(g, 0, vertices - 1, paths, log);
                var cut = g.FindCutEDP(0, usedEdges);
                if (log)
                {
                    Console.WriteLine("Size of cut: {0}", cut.Count());
                    Console.Write("Cut: ");
                    foreach (var v in cut) { Console.Write("{0} ", v); }
                    Console.Write("\n");
                }
                if (cut.Count() != paths.Count())
                {
                    throw new Exception("Cut is not the same size of set as edge-disjoint paths!");
                }
                foreach (var edge in cut)
                {
                    g.RemoveEdge(edge.Item1, edge.Item2);
                }
                if (g.FindPath(0, vertices - 1).Count() > 0)
                {
                    throw new Exception(String.Format("Graph {0} not disconnected by cut!", g));
                }
            }
            else
            {
                if (log) { Console.WriteLine("No path, apparently."); }
                var path = g.FindPath(0, vertices - 1);
                if (path.Count() > 0)
                {
                    throw new Exception("No path found, but there is a path!");
                }
            }
        }
    }

    public static void TestEdgeDisjointPath6(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new UndirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                g.AddEdge(rnd.Next(vertices), rnd.Next(vertices));
            }
            if (log) { Console.WriteLine(g); }

            var paths = g.FindEdgeDisjointPaths1(0, vertices - 1);
            if (paths.Count() > 0)
            {
                var usedEdges = CheckEDP(g, 0, vertices - 1, paths, log);
                var cut = g.FindCutEDP(0, usedEdges);
                if (log)
                {
                    Console.WriteLine("Size of cut: {0}", cut.Count());
                    Console.Write("Cut: ");
                    foreach (var v in cut) { Console.Write("{0} ", v); }
                    Console.Write("\n");
                }
                if (cut.Count() != paths.Count())
                {
                    throw new Exception("Cut is not the same size of set as edge-disjoint paths!");
                }
                foreach (var edge in cut)
                {
                    g.RemoveEdge(edge.Item1, edge.Item2);
                }
                if (g.FindPath(0, vertices - 1).Count() > 0)
                {
                    throw new Exception(String.Format("Graph {0} not disconnected by cut!", g));
                }
            }
            else
            {
                if (log) { Console.WriteLine("No path, apparently."); }
                var path = g.FindPath(0, vertices - 1);
                if (path.Count() > 0)
                {
                    throw new Exception("No path found, but there is a path!");
                }
            }
        }
    }

    public static void TestEdgeDisjointPath7(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new DirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            //vertices = 10; edges = 50;
            for (int i = 0; i < edges; ++i)
            {
                g.AddEdge(rnd.Next(vertices), rnd.Next(vertices));
            }
            if (log) { Console.WriteLine(g); }

            var paths = g.FindEdgeDisjointPaths1(0, vertices - 1);
            if (paths.Count() > 0)
            {
                var usedEdges = CheckEDP(g, 0, vertices - 1, paths, log);
                var cut = g.FindCutEDP(0, usedEdges);
                if (log)
                {
                    Console.WriteLine("Size of cut: {0}", cut.Count());
                    Console.Write("Cut: ");
                    foreach (var v in cut) { Console.Write("{0} ", v); }
                    Console.Write("\n");
                }
                if (cut.Count() != paths.Count())
                {
                    throw new Exception("Cut is not the same size of set as edge-disjoint paths!");
                }
                foreach (var edge in cut)
                {
                    g.RemoveEdge(edge.Item1, edge.Item2);
                }
                if (g.FindPath(0, vertices - 1).Count() > 0)
                {
                    throw new Exception(String.Format("Graph {0} not disconnected by cut!", g));
                }
            }
            else
            {
                if (log) { Console.WriteLine("No path, apparently."); }
                var path = g.FindPath(0, vertices - 1);
                if (path.Count() > 0)
                {
                    throw new Exception("No path found, but there is a path!");
                }
            }
        }
    }

    public static HashSet<int> CheckVDP(IGraphEdges<int> g, List<List<int>> paths, bool log)
    {
        if (log)
        {
            Console.WriteLine("Found {0} vertex-disjoint path(s), which are: ", paths.Count());
            foreach (var path in paths)
            {
                Console.Write("-- ");
                foreach (var v in path) { Console.Write("{0} ", v); }
                Console.Write("\n");
            }
        }
        // Check disjoint!
        var usedVertices = new HashSet<int>();
        foreach (var path in paths)
        {
            for (int i = 1; i < path.Count() - 1; ++i)
            {
                if (!usedVertices.Add(path[i]))
                {
                    throw new Exception(String.Format("Vertex {0} used twice", path[i]));
                }
            }
        }
        // Check are paths!
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count() - 1; ++i)
            {
                if (!g.IsNeighbour(path[i], path[i + 1]))
                {
                    throw new Exception(String.Format("Edge {0}--{1} not in graph!", path[i], path[i + 1]));
                }
            }
        }
        return usedVertices;
    }

    public static void TestVertexDisjointPath1(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new UndirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                int start = rnd.Next(vertices);
                int end = rnd.Next(vertices);
                if (start != end) { g.AddEdge(start, end); }
            }
            g.RemoveEdge(0, vertices - 1); // Remove trivial path!
            if (log) { Console.WriteLine(g); }

            var paths = g.FindVertexDisjointPaths(0, vertices - 1);
            var usedVertices = CheckVDP(g, paths, log);
            // Find a cut
            var cut = g.FindCutVDP(0, paths);
            if (log) { Console.WriteLine("Size of cut: {0}", cut.Count()); }
            if (paths.Count() != cut.Count())
            {
                throw new Exception("Cut and disjoint path count differ.");
            }
            foreach (var v in cut) { g.RemoveVertex(v); }
            if (g.FindPath(0, vertices - 1).Count() > 0)
            {
                Console.WriteLine(g);
                throw new Exception("Graph not disconnected!");
            }
        }
    }

    public static void TestVertexDisjointPath2(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new DirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                int start = rnd.Next(vertices);
                int end = rnd.Next(vertices);
                if (start != end) { g.AddEdge(start, end); }
            }
            g.RemoveEdge(0, vertices - 1); // Remove trivial path!
            if (log) { Console.WriteLine(g); }

            var paths = g.FindVertexDisjointPaths(0, vertices - 1);
            var usedVertices = CheckVDP(g, paths, log);
            // Find a cut
            var cut = g.FindCutVDP(0, paths);
            if (log) { Console.WriteLine("Size of cut: {0}", cut.Count()); }
            if (paths.Count() != cut.Count())
            {
                throw new Exception("Cut and disjoint path count differ.");
            }
            foreach (var v in cut) { g.RemoveVertex(v); }
            if (g.FindPath(0, vertices - 1).Count() > 0)
            {
                Console.WriteLine(g);
                throw new Exception("Graph not disconnected!");
            }
        }
    }

    public static void TestVertexDisjointPath3(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new UndirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                int start = rnd.Next(vertices);
                int end = rnd.Next(vertices);
                if (start != end) { g.AddEdge(start, end); }
            }
            g.RemoveEdge(0, vertices - 1); // Remove trivial path!
            if (log) { Console.WriteLine(g); }

            var paths = g.FindVertexDisjointPaths1(0, vertices - 1);
            var usedVertices = CheckVDP(g, paths, log);
            // Find a cut
            var cut = g.FindCutVDP(0, paths);
            if (log) { Console.WriteLine("Size of cut: {0}", cut.Count()); }
            if (paths.Count() != cut.Count())
            {
                throw new Exception("Cut and disjoint path count differ.");
            }
            foreach (var v in cut) { g.RemoveVertex(v); }
            if (g.FindPath(0, vertices - 1).Count() > 0)
            {
                Console.WriteLine(g);
                throw new Exception("Graph not disconnected!");
            }
        }
    }

    public static void TestVertexDisjointPath4(bool log = true, int loops = 1000)
    {
        for (int loop = 0; loop < loops; ++loop)
        {
            var g = new DirectedGraph<int>();
            var rnd = new Random();
            int vertices = 10 + rnd.Next(1000);
            int edges = vertices / 2 + rnd.Next(vertices * vertices / 4);
            for (int i = 0; i < edges; ++i)
            {
                int start = rnd.Next(vertices);
                int end = rnd.Next(vertices);
                if (start != end) { g.AddEdge(start, end); }
            }
            g.RemoveEdge(0, vertices - 1); // Remove trivial path!
            if (log) { Console.WriteLine(g); }

            var paths = g.FindVertexDisjointPaths1(0, vertices - 1);
            var usedVertices = CheckVDP(g, paths, log);
            // Find a cut
            var cut = g.FindCutVDP(0, paths);
            if (log) { Console.WriteLine("Size of cut: {0}", cut.Count()); }
            if (paths.Count() != cut.Count())
            {
                throw new Exception("Cut and disjoint path count differ.");
            }
            foreach (var v in cut) { g.RemoveVertex(v); }
            if (g.FindPath(0, vertices - 1).Count() > 0)
            {
                Console.WriteLine(g);
                throw new Exception("Graph not disconnected!");
            }
        }
    }
}

class Program
{
    static void Test(bool log = true)
    {
        TestGraph.UndirectedGraph_FindPath(5);
        TestGraph.DirectedGraph_FindPath(5);
        TestGraph.TestEdgeDisjointPath1();
        TestGraph.TestEdgeDisjointPath2();
        TestGraph.TestEdgeDisjointPath3();
        TestGraph.TestEdgeDisjointPath4(log, 100);
        TestGraph.TestEdgeDisjointPath5(log, 100);
        TestGraph.TestEdgeDisjointPath6(log, 100);
        TestGraph.TestEdgeDisjointPath7(log, 100);
        TestGraph.TestVertexDisjointPath1(log, 100);
        TestGraph.TestVertexDisjointPath2(log, 100);
        TestGraph.TestVertexDisjointPath3(log, 100);
        TestGraph.TestVertexDisjointPath4(log, 100);
    }
    
    static void Main()
    {
        Test(false);
    }
}
