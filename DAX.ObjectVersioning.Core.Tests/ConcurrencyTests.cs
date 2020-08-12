using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace DAX.ObjectVersioning.Core.Tests
{
    public class ConcurrencyTests
    {
        [Fact]
        public void TestConcurrentReaderOneWriter()
        {
            var manager = new InMemoryObjectManager();

            // Add initial object
            var transaction = manager.CreateTransaction();
            var objToAdd = new TestObjectA(Guid.NewGuid(), "initial");
            transaction.Add(objToAdd);
            transaction.Commit();

            // Update name of object to inital 2
            transaction = manager.CreateTransaction();
            var objToUpdate = new TestObjectA(objToAdd.Id, "initial 2");
            transaction.Update(objToUpdate);
            transaction.Commit();


            // Thread that create a new transaction every 10 milisecond
            new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var transaction = manager.CreateTransaction();
                    var objToUpdate = new TestObjectA(objToAdd.Id, "trans version " + transaction.Version.InternalVersionId);

                    transaction.Update(objToUpdate);
                    //Give some time to other threads to kick in  
                    Thread.Sleep(10);

                    transaction.Commit();

                    Assert.Single(manager.GetObjects(transaction.Version.InternalVersionId));
                }
            }).Start();

            // Reading thread #1
            new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Assert.Single(manager.GetObjects(1));
                    Assert.Equal("initial", ((TestObjectA)manager.GetObjects(1).First()).Name);
                    Thread.Sleep(10);
                }
            }).Start();

            // Reading thread #2
            new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Assert.Single(manager.GetObjects(1));
                    Assert.Equal("initial", ((TestObjectA)manager.GetObjects(1).First()).Name);
                    Thread.Sleep(10);
                }
            }).Start();

            // Reading thread #3
            new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Assert.Single(manager.GetObjects(2));
                    Assert.Equal("initial 2", ((TestObjectA)manager.GetObjects(2).First()).Name);
                    Thread.Sleep(10);
                }
            }).Start();

            Thread.Sleep(1500);
        }
    }
}
