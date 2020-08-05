using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public enum TransactionState
    {
        Running = 1,
        Commited = 2,
        Rolledback = 3
    }
}
