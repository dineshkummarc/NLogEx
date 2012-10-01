//===========================================================================
// MODULE:  TestTraceListener.cs
// PURPOSE: log-based System.Diagnostics trace listener unit test driver
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
// Compiler Symbols
#define TRACE
// System References
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestTraceListener
   {
      [TestMethod]
      public void TestTracing ()
      {
         // configure the trace listener
         var listener = new NLogEx.TraceListener();
         Trace.Listeners.Add(listener);
         // configure the logger
         var queue = new Loggers.Queue();
         using (
            Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[]
                  {
                     "Event.Type",
                     "Event.Message"
                  },
                  Synchronous = true,
                  Buffer = 0,
                  Logger = queue
               }
            )
         )
         {
            Event evt;
            // informational event
            Trace.TraceInformation("trace info");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Type"], EventType.Info);
            Assert.IsTrue(((String)evt["Event.Message"]).Contains("trace info"));
            // warning event
            Trace.TraceWarning("trace warning");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Type"], EventType.Warning);
            Assert.IsTrue(((String)evt["Event.Message"]).Contains("trace warning"));
            // error event
            Trace.TraceError("trace error");
            evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["Event.Type"], EventType.Error);
            Assert.IsTrue(((String)evt["Event.Message"]).Contains("trace error"));
         }
         Trace.Listeners.Remove(listener);
      }
   }
}
