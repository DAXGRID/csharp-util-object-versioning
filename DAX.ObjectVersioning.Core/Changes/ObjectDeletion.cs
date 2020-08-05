using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public class ObjectDeletion : Change
    {
        public ObjectDeletion(Guid id) : base(id)
        {
        }

        public override string ToString()
        {
            return "Deletion: " + Id;
        }

    }
}
