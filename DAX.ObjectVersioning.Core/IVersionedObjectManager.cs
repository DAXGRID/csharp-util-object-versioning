using System;
using System.Collections.Generic;

namespace DAX.ObjectVersioning.Core
{
    public interface IVersionedObjectManager
    {
        ITransaction CreateTransaction(string customVersionId = null);

        IVersionedObject GetObject(Guid id);
        IVersionedObject GetObject(Guid id, long internalVersionId);
        IVersionedObject GetObject(Guid id, string customVersionId);

        IEnumerable<IVersionedObject> GetObjects();
        IEnumerable<IVersionedObject> GetObjects(long internalVersionId);
        IEnumerable<IVersionedObject> GetObjects(string customVersionId);

        IEnumerable<Change> GetChanges(int fromVersionId, int toVersionId);

        long GetLatestCommitedVersion();
    }
}
