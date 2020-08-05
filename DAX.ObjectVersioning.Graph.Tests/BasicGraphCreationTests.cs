using DAX.ObjectVersioning.Core;
using System;
using System.Linq;
using Xunit;

namespace DAX.ObjectVersioning.Graph.Tests
{
    public class BasicGraphCreationTests
    {
        [Fact]
        public void CreateSimpleGraph_CheckNodeAndEdgeTraversal()
        {
            //    N1
            //    |
            // E1 | 
            //    |      E2
            //    N2-------------N3

            var manager = new InMemoryObjectManager();

            var trans = manager.CreateTransaction();

            var n1 = new GraphNode(Guid.NewGuid());
            trans.Add(n1);

            var n2 = new GraphNode(Guid.NewGuid());
            trans.Add(n2);

            var n3 = new GraphNode(Guid.NewGuid());
            trans.Add(n3);

            var e1 = new GraphEdge(Guid.NewGuid(), n1, n2);
            trans.Add(e1);

            var e2 = new GraphEdge(Guid.NewGuid(), n2, n3);
            trans.Add(e2);

            trans.Commit();

            var version = trans.Version.InternalVersionId;

            // N1 should have zero ingoing edges
            Assert.Empty(n1.InE(version));

            // N1 should have one outgoing edge
            Assert.Single(n1.OutE(version));
            Assert.Contains(e1, n1.OutE(version).Where(e => e.Id == e1.Id));

            // N2 should have one ingoing edge
            Assert.Single(n2.InE(version));
            Assert.Contains(e1, n2.InE(version).Where(e => e.Id == e1.Id));

            // N2 should have one outgoing edge
            Assert.Single(n2.OutE(version));
            Assert.Contains(e2, n2.OutE(version).Where(e => e.Id == e2.Id));

            // N3 should have one ingoing edges
            Assert.Single(n3.InE(version));
            Assert.Contains(e2, n3.InE(version).Where(e => e.Id == e2.Id));

            // N3 should have zero outgoing edges
            Assert.Empty(n3.OutE(version));

            // E1 should have n1 as ingoing node
            Assert.Equal(n1, e1.InV(version));

            // E1 should have n2 as ingoing node
            Assert.Equal(n2, e1.OutV(version));

            // E2 should have n2 as ingoing node
            Assert.Equal(n2, e2.InV(version));

            // E2 should have n3 as ingoing node
            Assert.Equal(n3, e2.OutV(version));
        }
    }
}
