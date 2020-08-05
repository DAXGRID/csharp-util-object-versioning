using System;
using System.Linq;
using Xunit;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class InMemoryCRUDTests
    {
        [Fact]
        public void AddedObject_ShouldGetCreationStateAssignedOnCommit()
        {
            var manager = new InMemoryObjectManager();

            var transaction = manager.CreateTransaction();

            // Internal version id should be 1
            Assert.Equal(1, transaction.Version.InternalVersionId);

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            transaction.Add(objToAdd);

            // Creation version should not be assigned yet
            Assert.Null(objToAdd.CreationVersion);

            // Deletion version should be null
            Assert.Null(objToAdd.DeletionVersion);

            transaction.Commit();

            // Creation version should now be assigned
            Assert.Equal(transaction.Version, objToAdd.CreationVersion);

            // Deletion version should still be null
            Assert.Null(objToAdd.DeletionVersion);
        }

        [Fact]
        public void AddingTheSameObjectTwiceInTheSameTransactions_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            firstTransaction.Add(objToAdd);
            Assert.Throws<OperationCanceledException>(() => firstTransaction.Add(objToAdd));
        }

        [Fact]
        public void AddingTheSameObjectTwiceInTwoTransactions_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();

            var secondTransaction = manager.CreateTransaction();

            Assert.Throws<OperationCanceledException>(() => secondTransaction.Add(objToAdd));
        }

        [Fact]
        public void AddingObjects_GetObjectsMustReturnCorrectNumberOfObjects()
        {
            var manager = new InMemoryObjectManager();

            // We should have zero object in the manager
            Assert.Empty(manager.GetObjects());

            // FIRST TRANSACTION
            var firstTransaction = manager.CreateTransaction();

            firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "first object"));

            Assert.Empty(manager.GetObjects()); // We should still have zero object in the manager, because transaction is not commited yet

            firstTransaction.Commit();

            Assert.Single(manager.GetObjects()); // We should have one object in the manager after transaction commit


            // SECOND TRANSACTION
            var secondTransaction = manager.CreateTransaction();

            secondTransaction.Add(new TestObjectA(Guid.NewGuid(), "second object"));

            Assert.Single(manager.GetObjects()); // We should have one object in the manager before transaction commit

            secondTransaction.Commit();

            Assert.Equal(2, manager.GetObjects().Count()); // We should have two object in the manager before transaction commit


            // THIRD TRANSACTION
            var thirdTransaction = manager.CreateTransaction();

            thirdTransaction.Add(new TestObjectA(Guid.NewGuid(), "third object"));
            thirdTransaction.Add(new TestObjectA(Guid.NewGuid(), "forth object"));

            Assert.Equal(2, manager.GetObjects().Count()); // We should have two object in the manager before transaction commit

            thirdTransaction.Commit();

            Assert.Equal(4, manager.GetObjects().Count()); // We should have four object in the manager before transaction commit

            // Assert that GetObjects by internal version id works
            Assert.Equal(1, manager.GetObjects(1).Count()); // In version 1 we should have 1 object
            Assert.Equal(2, manager.GetObjects(2).Count()); // In version 2 we should have 2 object
            Assert.Equal(4, manager.GetObjects(3).Count()); // In version 3 we should have 4 object
        }

        [Fact]
        public void UpdateObject_GetObjectsMustReturnCorrectResult()
        {
            var manager = new InMemoryObjectManager();

            // FIRST TRANSACTION: Add two object
            var firstTransaction = manager.CreateTransaction();

            var firstObj = new TestObjectA(Guid.NewGuid(), "first object");
            firstTransaction.Add(firstObj);

            var secondObj = new TestObjectA(Guid.NewGuid(), "second object");
            firstTransaction.Add(secondObj);

            firstTransaction.Commit();

            // SECOND TRANSACTION: Update name of second object
            var secondTransaction = manager.CreateTransaction();

            var updatedObj = new TestObjectA(secondObj.Id, "second object updated");

            secondTransaction.Update(updatedObj);

            secondTransaction.Commit();

            // In version 1 we should have 2 object
            Assert.Equal(2, manager.GetObjects(1).Count());
            // In version 1 name of first object should be "first object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == firstObj.Id && ((TestObjectA)o).Name == "first object");
            // In version 1 name of second object should be "second object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");

            // In version 2 we should still have 2 object
            Assert.Equal(2, manager.GetObjects(2).Count());
            // In version 2 name of first object should still be "first object"
            Assert.Contains(manager.GetObjects(2), o => o.Id == firstObj.Id && ((TestObjectA)o).Name == "first object");
            // In version 2 name of second object should be changed to "second object updated"
            Assert.Contains(manager.GetObjects(2), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object updated");
        }

        [Fact]
        public void UpdateObjectWithSameInstance_MustFail()
        {
            var manager = new InMemoryObjectManager();

            // FIRST TRANSACTION: Add two object
            var firstTransaction = manager.CreateTransaction();

            var firstObj = new TestObjectA(Guid.NewGuid(), "first object");

            firstTransaction.Add(firstObj);

            firstTransaction.Commit();

            // SECOND TRANSACTION: Update object using same instance - must fail
            var secondTransaction = manager.CreateTransaction();

            Assert.Throws<OperationCanceledException>(() => secondTransaction.Update(firstObj));
        }

        [Fact]
        public void DeleteObject_GetObjectsMustReturnCorrectResult()
        {
            var manager = new InMemoryObjectManager();

            // FIRST TRANSACTION: Add two object
            var firstTransaction = manager.CreateTransaction();

            var firstObj = new TestObjectA(Guid.NewGuid(), "first object");
            firstTransaction.Add(firstObj);

            var secondObj = new TestObjectA(Guid.NewGuid(), "second object");
            firstTransaction.Add(secondObj);

            firstTransaction.Commit();

            // SECOND TRANSACTION: Delete first object
            var secondTransaction = manager.CreateTransaction();

            secondTransaction.Delete(firstObj.Id);

            secondTransaction.Commit();

            // Deletion version should not be null
            Assert.NotNull(firstObj.DeletionVersion);

            // In version 1 we should have 2 object
            Assert.Equal(2, manager.GetObjects(1).Count());
            // In version 1 name of first object should be "first object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == firstObj.Id && ((TestObjectA)o).Name == "first object");
            // In version 1 name of second object should be "second object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");

            // In version 2 we should have only 1 object
            Assert.Equal(1, manager.GetObjects(2).Count());
            // In version 2 we should have the second object named "second object"
            Assert.Contains(manager.GetObjects(2), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");
        }

        [Fact]
        public void AddDeleteAddSameObjectInstance_MustFail()
        {
            // Due to the way version is managed it's crutial that added objects are new instances

            var manager = new InMemoryObjectManager();
            
            // FIRST TRANSACTION: Add two object
            var firstTransaction = manager.CreateTransaction();

            var firstObj = new TestObjectA(Guid.NewGuid(), "first object");

            firstTransaction.Add(firstObj);

            firstTransaction.Commit();

            // SECOND TRANSACTION: Delete first object
            var secondTransaction = manager.CreateTransaction();

            secondTransaction.Delete(firstObj.Id);

            secondTransaction.Commit();

            // THIRD TRANSACTION: Add first object again - same instance - must fail
            var thirdTransaction = manager.CreateTransaction();

            Assert.Throws<OperationCanceledException>(() => thirdTransaction.Add(firstObj));
        }
        

        [Fact]
        public void AddDeleteTest_GetObjectsMustReturnCorrectResult()
        {
            var manager = new InMemoryObjectManager();

            // FIRST TRANSACTION: Add two object
            var firstTransaction = manager.CreateTransaction();

            var firstObj = new TestObjectA(Guid.NewGuid(), "first object");
            firstTransaction.Add(firstObj);

            var secondObj = new TestObjectA(Guid.NewGuid(), "second object");
            firstTransaction.Add(secondObj);

            firstTransaction.Commit();

            // SECOND TRANSACTION: Delete first object
            var secondTransaction = manager.CreateTransaction();

            secondTransaction.Delete(firstObj.Id);

            secondTransaction.Commit();

            // THIRD TRANSACTION: Delete second object
            var thirdTransaction = manager.CreateTransaction();

            thirdTransaction.Delete(secondObj.Id);

            thirdTransaction.Commit();

            // FORTH TRANSACTION: Add second object again
            var forthTransaction = manager.CreateTransaction();

            forthTransaction.Add(new TestObjectA(secondObj.Id, "second object version 2"));

            forthTransaction.Commit();

            // In version 1 we should have 2 object
            Assert.Equal(2, manager.GetObjects(1).Count());
            // In version 1 name of first object should be "first object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == firstObj.Id && ((TestObjectA)o).Name == "first object");
            // In version 1 name of second object should be "second object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");

            // In version 2 we should have only 1 object
            Assert.Equal(1, manager.GetObjects(2).Count());
            // In version 2 we should have the second object named "second object"
            Assert.Contains(manager.GetObjects(2), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");

            // In version 3 we should have no objects
            Assert.Empty(manager.GetObjects(3));

            // In version 4 we should have 1 object
            Assert.Equal(1, manager.GetObjects(4).Count());
            // In version 4 we should have the second object named "second object version 2"
            Assert.Contains(manager.GetObjects(4), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object version 2");
        }

        [Fact]
        public void AddingObject_TestGetObject()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction("custom version id");

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();

            // Asset that GetObject(guid id) workd
            Assert.Equal(objToAdd, manager.GetObject(objToAdd.Id));

            // Asset that GetObject(guid id, long version id) works
            Assert.Equal(objToAdd, manager.GetObject(objToAdd.Id, 1));

            // Asset that GetObject(custom, string version id ) works
            Assert.Equal(objToAdd, manager.GetObject(objToAdd.Id, "custom version id"));
        }
    }
}