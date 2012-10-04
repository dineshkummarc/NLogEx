//===========================================================================
// MODULE:  TestLogBehavior.cs
// PURPOSE: WCF log behavior unit test driver
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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx.Wcf.Test
{
   [TestClass]
   public class TestLogBehavior
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;
      private const String TestServiceUri = "net.pipe://localhost/NLogEx.Wcf.Test/";
      private NetNamedPipeBinding binding = new NetNamedPipeBinding();

      [TestMethod]
      public void TestBehavior ()
      {
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Message",
            "Event.Exception",
            "Operation.ID",
            "Operation.Version",
            "Operation.Action",
            "Operation.Name",
            "Operation.From",
            "Operation.To",
            "Operation.Duration"
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
            // client-side logging
            using (var server = StartService(false))
            using (var client = ConnectSimplex(true))
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.AutoLog();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ISimplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // service-side logging
            using (var server = StartService(true))
            using (var client = ConnectSimplex(false))
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.AutoLog();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(Server));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // client-side logging (one-way)
            using (var server = StartService(false))
            using (var client = ConnectSimplex(true))
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ISimplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "FireAndForget");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] >= 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // client-side exception logging
            using (var server = StartService(false))
               for (Int32 i = 0; i < TestIterations; i++)
                  using (var client = ConnectSimplex(true))
                  {
                     try
                     {
                        client.Server.Throw("test");
                        Assert.Fail("Expected: exception");
                     }
                     catch { }
                  }
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ISimplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "Throw");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Error);
               Assert.AreEqual(evt["Event.Source"], typeof(ISimplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNotNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "Throw");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // service-side exception logging
            using (var server = StartService(true))
               for (Int32 i = 0; i < TestIterations; i++)
                  using (var client = ConnectSimplex(false))
                  {
                     try
                     {
                        client.Server.Throw("test");
                        Assert.Fail("Expected: exception");
                     }
                     catch { }
                  }
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(Server));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "Throw");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Error);
               Assert.AreEqual(evt["Event.Source"], typeof(Server));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNotNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "Throw");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // client-side callback logging
            using (var server = StartService(false))
            using (var client = ConnectDuplex(true))
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.CallbackAutoLog();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ICallback));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(IDuplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "CallbackAutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // server-side callback logging
            using (var server = StartService(true))
            using (var client = ConnectDuplex(false))
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.CallbackAutoLog();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ICallback));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(Server));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "CallbackAutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
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
            "Event.Exception",
            "Operation.ID",
            "Operation.Version",
            "Operation.Action",
            "Operation.Name",
            "Operation.From",
            "Operation.To",
            "Operation.Duration"
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
            // client-side logging
            using (var server = StartService(false))
            using (var client = ConnectSimplex(true))
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.AutoLog()
               );
            Log.Flush();
            for (Int32 i = 0; i < TestParallelIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(ISimplexServer));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // service-side logging
            using (var server = StartService(true))
            using (var client = ConnectSimplex(false))
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.AutoLog()
               );
            Log.Flush();
            for (Int32 i = 0; i < TestParallelIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(Server));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // client-side callback logging
            using (var server = StartService(false))
            using (var client = ConnectDuplex(true))
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.CallbackAutoLog()
               );
            Log.Flush();
            for (Int32 i = 0; i < 2 * TestParallelIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               if ((Type)evt["Event.Source"] == typeof(ICallback))
                  Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               else if ((Type)evt["Event.Source"] == typeof(IDuplexServer))
                  Assert.AreEqual(evt["Operation.Name"], "CallbackAutoLog");
               else
                  Assert.Fail("Invalid event source {0}", evt["Event.Source"]);
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // service-side callback logging
            using (var server = StartService(true))
            using (var client = ConnectDuplex(false))
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.CallbackAutoLog()
               );
            Log.Flush();
            for (Int32 i = 0; i < 2 * TestParallelIterations; i++)
            {
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.IsNotNull(evt["Operation.ID"]);
               Assert.IsNotNull(evt["Operation.Version"]);
               Assert.IsNotNull(evt["Operation.Action"]);
               Assert.IsNull(evt["Operation.From"]);
               Assert.IsNotNull(evt["Operation.To"]);
               Assert.IsTrue((Int64)evt["Operation.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Operation.Duration"] < 10000000);
               if ((Type)evt["Event.Source"] == typeof(ICallback))
                  Assert.AreEqual(evt["Operation.Name"], "AutoLog");
               else if ((Type)evt["Event.Source"] == typeof(Server))
                  Assert.AreEqual(evt["Operation.Name"], "CallbackAutoLog");
               else
                  Assert.Fail("Invalid event source {0}", evt["Event.Source"]);
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      private ServiceHost StartService (Boolean enableLog)
      {
         var endpoints = new[]
         {
            new ServiceEndpoint(
               ContractDescription.GetContract(typeof(ISimplexServer)),
               binding,
               new EndpointAddress(TestServiceUri)
            ),
            new ServiceEndpoint(
               ContractDescription.GetContract(typeof(IDuplexServer)),
               binding,
               new EndpointAddress(TestServiceUri)
            ),
         };
         if (enableLog)
            foreach (var endpoint in endpoints)
               endpoint.Behaviors.Add(new NLogEx.Wcf.LogBehavior());
         var host = new ServiceHost(typeof(Server));
         foreach (var endpoint in endpoints)
            host.AddServiceEndpoint(endpoint);
         host.Open();
         return host;
      }

      private SimplexClient ConnectSimplex (Boolean enableLog)
      {
         var endpoint = new ServiceEndpoint(
            ContractDescription.GetContract(typeof(ISimplexServer)),
            binding,
            new EndpointAddress(TestServiceUri)
         );
         if (enableLog)
            endpoint.Behaviors.Add(new NLogEx.Wcf.LogBehavior());
         return new SimplexClient(endpoint);
      }

      private DuplexClient ConnectDuplex (Boolean enableLog)
      {
         var endpoint = new ServiceEndpoint(
            ContractDescription.GetContract(typeof(IDuplexServer)),
            binding,
            new EndpointAddress(TestServiceUri)
         );
         if (enableLog)
            endpoint.Behaviors.Add(new NLogEx.Wcf.LogBehavior());
         return new DuplexClient(endpoint);
      }
   }
}
