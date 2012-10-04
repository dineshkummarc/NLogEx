//===========================================================================
// MODULE:  TestStackFrame.cs
// PURPOSE: stack frame logging context unit test driver
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
   #if !DEBUG     // stack frame is only reliable in debug mode
      [Ignore]
   #endif
   [TestClass]
   public class TestStackFrame
   {
      static TestStackFrame ()
      {
         Context.StackFrame.Register();
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
                     "StackFrame.File",
                     "StackFrame.Line",
                     "StackFrame.Type",
                     "StackFrame.Method"
                  },
                  Logger = queue,
                  Buffer = 0,
                  Synchronous = true
               }
            )
         )
         {
            var frame = new System.Diagnostics.StackFrame(0, true);
            log.Info("test");
            var evt = queue.Dequeue().Single();
            Assert.AreEqual(evt["StackFrame.File"], frame.GetFileName());
            Assert.AreEqual(evt["StackFrame.Line"], frame.GetFileLineNumber() + 1);
            Assert.AreEqual(evt["StackFrame.Type"], frame.GetMethod().DeclaringType.FullName);
            Assert.AreEqual(evt["StackFrame.Method"], frame.GetMethod().Name);
         }
      }
   }
}
