using DAX.ObjectVersioning.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Graph
{
    public interface IGraphObject : IVersionedObject
    {
        List<IGraphObject> NeighborElements(long version);

        IEnumerable<IGraphObject> UndirectionalDFS<TNode, TEdge>(long version, Predicate<TNode> nodeCriteria = null, Predicate<TEdge> edgePredicate = null, bool includeElementsWhereCriteriaIsFalse = false) where TNode : IGraphObject where TEdge : IGraphObject;
    }
}
