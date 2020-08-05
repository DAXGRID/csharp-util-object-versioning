using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class InMemoryChangesTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(0, 1)]
        [InlineData(1, 3)]
        [InlineData(2, 1)]
        [InlineData(-5, 1)]
        [InlineData(1, -5)]
        public void InvalidFromAndToVersionIds_ShouldFail(int fromVersionId, int toVersionId)
        {
            var manager = new InMemoryObjectManager();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // FIRST TRANSACTION
            var firstTransaction = manager.CreateTransaction();

            var firstObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "first object"));
            var secondObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "second object"));
            var thirdObj = firstTransaction.Add(new TestObjectA(Guid.NewGuid(), "third object"));

            firstTransaction.Commit();

            Assert.Throws<ArgumentException>(() => manager.GetChanges(fromVersionId, toVersionId));
        }


        [Fact]
        public void DiffBetweenTwoAdjacentVersions_CheckResult()
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

            secondTransaction.Commit();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get the changes and assert correctness
            var changes = manager.GetChanges(1, 2);

            // We did 3 changes: add, delete and update.
            Assert.Equal(3, changes.Count());

            Assert.Contains(changes, c => c is ObjectCreation && ((ObjectCreation)c).NewState == forthObj);
            Assert.Contains(changes, c => c is ObjectDeletion && c.Id == secondObj.Id);
            Assert.Contains(changes, c => c is ObjectModification && ((ObjectModification)c).NewState == thirdObjUpdated && ((ObjectModification)c).PreviousState == thirdObj);

        }


        [Fact]
        public void DiffBetweenManyVersions_CheckResult()
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
            var thirdObjUpdatedFirstTime = new TestObjectA(thirdObj.Id, "third object updated first time");
            secondTransaction.Update(thirdObjUpdatedFirstTime);

            secondTransaction.Commit();


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // THIRD TRANSACTION
            var thirdTransaction = manager.CreateTransaction();

            // Insert fifth object
            var fifthObj = thirdTransaction.Add(new TestObjectA(Guid.NewGuid(), "fifth object"));

            // Delete first object
            thirdTransaction.Delete(firstObj.Id);

            // Update third object again
            var thirdObjUpdatedSecondTime = new TestObjectA(thirdObj.Id, "third object updated second time");
            thirdTransaction.Update(thirdObjUpdatedSecondTime);

            thirdTransaction.Commit();


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get the changes between version 1 and 2 and assert correctness
            var changes = manager.GetChanges(1, 2);

            // We did 3 changes: add, delete and update.
            Assert.Equal(3, changes.Count());

            Assert.Contains(changes, c => c is ObjectCreation && ((ObjectCreation)c).NewState == forthObj);
            Assert.Contains(changes, c => c is ObjectDeletion && c.Id == secondObj.Id);
            Assert.Contains(changes, c => c is ObjectModification && ((ObjectModification)c).NewState == thirdObjUpdatedFirstTime && ((ObjectModification)c).PreviousState == thirdObj);

      
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get the changes between version 1 and 3 and assert correctness
            changes = manager.GetChanges(1, 3);

            // We did 5 changes: two add, two delete and one update.
            Assert.Equal(5, changes.Count());

            Assert.Contains(changes, c => c is ObjectCreation && ((ObjectCreation)c).NewState == forthObj);
            Assert.Contains(changes, c => c is ObjectCreation && ((ObjectCreation)c).NewState == fifthObj);
            Assert.Contains(changes, c => c is ObjectDeletion && c.Id == firstObj.Id);
            Assert.Contains(changes, c => c is ObjectDeletion && c.Id == secondObj.Id);
            Assert.Contains(changes, c => c is ObjectModification && ((ObjectModification)c).NewState == thirdObjUpdatedSecondTime && ((ObjectModification)c).PreviousState == thirdObj);


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get the changes between version 2 and 3 and assert correctness
            changes = manager.GetChanges(2, 3);

            // We did 3 changes: add, delete and update.
            Assert.Equal(3, changes.Count());

            Assert.Contains(changes, c => c is ObjectCreation && ((ObjectCreation)c).NewState == fifthObj);
            Assert.Contains(changes, c => c is ObjectDeletion && c.Id == firstObj.Id);
            Assert.Contains(changes, c => c is ObjectModification && ((ObjectModification)c).NewState == thirdObjUpdatedSecondTime && ((ObjectModification)c).PreviousState == thirdObjUpdatedFirstTime);
        }


     
    }
}
