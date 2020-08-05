using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public interface ITransaction
    {        
        IVersion Version { get; }
        void Rollback();
        void Commit();
        IVersionedObject Add(IVersionedObject @object);
        void Delete(Guid id);
        void Update(IVersionedObject @object);
    }
}
