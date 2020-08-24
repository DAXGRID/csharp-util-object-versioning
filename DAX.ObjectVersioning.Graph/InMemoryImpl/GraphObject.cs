using DAX.ObjectVersioning.Core;
using DAX.ObjectVersioning.Graph.Traversal;
using System;
using System.Collections.Generic;

namespace DAX.ObjectVersioning.Graph
{
    public abstract class GraphObject : IGraphObject
    {
        // Id
        private readonly Guid _id;
        public Guid Id => _id;

        // Creation state provided by versioned object manager
        private IVersion _creationState;
        public IVersion CreationVersion { get => _creationState; set => _creationState = value; }

        // Deletion state provided by versioned object manager
        private IVersion _deletionState;
        public IVersion DeletionVersion { get => _deletionState; set => _deletionState = value; }
        
        public GraphObject(Guid id)
        {
            _id = id;
        }

        public abstract List<IGraphObject> NeighborElements(long version);

        public IEnumerable<IGraphObject> UndirectionalDFS<TNode, TEdge>(long version, Predicate<TNode> nodeCriteria = null, Predicate<TEdge> edgePredicate = null, bool includeElementsWhereCriteriaIsFalse = false) where TNode : IGraphObject where TEdge : IGraphObject
        {
            var traversal = new BasicTraversal<TNode, TEdge>(this);

            return traversal.UndirectedDFS(version, nodeCriteria, edgePredicate, includeElementsWhereCriteriaIsFalse);
        }
    }
}
