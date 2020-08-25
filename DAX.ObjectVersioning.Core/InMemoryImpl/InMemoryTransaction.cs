using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAX.ObjectVersioning.Core.Memory
{
    public class InMemoryTransaction : ITransaction
    {
        private readonly InMemoryObjectManager _objectManager;
        private readonly IVersion _version;
        private TransactionState _transactionState;
        Dictionary<Guid,IVersionedObject> _addedObjects = new Dictionary<Guid,IVersionedObject>();
        Dictionary<Guid, IVersionedObject> _updatedObjects = new Dictionary<Guid, IVersionedObject>();

        List<Guid> _deletedIds = new List<Guid>();

        internal InMemoryTransaction(InMemoryObjectManager objectManager, IVersion state)
        {
            _objectManager = objectManager;
            _version = state;
            _transactionState = TransactionState.Running;
        }

        public IVersionedObject? GetObject(Guid id)
        {
            if (_addedObjects.ContainsKey(id))
                return _addedObjects[id];
            else
                return null;
        }

        public IVersion Version => _version;

        public IVersionedObject Add(IVersionedObject @object)
        {
            if (_transactionState != TransactionState.Running)
                throw new OperationCanceledException($"Cannot add objects when transaction state = {_transactionState.ToString() }");

            if (_objectManager.GetObject(@object.Id) != null)
                throw new OperationCanceledException($"An object with id: {@object.Id} is already present in the object manager. If you want to update an existing object, please use Update instead.");

            if (_addedObjects.ContainsKey(@object.Id))
                throw new OperationCanceledException($"An object with id: {@object.Id} is already added in this transaction.");

            // Check if pointer to same object has been added before
            if (_objectManager._objectAdditionStates.ContainsKey(@object.Id))
            {
                var objVersions = _objectManager._objectAdditionStates[@object.Id];

                if (objVersions.Contains(@object))
                    throw new OperationCanceledException($"An object with id: {@object.Id} referencing the same object already exists inside the object manager. You cannot add the same object instance twice. You need to create a new object instance everytime you add an object.");
            }

            _addedObjects.Add(@object.Id, @object);

            return @object;
        }


        public void Update(IVersionedObject @object)
        {
            if (_transactionState != TransactionState.Running)
                throw new OperationCanceledException($"Cannot update objects when transaction state = {_transactionState.ToString() }");

            if (_updatedObjects.ContainsKey(@object.Id))
                throw new OperationCanceledException($"An object with id: {@object.Id} is already updated once in this transaction.");

            if (!_objectManager._objectAdditionStates.ContainsKey(@object.Id))
                throw new OperationCanceledException($"An object with id: {@object.Id} is not present in the object manager. You cannot update an object, unless it has been added in a previous transaction.");

            // Check if pointer to same object has been added before
            if (_objectManager._objectAdditionStates.ContainsKey(@object.Id))
            {
                var objVersions = _objectManager._objectAdditionStates[@object.Id];

                if (objVersions.Contains(@object))
                    throw new OperationCanceledException($"An object with id: {@object.Id} referencing the same object already exists inside the object manager. You cannot update an object using an existing instance. You need to create a new object instance everytime you want to update an object.");
            }

            _updatedObjects.Add(@object.Id, @object);
        }

        public void Delete(Guid id)
        {
            if (_transactionState != TransactionState.Running)
                throw new OperationCanceledException($"Cannot update objects when transaction state = {_transactionState.ToString() }");

            if (!_objectManager._objectAdditionStates.ContainsKey(id) && !_addedObjects.ContainsKey(id))
                throw new OperationCanceledException($"An object with id: {id} is not present in the object manager or in the transaction.");

            if (_deletedIds.Contains(id))
                throw new OperationCanceledException($"An object with id: {id} is already deleted in this transaction.");

            if (!_addedObjects.ContainsKey(id) && _objectManager.GetObject(id) == null)
                throw new OperationCanceledException($"An object with id: {id} is already deleted.");

            /*
            if (_addedObjects.ContainsKey(id))
            {
                //throw new OperationCanceledException($"An object with id: {id} is already added in this transaction. Adding and deleting an object in the same transaction is not allowed.");
                _addedObjects.Remove(id);
            }
            else
            {
                _deletedIds.Add(id);
            }
            */

            _deletedIds.Add(id);
        }

        public void Commit()
        {
            // Process added objects
            foreach (var obj in _addedObjects)
            {
                // Set creation state
                obj.Value.CreationVersion = _version;

                // Add id of deleted object to manager
                if (!_objectManager._objectAdditionStates.ContainsKey(obj.Value.Id))
                {
                    // Add object to manager
                    if (!_objectManager._objectAdditionStates.TryAdd(obj.Value.Id, new IVersionedObject[] { obj.Value }))
                        throw new ApplicationException($"Unxepected error adding object with id: {obj.Value.Id} to object additions concurrent dictionary using TryAdd. The returned false indicates that an object with that id already exists in the collection.");
                }
                else
                {
                    // Create new array with space for one extra object
                    IVersionedObject[] oldObjectArray = _objectManager._objectAdditionStates[obj.Value.Id];

                    IVersionedObject[] newObjectArray = new IVersionedObject[oldObjectArray.Length + 1];

                    // Copy values from old array to new
                    for (int i = 0; i < oldObjectArray.Length; i++)
                        newObjectArray[i] = oldObjectArray[i];

                    // Add new object to array
                    newObjectArray[oldObjectArray.Length] = obj.Value;

                    // Add new array to object dict
                    if (!_objectManager._objectAdditionStates.TryUpdate(obj.Value.Id, newObjectArray, oldObjectArray))
                        throw new ApplicationException($"Unxepected error updating object with id: {obj.Value.Id} to object additions concurrent dictionary using TryUpdate. The returned false indicates something went wrong.");
                }
            }

            // Process updated objects
            foreach (var obj in _updatedObjects)
            {
                // Set creation state
                obj.Value.CreationVersion = _version;

                // Add object to manager
                if (!_objectManager._objectAdditionStates.ContainsKey(obj.Value.Id))
                    throw new ApplicationException ($"Unxepected error processing updated objects. Object with id = {obj.Value.Id} do not exist in object manager.");

                // Create new array with space for one extra object
                IVersionedObject[] oldObjectArray = _objectManager._objectAdditionStates[obj.Value.Id];

                IVersionedObject[] newObjectArray = new IVersionedObject[oldObjectArray.Length + 1];

                // Copy values from old array to new
                for (int i = 0; i < oldObjectArray.Length; i++)
                    newObjectArray[i] = oldObjectArray[i];

                // Add new object to array
                newObjectArray[oldObjectArray.Length] = obj.Value;

                // Add new array to object dict
                if (!_objectManager._objectAdditionStates.TryUpdate(obj.Value.Id, newObjectArray, oldObjectArray))
                    throw new ApplicationException($"Unxepected error adding updated object with id: {obj.Value.Id} to object additions concurrent dictionary using TryUpdate. The returned false indicates something went wrong.");

            }

            // Process deleted objects
            foreach (var id in _deletedIds)
            {
                var objToDelete = _objectManager.GetObject(id);

                if (objToDelete == null && _addedObjects.ContainsKey(id))
                    objToDelete = _addedObjects[id];


                if (objToDelete != null)
                    objToDelete.DeletionVersion = _version;
                else
                    throw new ApplicationException($"Unxepected error handling deletion of object with id = {id}. The object does not exist in manager or transaction!");

                // Add id of deleted object to manager
                if (!_objectManager._objectDeletionStates.ContainsKey(id))
                {
                    if (!_objectManager._objectDeletionStates.TryAdd(id, new long[] { _version.InternalVersionId }))
                        throw new ApplicationException($"Unxepected error adding object with id: {id} to deletion states concurrent dictionary using TryAdd. The returned false indicates that an object with that id already exists in the collection.");
                }
                else
                {
                    // Get old id list
                    long[] oldObjectArray = _objectManager._objectDeletionStates[id];

                    // Create new array with space for one extra id
                    long[] newObjectArray = new long[oldObjectArray.Length + 1];

                    // Copy data from old array to new array
                    for (int i = 0; i < oldObjectArray.Length; i++)
                        newObjectArray[i] = oldObjectArray[i];

                    // Add new id to array
                    newObjectArray[oldObjectArray.Length] = _version.InternalVersionId;

                    // Add new array to object dict
                    if (!_objectManager._objectDeletionStates.TryUpdate(id, newObjectArray, oldObjectArray))
                        throw new ApplicationException($"Unxepected error adding updated object with id: {id} in deletion states concurrent dictionary using TryUpdate. The returned false indicates something went wrong.");
                }

            }

            _transactionState = TransactionState.Commited;

            _objectManager._runningTransaction = null;
            _objectManager._lastCommitedVersion = _version;
            _objectManager._versionsByInternalId.Add(_version.InternalVersionId, _version);

            if (_version.CustomVersionId != null)
                _objectManager._versionsByCustomId.Add(_version.CustomVersionId, _version);
        }
              

        public void Rollback()
        {
            _transactionState = TransactionState.Rolledback;
            _objectManager._runningTransaction = null;
        }
    }
}
