using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Graph
{
    public class GraphEdge : GraphObject, IGraphEdge
    {
        private GraphNode _inV;
        private GraphNode _outV;

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

        public override List<IGraphObject> NeighborElements(long version)
        {
            var neighbors = new List<IGraphObject>();

            neighbors.Add(InV(version));
            neighbors.Add(OutV(version));

            return neighbors;
        }

        /// <summary>
        /// Copy ingoing and outgoing node relationships from this edge to another edge
        /// </summary>
        /// <param name="copyTo"></param>
        public void CopyNodeRelationshipsTo(GraphEdge copyTo)
        {
            copyTo._inV = this._inV;
            copyTo._outV = this._outV;
        }
    }
}
