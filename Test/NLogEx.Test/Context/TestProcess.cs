//===========================================================================
// MODULE:  TestProcess.cs
// PURPOSE: application process logging context unit test driver
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestProcess
   {
      static TestProcess ()
      {
         Context.Process.Register();
      }

      [TestMethod]
      public void TestContext ()
      {
         var queue = new Loggers.Queue();
         var log = new Log(this);
         using (Log.RegisterInstance(
               new Instance()
               {
                  Properties = new[]
                  {
                     "Process.ID",
                     "Process.Name",
                     "Process.StartTime",
                     "Process.PeakVirtualMemory",
                     "Process.PeakPagedMemory",
                     "Process.PeakWorkingSetMemory",
                     "Process.ProcessorTime",
                     "Process.PrivilegedProcessorTime",
                     "Process.UserProcessorTime"
                  },
                  Logger = queue,
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            log.Info("test");
            var evt = queue.Dequeue().Single();
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
               Assert.AreEqual(evt["Process.ID"], process.Id);
               Assert.AreEqual(evt["Process.Name"], process.ProcessName);
               Assert.AreEqual(evt["Process.StartTime"], process.StartTime);
               Assert.IsTrue((Int64)evt["Process.PeakVirtualMemory"] > 0);
               Assert.IsTrue((Int64)evt["Process.PeakPagedMemory"] > 0);
               Assert.IsTrue((Int64)evt["Process.PeakWorkingSetMemory"] > 0);
               Assert.IsTrue((TimeSpan)evt["Process.ProcessorTime"] > TimeSpan.FromTicks(0));
               Assert.IsTrue((TimeSpan)evt["Process.PrivilegedProcessorTime"] > TimeSpan.FromTicks(0));
               Assert.IsTrue((TimeSpan)evt["Process.UserProcessorTime"] > TimeSpan.FromTicks(0));
            }
         }
      }
   }
}
