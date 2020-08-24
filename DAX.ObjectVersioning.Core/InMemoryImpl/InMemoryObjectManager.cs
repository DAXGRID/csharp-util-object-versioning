using DAX.ObjectVersioning.Core.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public class InMemoryObjectManager : IVersionedObjectManager
    {
        internal ConcurrentDictionary<Guid, IVersionedObject[]> _objectAdditionStates = new ConcurrentDictionary<Guid, IVersionedObject[]>();

        internal ConcurrentDictionary<Guid, long[]> _objectDeletionStates = new ConcurrentDictionary<Guid, long[]>();

        internal ITransaction _runningTransaction = null;

        internal IVersion _lastCommitedVersion = null;

        /// <summary>
        /// These dictionaries don't need to be thread safe, because only one transaction is fiddeling with them
        /// </summary>
        internal Dictionary<long, IVersion> _versionsByInternalId = new Dictionary<long, IVersion>();

        internal Dictionary<string, IVersion> _versionsByCustomId = new Dictionary<string, IVersion>();


        public ITransaction CreateTransaction(string customVersionId = null)
        {
            if (_runningTransaction != null)
                throw new InvalidOperationException("A transaction is already running. This object manager does not support concurrent transactions.");

            if (customVersionId != null && _versionsByCustomId.ContainsKey(customVersionId))
                throw new ArgumentException("Custom version id already used. Must be unique!");

            _runningTransaction = new InMemoryTransaction(this, GetNextVersion(customVersionId));

            return _runningTransaction;
        }

        public IVersionedObject GetObject(Guid id)
        {
            if (_lastCommitedVersion != null)
                return FetchObjectByVersion(id, _lastCommitedVersion.InternalVersionId);
            else
                return null;
        }

        public IVersionedObject GetObject(Guid id, long versionId)
        {
            return FetchObjectByVersion(id, versionId);
        }

        public IVersionedObject GetObject(Guid id, string customVersionId)
        {
            if (!_versionsByCustomId.ContainsKey(customVersionId))
                throw new ArgumentException("No such version id found: " + customVersionId);

            long versionId = _versionsByCustomId[customVersionId].InternalVersionId;

            return FetchObjectByVersion(id, versionId);
        }


        public IEnumerable<IVersionedObject> GetObjects()
        {
            return FetchObjectsByVersion();
        }

        public IEnumerable<IVersionedObject> GetObjects(long versionId)
        {
            if (!_versionsByInternalId.ContainsKey(versionId))
                throw new ArgumentException("No such version id found: " + versionId);

            return FetchObjectsByVersion(versionId);
        }

        public IEnumerable<IVersionedObject> GetObjects(string customVersionId)
        {
            if (!_versionsByCustomId.ContainsKey(customVersionId))
                throw new ArgumentException("No such version id found: " + customVersionId);

            long versionId = _versionsByCustomId[customVersionId].InternalVersionId;

            return FetchObjectsByVersion(versionId);
        }

        public IEnumerable<Change> GetChanges(int fromVersionId, int toVersionId)
        {
            // Snatch current version
            var currentVersion = GetLatestVersion();

            if (fromVersionId < 1)
                throw new ArgumentException("From version id is invalid. Must be positiv.");
            if (fromVersionId >= currentVersion)
                throw new ArgumentException("From version id cannot be greater than or equal to latest version.");
            if (toVersionId < 1)
                throw new ArgumentException("To version id is invalid. Must be positiv.");
            if (toVersionId > currentVersion)
                throw new ArgumentException("To version id cannot be greater than latest version.");
            if (fromVersionId == toVersionId)
                throw new ArgumentException("From and to version id cannot be the same. Nothing to diff.");
            if (fromVersionId > toVersionId)
                throw new ArgumentException("From version id cannot be greater than to version id. Nothing to diff.");


            List<Change> result = new List<Change>();

            // Find objects created and modified by comparing objects in to version with from version
            var toList = GetObjects(toVersionId);
            
            foreach (var toObject in toList)
            {
                var fromObject = GetObject(toObject.Id, fromVersionId);

                if (fromObject == null)
                    result.Add(new ObjectCreation(toObject.Id, toObject));
                else if (fromObject.CreationVersion.InternalVersionId != toObject.CreationVersion.InternalVersionId)
                    result.Add(new ObjectModification(toObject.Id, fromObject, toObject));
            }

            // Find objects deleted by comparing objects in from version with to version
            var fromList = GetObjects(fromVersionId);

            foreach (var fromObject in fromList)
            {
                var toObject = GetObject(fromObject.Id, toVersionId);

                if (toObject == null)
                    result.Add(new ObjectDeletion(fromObject.Id));
            }

            return result;
        }


        private IEnumerable<IVersionedObject> FetchObjectsByVersion(long internaVersionIdParam = -1)
        {
            if (_lastCommitedVersion == null)
                return new List<IVersionedObject>();
            else
            {
                // We need to snatch the current version, because it might change while we build result list
                long versionToReturn = internaVersionIdParam > 0 ? internaVersionIdParam : _lastCommitedVersion.InternalVersionId;

                List<IVersionedObject> result = new List<IVersionedObject>();
                                
                foreach (var objId in _objectAdditionStates.Keys)
                {
                    var objToReturn = FetchObjectByVersion(objId, versionToReturn);

                    if (objToReturn != null)
                        result.Add(objToReturn);
                }

                return result;
            }
        }

        private IVersionedObject FetchObjectByVersion(Guid id, long version)
        {
            // Get object deletions within scope of the version we're logging at
            long[] deletionsWithinVersionScope = _objectDeletionStates.ContainsKey(id) ? _objectDeletionStates[id].Where(s => s <= version).ToArray() : null;

            IVersionedObject objToReturn = null;

            if (_objectAdditionStates.ContainsKey(id))
            {
                var objVersions = _objectAdditionStates[id];

                // Loop through all the different versions of the object to find most recent one within scope of the version we're looking at
                foreach (var obj in objVersions)
                {
                    // We're only interested in objects within the scope of the version we're looking at
                    if (obj.CreationVersion.InternalVersionId <= version)
                    {
                        // Unless a deletion has happend, then the object is a candidate for returning
                        if (!(deletionsWithinVersionScope != null && deletionsWithinVersionScope.Any(s => s > obj.CreationVersion.InternalVersionId)))
                            objToReturn = obj;
                    }
                }
            }

            return objToReturn;
        }

        private IVersion GetNextVersion(string customVersionId)
        {
            if (_lastCommitedVersion == null)
                return new InMemoryVersion(1, customVersionId);
            else
                return new InMemoryVersion(_lastCommitedVersion.InternalVersionId + 1, customVersionId);
        }

        private long GetLatestVersion()
        {
            if (_lastCommitedVersion == null)
                return 0;
            else
                return _lastCommitedVersion.InternalVersionId;
        }

        public long GetLatestCommitedVersion()
        {
            return GetLatestVersion();
        }
    }
}
