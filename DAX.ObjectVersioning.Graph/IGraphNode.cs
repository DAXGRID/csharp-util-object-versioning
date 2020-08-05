using DAX.ObjectVersioning.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Graph
{
    public interface IGraphNode : IGraphObject
    {
        /// <summary>
        /// Gets the outgoing edges to the vertex/node.
        /// Gremlin style.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        IEnumerable<IGraphEdge> OutE(long version);

        /// <summary>
        /// Gets the incoming edges of the vertex/node. 
        /// Gremlin style.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        IEnumerable<IGraphEdge> InE(long version);

        /// <summary>
        /// Gets the out adjacent vertices to the vertex.
        /// Gremlin style.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        IEnumerable<IGraphNode> Out(long version);
    }
}
