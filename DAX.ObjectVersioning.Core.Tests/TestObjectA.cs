using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class TestObjectA : IVersionedObject
    {
        // Id
        private readonly Guid _id;
        public Guid Id => _id;

        // Name
        private readonly string _name;
        public string Name => _name;

        // Creation state provided by versioned object manager
        private IVersion _creationState;
        public IVersion CreationVersion { get => _creationState;  set => _creationState = value; }

        // Deletion state provided by versioned object manager
        private IVersion _deletionState;
        public IVersion DeletionVersion { get => _deletionState;  set => _deletionState = value; }

        public TestObjectA(Guid id, string name)
        {
            _id = id;
            _name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
    
}
