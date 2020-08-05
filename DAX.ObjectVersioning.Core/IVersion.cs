using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public interface IVersion
    {
        Int64 InternalVersionId { get; }
        string CustomVersionId { get; }
    }
}
