using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Graph
{
    public class GraphEdge : GraphObject, IGraphEdge
    {
        private readonly GraphNode _inV;
        private readonly GraphNode _outV;

        public GraphEdge(Guid id, GraphNode fromNode, GraphNode toNode) : base(id)
        {
            _inV = fromNode;
            _outV = toNode;
         
            fromNode.AddOutgoingEdge(this);
            toNode.AddIngoingEdge(this);
        }

        public IGraphNode InV(long version)
        {
            return _inV;
        }

        public IGraphNode OutV(long version)
        {
            return _outV;
        }
    }
}
