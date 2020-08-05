using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Graph
{
    public interface IGraphEdge : IGraphObject
    {
        IGraphNode InV(long version);
        IGraphNode OutV(long version);
    }
}
