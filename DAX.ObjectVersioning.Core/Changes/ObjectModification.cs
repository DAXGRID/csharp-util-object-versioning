using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public class ObjectModification : Change
    {
        private readonly IVersionedObject _previousState;
        public IVersionedObject PreviousState => _previousState;

        private readonly IVersionedObject _newState;
        public IVersionedObject NewState => _newState;

        public ObjectModification(Guid id, IVersionedObject previousState, IVersionedObject newState) : base(id)
        {
            _previousState = previousState;
            _newState = newState;
        }

        public override string ToString()
        {
            return "Modification: '" + _previousState + "' -> '" + _newState + "'";
        }

    }
}
