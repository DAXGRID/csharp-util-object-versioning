using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class InMemoryTransactionTests
    {
        [Fact]
        public void WorkAfterRollback_ShouldNotBeCommited()
        {
            var manager = new InMemoryObjectManager();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // FIRST TRANSACTION
            var firstTransaction = manager.CreateTransaction();

            var firstObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "first object"));
            var secondObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "second object"));
            var thirdObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "third object"));

            firstTransaction.Commit();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // SECOND TRANSACTION
            var secondTransaction = manager.CreateTransaction();

            // Insert forth object
            var forthObj = secondTransaction.Add(new TestObjectA(Guid.NewGuid(), "forth object"));

            // Delete second object
            secondTransaction.Delete(secondObj.Id);

            // Update third object
            var thirdObjUpdated = new TestObjectA(thirdObj.Id, "third object updated");
            secondTransaction.Update(thirdObjUpdated);

            secondTransaction.Rollback();

            // In version 1 we should have 3 object
            Assert.Equal(3, manager.GetObjects(1).Count());
            // In version 1 name of first object should be "first object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == firstObj.Id && ((TestObjectA)o).Name == "first object");
            // In version 1 name of second object should be "second object"
            Assert.Contains(manager.GetObjects(1), o => o.Id == secondObj.Id && ((TestObjectA)o).Name == "second object");

            // Version 2 should not exist, because we did a rollback of the second transaction
            Assert.Throws<ArgumentException>(() => manager.GetObjects(2));
        }

        [Fact]
        public void AddAfterCommit_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "hello"));
            firstTransaction.Commit();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "kitty")));
        }

        [Fact]
        public void AddAfterRollback_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "hello"));
            firstTransaction.Rollback();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "kitty")));
        }

        [Fact]
        public void DeleteAfterCommit_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hello");
            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Delete(objToAdd.Id));
        }

        [Fact]
        public void DeleteAfterRollback_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hello");
            firstTransaction.Add(objToAdd);
            firstTransaction.Rollback();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Delete(objToAdd.Id));
        }

        [Fact]
        public void UpdateAfterCommit_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hello");
            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Update(new TestObjectA(objToAdd.Id, "hello kitty")));
        }

        [Fact]
        public void UpdateAfterRollback_MustFail()
        {
            var manager = new InMemoryObjectManager();

            var firstTransaction = manager.CreateTransaction();

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hello");
            firstTransaction.Add(objToAdd);
            firstTransaction.Rollback();

            Assert.Throws<OperationCanceledException>(() => firstTransaction.Update(new TestObjectA(objToAdd.Id, "hello kitty")));
        }
    }
}
