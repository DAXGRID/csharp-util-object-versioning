using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public class ObjectCreation : Change
    {
        private readonly IVersionedObject _newState;
        public IVersionedObject NewState => _newState;

        public ObjectCreation(Guid id, IVersionedObject newState) : base(id)
        {
            _newState = newState;
        }

        public override string ToString()
        {
            return "Creation: " + _newState;
        }
    }
}
