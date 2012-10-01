//===========================================================================
// MODULE:  TestLogContext.cs
// PURPOSE: WCF log context unit test driver
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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx.Wcf.Test
{
   [TestClass]
   public class TestLogContext
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;
      private const String TestServiceUri = "net.tcp://localhost:42000/NLogEx.Wcf.Test/";
      private ServiceHost serviceHost = new ServiceHost(typeof(Server));
      private NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true);

      static TestLogContext ()
      {
         LogContext.Register();
      }

      [TestInitialize]
      public void Initialize ()
      {
         serviceHost.AddServiceEndpoint(
            typeof(ISimplexServer),
            binding,
            TestServiceUri
         );
         serviceHost.AddServiceEndpoint(
            typeof(IDuplexServer),
            binding,
            TestServiceUri
         );
         serviceHost.Open();
      }

      [TestCleanup]
      public void Cleanup ()
      {
         serviceHost.Close();
      }

      [TestMethod]
      public void TestProperties ()
      {
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Message",
            "Wcf.SessionID",
            "Wcf.LocalAddress",
            "Wcf.RemoteAddress",
         };
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = 0,
                  Properties = props
               }
            )
         )
         {
            Event evt;
            // service context
            using (var client = new SimplexClient(binding, TestServiceUri))
            {
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.ManualLog("test");
               for (Int32 i = 0; i < TestIterations; i++)
               {
                  evt = queue.Dequeue().First();
                  Assert.AreEqual(evt["Event.Type"], EventType.Info);
                  Assert.AreEqual(evt["Event.Source"], typeof(Server));
                  Assert.AreEqual(evt["Event.Message"], "test");
                  Assert.AreEqual(evt["Wcf.SessionID"], client.Context.SessionId);
                  Assert.AreEqual(evt["Wcf.LocalAddress"].ToString(), TestServiceUri);
                  Assert.IsNotNull(evt["Wcf.RemoteAddress"]);
               }
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // callback context
            using (var client = new DuplexClient(binding, TestServiceUri))
            {
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.CallbackManualLog("test");
               for (Int32 i = 0; i < TestIterations; i++)
               {
                  evt = queue.Dequeue().First();
                  Assert.AreEqual(evt["Event.Type"], EventType.Info);
                  Assert.AreEqual(evt["Event.Source"], typeof(DuplexClient.Callback));
                  Assert.AreEqual(evt["Event.Message"], "test");
                  Assert.AreEqual(evt["Wcf.SessionID"], client.Context.SessionId);
                  Assert.IsNotNull(evt["Wcf.LocalAddress"]);
                  Assert.AreEqual(evt["Wcf.RemoteAddress"].ToString(), TestServiceUri);
               }
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      [TestMethod]
      public void TestConcurrency ()
      {
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Message",
            "Wcf.SessionID",
            "Wcf.LocalAddress",
            "Wcf.RemoteAddress",
         };
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = false,
                  Buffer = 0,
                  Properties = props
               }
            )
         )
         {
            Event evt;
            // service context
            using (var client = new SimplexClient(binding, TestServiceUri))
            {
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.ManualLog("test")
               );
               Log.Flush();
               for (Int32 i = 0; i < TestParallelIterations; i++)
               {
                  evt = queue.Dequeue().First();
                  Assert.AreEqual(evt["Event.Type"], EventType.Info);
                  Assert.AreEqual(evt["Event.Source"], typeof(Server));
                  Assert.AreEqual(evt["Event.Message"], "test");
                  Assert.AreEqual(evt["Wcf.SessionID"], client.Context.SessionId);
                  Assert.AreEqual(evt["Wcf.LocalAddress"].ToString(), TestServiceUri);
                  Assert.IsNotNull(evt["Wcf.RemoteAddress"]);
               }
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // callback context
            using (var client = new DuplexClient(binding, TestServiceUri))
            {
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.CallbackManualLog("test")
               );
               Log.Flush();
               for (Int32 i = 0; i < TestParallelIterations; i++)
               {
                  evt = queue.Dequeue().First();
                  Assert.AreEqual(evt["Event.Type"], EventType.Info);
                  Assert.AreEqual(evt["Event.Source"], typeof(DuplexClient.Callback));
                  Assert.AreEqual(evt["Event.Message"], "test");
                  Assert.AreEqual(evt["Wcf.SessionID"], client.Context.SessionId);
                  Assert.IsNotNull(evt["Wcf.LocalAddress"]);
                  Assert.AreEqual(evt["Wcf.RemoteAddress"].ToString(), TestServiceUri);
               }
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }
   }
}
