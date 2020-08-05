using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public abstract class Change
    {
        // Id
        private readonly Guid _id;
        public Guid Id => _id;

        public Change(Guid id)
        {
            _id = id;
        }
    }
}
