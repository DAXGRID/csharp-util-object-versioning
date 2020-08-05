using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public interface ISnapshotter
    {
        Stream Serialize(long internalVersionId);
        Stream Serialize(string customVersionId);
    }
}
