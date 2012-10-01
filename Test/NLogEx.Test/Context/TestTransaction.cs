//===========================================================================
// MODULE:  TestTransaction.cs
// PURPOSE: transaction logging context unit test driver
// 
// Copyright © 2012
// Brent M. Spell. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 3 of the License, or 
// (at your option) any later version. This library is distributed in the 
// hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU Lesser General Public License for more details. You should 
// have received a copy of the GNU Lesser General Public License along with 
// this library; if not, write to 
//    Free Software Foundation, Inc. 
//    51 Franklin Street, Fifth Floor 
//    Boston, MA 02110-1301 USA
//===========================================================================
// System References
using System;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestTransaction
   {
      static TestTransaction ()
      {
         Context.Transaction.Register();
      }

      [TestMethod]
      public void TestContext ()
      {
         var queue = new Loggers.Queue();
         var log = new Log(this);
         var evt = (Event)null;
         using (Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[]
                  {
                     "Transaction.LocalID",
                     "Transaction.GlobalID",
                     "Transaction.Started",
                     "Transaction.State"
                  },
                  Logger = queue,
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            // default transaction state
            log.Info("test");
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Transaction.LocalID"]);
            Assert.IsNull(evt["Transaction.GlobalID"]);
            Assert.IsNull(evt["Transaction.Started"]);
            Assert.IsNull(evt["Transaction.State"]);
            // running transaction state
            using (var txn = new TransactionScope())
               log.Info("test");
            evt = queue.Dequeue().Single();
            Assert.IsNotNull(evt["Transaction.LocalID"]);
            Assert.IsNull(evt["Transaction.GlobalID"]);
            Assert.IsTrue((DateTime)evt["Transaction.Started"] <= DateTime.UtcNow);
            Assert.AreEqual(evt["Transaction.State"], TransactionStatus.Active);
            // committed transaction state
            using (var txn = new CommittableTransaction())
            {
               Transaction.Current = txn;
               txn.Commit();
               log.Info("test");
               Transaction.Current = null;
            }
            evt = queue.Dequeue().Single();
            Assert.IsNotNull(evt["Transaction.LocalID"]);
            Assert.IsNull(evt["Transaction.GlobalID"]);
            Assert.IsTrue((DateTime)evt["Transaction.Started"] <= DateTime.UtcNow);
            Assert.AreEqual(evt["Transaction.State"], TransactionStatus.Committed);
            // aborted transaction state
            using (var txn = new TransactionScope())
            {
               Transaction.Current.Rollback();
               log.Info("test");
            }
            evt = queue.Dequeue().Single();
            Assert.IsNotNull(evt["Transaction.LocalID"]);
            Assert.IsNull(evt["Transaction.GlobalID"]);
            Assert.IsTrue((DateTime)evt["Transaction.Started"] <= DateTime.UtcNow);
            Assert.AreEqual(evt["Transaction.State"], TransactionStatus.Aborted);
            // globally-enlisted transaction state
            using (var txn = new TransactionScope())
            using (var connect1 = new SqlConnection("Data Source=.;Integrated Security=true;"))
            using (var connect2 = new SqlConnection("Data Source=.;Integrated Security=true;"))
            {
               connect1.Open();
               connect2.Open();
               log.Info("test");
            }
            evt = queue.Dequeue().Single();
            Assert.IsNotNull(evt["Transaction.LocalID"]);
            Assert.IsNotNull(evt["Transaction.GlobalID"]);
            Assert.IsTrue((DateTime)evt["Transaction.Started"] <= DateTime.UtcNow);
            Assert.AreEqual(evt["Transaction.State"], TransactionStatus.Active);
            // null transaction state
            log.Info("test");
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Transaction.LocalID"]);
            Assert.IsNull(evt["Transaction.GlobalID"]);
            Assert.IsNull(evt["Transaction.Started"]);
            Assert.IsNull(evt["Transaction.State"]);
         }
      }
   }
}
