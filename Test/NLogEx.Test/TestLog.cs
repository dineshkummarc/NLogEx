//===========================================================================
// MODULE:  TestLog.cs
// PURPOSE: logging framework unit test driver
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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestLog
   {
      private static Int32 globalValue = 0;
      public const Int32 ParallelIterations = 100;
      public const Int32 PerformanceIterations = 100;
      public String Property { get; set; }

      static TestLog ()
      {
         Log.RegisterContext(
            "Global",
            new Dictionary<String, Func<Object>>()
            {
               { "Value", () => Interlocked.Increment(ref globalValue) },
               { "Host", () => Environment.MachineName }
            }
         );
      }

      [TestMethod]
      public void TestConfiguration ()
      {
         var log = new Log(this);
         var queue = new Loggers.Queue();
         Event evt = null;
         // invalid global context
         AssertException(() => Log.RegisterContext("Global", new Dictionary<String, Func<Object>>()
            {
               { "Host", () => "test" }
            }
         ));
         // invalid instance registration
         AssertException(() => Log.RegisterInstance(null));
         AssertException(() => Log.RegisterInstance(new Instance()));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Buffer = -1,
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = null,
               Properties = new[] { "Event.Type" },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = null,
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new String[0],
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { (String)null },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "" },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { " " },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "invalid" },
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  IncludeProps = new[] { new Filter.Property(null, "test") },
               }
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  IncludeProps = new[] { new Filter.Property("", "test") },
               }
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  IncludeProps = new[] { new Filter.Property(" ", "test") },
               }
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  ExcludeProps = new[] { new Filter.Property(null, "test") },
               }
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  ExcludeProps = new[] { new Filter.Property("", "test") },
               }
            }
         ));
         AssertException(() => Log.RegisterInstance(
            new Instance()
            {
               Logger = new Loggers.Null(),
               Properties = new[] { "Event.Type" },
               Filter = new Filter()
               {
                  ExcludeProps = new[] { new Filter.Property(" ", "test") },
               }
            }
         ));
         // default instance registration
         var instance = new Instance()
         {
            Logger = queue,
            Synchronous = true,
            Buffer = 0,
            Properties = new[] { "Event.Type" }
         };
         Log.RegisterInstance(instance);
         log.Trace("trace");
         evt = queue.Dequeue().Single();
         Assert.AreEqual(evt["Event.Type"], EventType.Trace);
         Log.UnregisterInstance(instance);
         log.Trace("trace");
         Assert.IsFalse(queue.Dequeue().Any());
         // disposable instance registration
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[] { "Event.Type" }
               }
            )
         )
         {
            log.Info("info");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Type"], EventType.Info);
         }
         log.Info("info");
         Assert.IsFalse(queue.Dequeue().Any());
         // log source type
         Assert.AreEqual(log.SourceType, GetType());
      }

      [TestMethod]
      public void TestUnlogged ()
      {
         var log = new Log(this);
         log.Event(EventType.Trace, "event");
         log.Event(EventType.Info, "{0}", "test");
         log.Event(EventType.Warning, new Exception("test"));
         log.Event(EventType.Error, new Exception("test"), "test");
         log.Event(EventType.Trace, new Exception("test"), "{0}", "test");
         log.EventEx(EventType.Info, new { Value = "test" });
         log.EventEx(EventType.Warning, new { Value = "test" }, "test");
         log.EventEx(EventType.Error, new { Value = "test" }, "{0}", new[] { "test" });
         log.EventEx(EventType.Info, new { Value = "test" }, new Exception("test"));
         log.EventEx(EventType.Warning, new { Value = "test" }, new Exception("test"), "test");
         log.EventEx(EventType.Error, new { Value = "test" }, new Exception("test"), "{0}", "test");
         log.Trace("trace");
         log.TraceEx("{0}", "test");
         log.TraceEx(new { Value = "test" });
         log.TraceEx(new { Value = "test" }, "test");
         log.TraceEx(new { Value = "test" }, "{0}", new[] { "test" });
         log.Info("info");
         log.InfoEx("{0}", "test");
         log.InfoEx(new { Value = "test" });
         log.InfoEx(new { Value = "test" }, "test");
         log.InfoEx(new { Value = "test" }, "{0}", new[] { "test" });
         log.Warn("warning");
         log.Warn("{0}", "test");
         log.Warn(new Exception("test"));
         log.Warn(new Exception("test"), "test");
         log.Warn(new Exception("test"), "{0}", "test");
         log.WarnEx(new { Value = "test" });
         log.WarnEx(new { Value = "test" }, "test");
         log.WarnEx(new { Value = "test" }, "{0}", new[] { "test" });
         log.WarnEx(new { Value = "test" }, new Exception("test"));
         log.WarnEx(new { Value = "test" }, new Exception("test"), "test");
         log.WarnEx(new { Value = "test" }, new Exception("test"), "{0}", "test");
         log.Error("error");
         log.Error("{0}", "test");
         log.Error(new Exception("test"));
         log.Error(new Exception("test"), "test");
         log.Error(new Exception("test"), "{0}", "test");
         log.ErrorEx(new { Value = "test" });
         log.ErrorEx(new { Value = "test" }, "test");
         log.ErrorEx(new { Value = "test" }, "{0}", new[] { "test" });
         log.ErrorEx(new { Value = "test" }, new Exception("test"));
         log.ErrorEx(new { Value = "test" }, new Exception("test"), "test");
         log.ErrorEx(new { Value = "test" }, new Exception("test"), "{0}", "test");
      }

      [TestMethod]
      public void TestEventProperties ()
      {
         // configure the log
         var log = new Log(this);
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Timestamp",
            "Event.Time",
            "Event.Message",
            "Event.Exception",
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
            // invalid property
            log.Trace("");
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt[null]);
            Assert.IsNull(evt[""]);
            Assert.IsNull(evt[" "]);
            Assert.IsNull(evt["Invalid"]);
            Assert.IsNull(evt["Event.Invalid"]);
            // property list
            log.Error(new Exception("test"));
            evt = queue.Dequeue().Single();
            Assert.IsTrue(evt.Names.SequenceEqual(props));
            Assert.IsTrue(evt.Properties.Select(p => p.Key).SequenceEqual(props));
            Assert.IsTrue(!evt.Values.Any(v => v == null));
            Assert.IsTrue(!evt.Properties.Any(p => p.Value == null));
            // event type
            log.Trace("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Type"], EventType.Trace);
            log.Info("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["event.type"], EventType.Info);
            log.Warn("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.type"], EventType.Warning);
            log.Error("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["event.Type"], EventType.Error);
            // event source
            log.Info("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Source"], GetType());
            new Log(typeof(String)).Info("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Source"], typeof(String));
            // event timestamp
            Int64 oldStamp = 0;
            log.Info("");
            evt = queue.Dequeue().Single();
            oldStamp = (Int64)evt["Event.Timestamp"];
            Assert.IsTrue(oldStamp != 0);
            log.Info("");
            evt = queue.Dequeue().Single();
            Assert.IsTrue((Int64)evt["Event.Timestamp"] > oldStamp);
            // event time
            DateTime oldTime;
            log.Info("");
            evt = queue.Dequeue().Single();
            oldTime = (DateTime)evt["Event.Time"];
            Assert.IsTrue(oldTime > DateTime.UtcNow - TimeSpan.FromMinutes(1));
            Assert.IsTrue(oldTime <= DateTime.UtcNow);
            log.Info("");
            evt = queue.Dequeue().Single();
            Assert.IsTrue((DateTime)evt["Event.Time"] >= oldTime);
            // event message
            log.Info(null);
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Message"]);
            log.Info("");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Message"], "");
            log.Info(" ");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Message"], " ");
            log.Info("test");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Message"], "test");
            log.Info("test{0}test{1}test", 0, 1);
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Message"], "test0test1test");
            // event exception
            log.Trace(null);
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Exception"]);
            log.Info(null);
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Exception"]);
            log.Warn((Exception)null);
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Exception"]);
            log.Error((Exception)null);
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Exception"]);
            log.Warn(new Exception("test"));
            evt = queue.Dequeue().Single();
            Assert.AreEqual(((Exception)evt["Event.Exception"]).Message, "test");
            Assert.AreEqual(evt["Event.Message"], "test");
            log.Error(new Exception("test"));
            evt = queue.Dequeue().Single();
            Assert.AreEqual(((Exception)evt["Event.Exception"]).Message, "test");
            Assert.AreEqual(evt["Event.Message"], "test");
         }
      }

      [TestMethod]
      public void TestCustomProperties ()
      {
         // configure the log
         var log = new Log(this);
         var queue = new Loggers.Queue();
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[]
                  {
                     "Event.Property",
                     "TestContext.Field",
                     "TestContext.Property",
                     "TestContext.Private",
                     "TestContext.Method",
                  }
               }
            )
         )
         {
            Event evt;
            // typed properties
            log.InfoEx(new TestContext() { Field = null, Property = null });
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["TestContext.Field"]);
            Assert.IsNull(evt["TestContext.Property"]);
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new TestContext() { Field = "", Property = "" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Field"], "");
            Assert.AreEqual(evt["TestContext.Property"], "");
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new TestContext() { Field = " ", Property = " " });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Field"], " ");
            Assert.AreEqual(evt["TestContext.Property"], " ");
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new TestContext() { Field = "test field", Property = "test prop" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Field"], "test field");
            Assert.AreEqual(evt["TestContext.Property"], "test prop");
            Assert.IsNull(evt["Event.Property"]);
            // anonymous properties (default context)
            log.InfoEx(new { Property = (String)null });
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["Event.Property"]);
            Assert.IsNull(evt["TestContext.Property"]);
            log.InfoEx(new { Property = "" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Property"], "");
            Assert.IsNull(evt["TestContext.Property"]);
            log.InfoEx(new { Property = " " });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Property"], " ");
            Assert.IsNull(evt["TestContext.Property"]);
            log.InfoEx(new { Property = "test" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Property"], "test");
            Assert.IsNull(evt["TestContext.Property"]);
            // anonymous property (valid context)
            log.InfoEx(new { Context = "TestContext", Property = (String)null });
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["TestContext.Property"]);
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new { Context = "TestContext", Property = "" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Property"], "");
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new { Context = "TestContext", Property = " " });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Property"], " ");
            Assert.IsNull(evt["Event.Property"]);
            log.InfoEx(new { Context = "TestContext", Property = "test" });
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestContext.Property"], "test");
            Assert.IsNull(evt["Event.Property"]);
         }
      }

      [TestMethod]
      public void TestSourceProperties ()
      {
         // configure the log
         var queue = new Loggers.Queue();
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[] { "TestLog.Property" }
               }
            )
         )
         {
            Event evt;
            // default value
            new Log(this).Info("test");
            evt = queue.Dequeue().Single();
            Assert.IsNull(evt["TestLog.Property"]);
            // valid value
            this.Property = "value";
            new Log(this).Info("test");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["TestLog.Property"], "value");
            // typed external value
            this.Property = null;
            new Log(GetType(), new TestContext() { Property = "test" }).Info("test");
            Assert.AreEqual(evt["TestLog.Property"], "value");
            // anonymous external value
            this.Property = null;
            new Log(GetType(), new { Property = "test" }).Info("test");
            Assert.AreEqual(evt["TestLog.Property"], "value");
         }
      }

      [TestMethod]
      public void TestGlobalProperties ()
      {
         // configure the log
         var log = new Log(this);
         var queue = new Loggers.Queue();
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[]
                  {
                     "Global.Value",
                     "Global.Host"
                  }
               }
            )
         )
         {
            Event evt;
            var globalCurrent = globalValue;
            // initial value
            log.Info(null);
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Global.Value"], globalValue);
            Assert.AreEqual(evt["Global.Value"], ++globalCurrent);
            Assert.AreEqual(evt["Global.Host"], Environment.MachineName);
            // subsequent
            log.Info(null);
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Global.Value"], globalValue);
            Assert.AreEqual(evt["Global.Value"], ++globalCurrent);
            Assert.AreEqual(evt["Global.Host"], Environment.MachineName);
         }
      }

      [TestMethod]
      public void TestFiltering ()
      {
         var log = new Log(this);
         var queue = new Loggers.Queue();
         var instance = new Instance()
         {
            Logger = queue,
            Synchronous = true,
            Buffer = 0,
            Properties = new[]
            {
               "Event.Type",
               "TestLog.ClassValue",
               "Event.EventValue",
               "Global.Host"
            }
         };
         // property inclusion
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Event.Type", "Error"),
               new Filter.Property("Event.Type", "Warning") 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            log.Warn("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            log.Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
         }
         // property exclusion
         instance.Filter = new Filter()
         {
            ExcludeProps = new[]
            { 
               new Filter.Property("Event.Type", "Error"),
               new Filter.Property("Event.Type", "Warning") 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Warn("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
         }
         instance.Filter = new Filter()
         {
            ExcludeProps = new[]
            { 
               new Filter.Property("Event.Type", "Error"),
               new Filter.Property("Event.Type", "Warning") 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Warn("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
         }
         // property inclusion/exclusion
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Event.Type", "Error"),
               new Filter.Property("Event.Type", "Warning") 
            },
            ExcludeProps = new[]
            {
               new Filter.Property("Event.Type", "Warning"),
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            log.Warn("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
         }
         // source inclusion
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Event.Source", GetType().AssemblyQualifiedName),
               new Filter.Property("Event.Source", typeof(Int32).AssemblyQualifiedName) 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            new Log(typeof(Int32)).Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            new Log(typeof(String)).Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
         }
         // source exclusion
         instance.Filter = new Filter()
         {
            ExcludeProps = new[]
            { 
               new Filter.Property("Event.Source", GetType().AssemblyQualifiedName),
               new Filter.Property("Event.Source", typeof(Int32).AssemblyQualifiedName) 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
            new Log(typeof(Int32)).Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
            new Log(typeof(String)).Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
         }
         // source inclusion/exclusion
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Event.Source", GetType().AssemblyQualifiedName),
               new Filter.Property("Event.Source", typeof(Int32).AssemblyQualifiedName) 
            },
            ExcludeProps = new[]
            { 
               new Filter.Property("Event.Source", typeof(Int32).AssemblyQualifiedName) 
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Info("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            new Log(typeof(Int32)).Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
            new Log(typeof(String)).Info("test");
            Assert.IsFalse(queue.Dequeue().Any());
         }
         // non-event property filter
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Event.Message", "test"),
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
            log.Error("invalid");
            Assert.IsFalse(queue.Dequeue().Any());
         }
         instance.Filter = new Filter()
         {
            ExcludeProps = new[]
            { 
               new Filter.Property("Event.Message", "test"),
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.IsFalse(queue.Dequeue().Any());
            log.Error("invalid");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
         }
         // non-event global property filter
         instance.Filter = new Filter()
         {
            IncludeProps = new[]
            { 
               new Filter.Property("Global.Host", Environment.MachineName),
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.AreEqual(queue.Dequeue().Count(), 1);
         }
         instance.Filter = new Filter()
         {
            ExcludeProps = new[]
            { 
               new Filter.Property("Global.Host", Environment.MachineName),
            }
         };
         using (Log.RegisterInstance(instance))
         {
            log.Error("test");
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      [TestMethod]
      public void TestBuffering ()
      {
         var log = new Log(this);
         var queue = new Loggers.Queue();
         var buffer = 10;
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue,
                  Synchronous = true,
                  Buffer = buffer,
                  Properties = new[]
                  { 
                     "Event.Timestamp",
                     "Event.Time",
                     "Event.Index"
                  }
               }
            )
         )
         {
            // manual flush
            {
               for (Int32 i = 0; i < buffer - 1; i++)
               {
                  log.InfoEx(new { Index = i });
                  Assert.IsFalse(queue.Dequeue().Any());
               }
               Log.Flush();
               var events = queue.Dequeue().ToList();
               Assert.AreEqual(events.Count, buffer - 1);
               for (Int32 i = 0; i < buffer - 1; i++)
               {
                  if (i > 0)
                  {
                     Assert.IsTrue((Int64)events[i]["Event.Timestamp"] >= (Int64)events[i - 1]["Event.Timestamp"]);
                     Assert.IsTrue((DateTime)events[i]["Event.Time"] >= (DateTime)events[i - 1]["Event.Time"]);
                  }
                  Assert.AreEqual(events[i]["Event.Index"], i);
               }
            }
            // auto flush
            {
               for (Int32 i = 0; i < buffer - 1; i++)
               {
                  log.InfoEx(new { Index = i });
                  Assert.IsFalse(queue.Dequeue().Any());
               }
               log.InfoEx(new { Index = buffer - 1 });
               var events = queue.Dequeue().ToList();
               Assert.AreEqual(events.Count, buffer);
               for (Int32 i = 0; i < buffer; i++)
               {
                  if (i > 0)
                  {
                     Assert.IsTrue((Int64)events[i]["Event.Timestamp"] >= (Int64)events[i - 1]["Event.Timestamp"]);
                     Assert.IsTrue((DateTime)events[i]["Event.Time"] >= (DateTime)events[i - 1]["Event.Time"]);
                  }
                  Assert.AreEqual(events[i]["Event.Index"], i);
               }
            }
         }
      }

      [TestMethod]
      public void TestConcurrency ()
      {
         // concurrent events with no log
         Parallel.For(0, ParallelIterations,
            i =>
            {
               var log = new Log(this);
               log.InfoEx(
                  new TestContext()
                  {
                     Field = "TestContextField",
                     Property = "TestContextProperty"
                  },
                  "TestMessage",
                  null
               );
            }
         );
         // configure the log
         Loggers.Queue queue;
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue = new Loggers.Queue(),
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[]
                  {
                     "Event.Type",
                     "Event.Source",
                     "Event.Timestamp",
                     "Event.Time",
                     "Event.Message",
                     "Event.Exception",
                     "TestLog.Property",
                     "TestContext.Field",
                     "Global.Host"
                  }
               }
            )
         )
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = new Loggers.Null(),
                  Synchronous = true,
                  Buffer = 0,
                  Properties = new[] { "Event.Type" }
               }
            )
         )
         {
            this.Property = "LogTestProperty";
            // concurrent events through private logs
            {
               var logs = new Log[ParallelIterations];
               for (Int32 i = 0; i < logs.Length; i++)
                  logs[i] = new Log(this);
               Parallel.For(0, ParallelIterations,
                  i =>
                  {
                     logs[i].ErrorEx(
                        new TestContext()
                        {
                           Field = "TestContextField",
                           Property = "TestContextProperty"
                        },
                        new Exception("TestMessage")
                     );
                  }
               );
               var events = queue.Dequeue().ToList();
               Assert.AreEqual(events.Count, ParallelIterations);
               foreach (var evt in events)
               {
                  Assert.AreEqual(evt["Event.Type"], EventType.Error);
                  Assert.AreEqual(evt["Event.Source"], GetType());
                  Assert.AreNotEqual(evt["Event.Timestamp"], 0);
                  Assert.IsNotNull(evt["Event.Time"]);
                  Assert.AreEqual(evt["Event.Message"], "TestMessage");
                  Assert.AreEqual(((Exception)evt["Event.Exception"]).Message, "TestMessage");
                  Assert.AreEqual(evt["TestLog.Property"], "LogTestProperty");
                  Assert.AreEqual(evt["TestContext.Field"], "TestContextField");
                  Assert.AreEqual(evt["Global.Host"], Environment.MachineName);
               }
            }
            // concurrent events through a shared log
            {
               var log = new Log(this);
               Parallel.For(0, ParallelIterations,
                  i =>
                  {
                     log.ErrorEx(
                        new TestContext()
                        {
                           Field = "TestContextField",
                           Property = "TestContextProperty"
                        },
                        new Exception("TestMessage")
                     );
                  }
               );
               var events = queue.Dequeue().ToList();
               Assert.AreEqual(events.Count, ParallelIterations);
               foreach (var evt in events)
               {
                  Assert.AreEqual(evt["Event.Type"], EventType.Error);
                  Assert.AreEqual(evt["Event.Source"], GetType());
                  Assert.AreNotEqual(evt["Event.Timestamp"], 0);
                  Assert.IsNotNull(evt["Event.Time"]);
                  Assert.AreEqual(evt["Event.Message"], "TestMessage");
                  Assert.AreEqual(((Exception)evt["Event.Exception"]).Message, "TestMessage");
                  Assert.AreEqual(evt["TestLog.Property"], "LogTestProperty");
                  Assert.AreEqual(evt["TestContext.Field"], "TestContextField");
                  Assert.AreEqual(evt["Global.Host"], Environment.MachineName);
               }
            }
         }
         // concurrent events with configuration changes
         {
            var log = new Log(this);
            var queues = new Loggers.Queue[ParallelIterations];
            var instances = new Instance[ParallelIterations];
            for (Int32 i = 0; i < ParallelIterations; i++)
               instances[i] = new Instance()
               {
                  Logger = queues[i] = new Loggers.Queue(),
                  Buffer = 0,
                  Synchronous = true,
                  Properties = new[] { "Event.Type" }
               };
            Parallel.For(0, ParallelIterations,
               i =>
               {
                  using (
                     Log.RegisterInstance(
                        new Instance()
                        {
                           Logger = new Loggers.Null(),
                           Buffer = i % 10,
                           Synchronous = true,
                           Properties = new[] { "Event.Type" }
                        }
                     )
                  )
                     log.Info("info");
                  using (Log.RegisterInstance(instances[i]))
                     log.Info("info");
                  Assert.IsTrue(queues[i].Dequeue().Count() >= 1);
               }
            );
         }
         // async event causal ordering
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Logger = queue = new Loggers.Queue(),
                  Buffer = 0,
                  Synchronous = false,
                  Properties = new[] 
                  { 
                     "Event.Timestamp",
                     "Event.Time",
                     "Event.Index"
                  }
               }
            )
         )
         {
            var log = new Log(this);
            for (Int32 i = 0; i < ParallelIterations; i++)
               log.InfoEx(new { Index = i });
            Thread.Sleep(100);
            var events = queue.Dequeue().ToList();
            Assert.AreEqual(events.Count, ParallelIterations);
            for (Int32 i = 0; i < ParallelIterations; i++)
            {
               if (i > 0)
               {
                  Assert.IsTrue((Int64)events[i]["Event.Timestamp"] >= (Int64)events[i - 1]["Event.Timestamp"]);
                  Assert.IsTrue((DateTime)events[i]["Event.Time"] >= (DateTime)events[i - 1]["Event.Time"]);
               }
               Assert.AreEqual(events[i]["Event.Index"], i);
            }
         }
         Log.Flush();
      }

      [TestMethod]
      public void TestPerformance ()
      {
         var log = new Log(this);
         Stopwatch clock = new Stopwatch();
         // zero loggers, event context, unfiltered, unbuffered, synchronous
         clock.Restart();
         for (Int32 i = 0; i < PerformanceIterations; i++)
            log.Info("test");
         clock.Stop();
         TraceClock("ZEUUS", clock);
         // single logger, event context, unfiltered, unbuffered, synchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("SEUUS", clock);
         }
         // two loggers, event context, unfiltered, unbuffered, synchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("TEUUS", clock);
         }
         // single logger, global context, unfiltered, unbuffered, synchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Global.Value" },
                  Logger = new Loggers.Null(),
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("SGUUS", clock);
         }
         // single logger, event context, property filtered, unbuffered, synchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Filter = new Filter() 
                  { 
                     IncludeProps = new[] 
                     { 
                        new Filter.Property("Event.Type", EventType.Info.ToString()) 
                     } 
                  },
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("SEPUS", clock);
         }
         // single logger, event context, unfiltered, buffered, synchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Buffer = Math.Max(PerformanceIterations / 10, 10),
                  Synchronous = true
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("SEUBS", clock);
         }
         // single logger, event context, unfiltered, unbuffered, asynchronous
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[] { "Event.Type" },
                  Logger = new Loggers.Null(),
                  Buffer = Math.Max(PerformanceIterations / 10, 10),
                  Synchronous = false
               }
            )
         )
         {
            clock.Restart();
            for (Int32 i = 0; i < PerformanceIterations; i++)
               log.Info("test");
            clock.Stop();
            TraceClock("SEUUA", clock);
         }
         Log.Flush();
      }

      private void TraceClock (String trace, Stopwatch clock)
      {
         Debug.WriteLine(
            String.Format(
               "{0}: {1,5} ms, {2:0.000} ms/i, {3} i",
               trace,
               clock.ElapsedMilliseconds,
               (float)clock.ElapsedMilliseconds / (float)PerformanceIterations,
               PerformanceIterations
            )
         );
      }

      private void AssertException (Action action)
      {
         try
         {
            action();
            Assert.Fail();
         }
         catch (AssertFailedException)
         {
            throw;
         }
         catch { }
      }

      private sealed class TestContext
      {
         public String Context { get { return "Test"; } }
         public String Field;
         public String Property { get; set; }
         private String Private { get { return "Private"; } }
         public String Method () { return "Test"; }
      }
   }
}
