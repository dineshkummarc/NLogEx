//===========================================================================
// MODULE:  TestLogModule.cs
// PURPOSE: ASP.NET logging module unit test driver
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
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx.Mvc.Test
{
   [TestClass]
   public class TestLogModule
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;

      static TestLogModule ()
      {
         new LogModule().Init(MockHttp.CreateGlobalApplication());
      }

      [TestMethod]
      public void TestProperties ()
      {
         // start up the next log instance
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Request.Duration"
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
            for (Int32 i = 0; i < TestIterations; i++)
               new MockHttp.Application().DispatchRequest();
            // verify event properties
            for (Int32 i = 0; i < TestIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.IsTrue((Int64)evt["Request.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Request.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      [TestMethod]
      public void TestConcurrency ()
      {
         // start up the next log instance
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Request.Duration"
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
            Log log = new Log(GetType());
            Parallel.For(0, TestParallelIterations,
               i => new MockHttp.Application().DispatchRequest()
            );
            Log.Flush();
            // verify event properties
            for (Int32 i = 0; i < TestParallelIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.IsTrue((Int64)evt["Request.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Request.Duration"] < 10000000);
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }
   }
}
