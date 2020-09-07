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
        IVersionedObject Add(IVersionedObject @object, bool ignoreDublicates = false);
        void Delete(Guid id, bool ignoreDublicates = false);
        void Update(IVersionedObject @object, bool ignoreDublicates = false);
        IVersionedObject? GetObject(Guid id);
    }
}
