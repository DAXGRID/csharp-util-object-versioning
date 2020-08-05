using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class InMemoryCustomVersionIdTests
    {
        [Fact]
        public void CreateTransactionWithCustomId_TestGetObject()
        {
            var manager = new InMemoryObjectManager();

            var myCustomId = "my custom version id 1";

            var firstTransaction = manager.CreateTransaction(myCustomId);

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();


            // Asser that GetObjects work with custom ids
            var objects = manager.GetObjects(myCustomId);
            Assert.Single(objects);

            var obj = manager.GetObject(objToAdd.Id, myCustomId);
            Assert.Equal(objToAdd, obj);
        }

        [Fact]
        public void CreateTwoTransactionWithSameCustomId_MustFail()
        {
            var manager = new InMemoryObjectManager();

            // First transaction
            var firstTransaction = manager.CreateTransaction("my custom version id 1");

            var objToAdd = new TestObjectA(Guid.NewGuid(), "hej");

            firstTransaction.Add(objToAdd);
            firstTransaction.Commit();

            // Create second transaction with the same version id
            Assert.Throws<ArgumentException>(() => manager.CreateTransaction("my custom version id 1"));

        }
    }
}
