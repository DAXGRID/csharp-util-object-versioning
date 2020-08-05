using DAX.ObjectVersioning.Core;
using System;

namespace DAX.ObjectVersioning.Graph
{
    public abstract class GraphObject : IGraphObject
    {
        // Id
        private readonly Guid _id;
        public Guid Id => _id;

        // Creation state provided by versioned object manager
        private IVersion _creationState;
        public IVersion CreationVersion { get => _creationState; set => _creationState = value; }

        // Deletion state provided by versioned object manager
        private IVersion _deletionState;
        public IVersion DeletionVersion { get => _deletionState; set => _deletionState = value; }

        public GraphObject(Guid id)
        {
            _id = id;
        }
    }
}
