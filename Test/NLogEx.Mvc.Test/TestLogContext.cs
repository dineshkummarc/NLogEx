﻿//===========================================================================
// MODULE:  TestLogContext.cs
// PURPOSE: MVC log context unit test driver
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
   public class TestLogContext
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;

      static TestLogContext ()
      {
         LogContext.Register(
            new LogContext.Config()
            {
               RequestHeaders = new[] { "TestHeader" },
               SessionVariables = new[] { "TestVariable" }
            }
         );
      }

      [TestMethod]
      public void TestProperties ()
      {
         // mock the current HTTP context instance
         HttpContext.Current = MockHttp.CreateHttpContext();
         // start up the next log instance
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Http.Uri",
            "Http.Path",
            "Http.Method",
            "Http.StatusCode",
            "Http.Status",
            "Http.Timestamp",
            "Http.User",
            "Http.SessionID",
            "Http.IPAddress",
            "Http.Request.TestHeader",
            "Http.Session.TestVariable"
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
            for (Int32 i = 0; i < TestIterations; i++ )
               new Log(GetType()).Info("test");
            // verify event properties
            for (Int32 i = 0; i < TestIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Http.Uri"], "http://www.tempuri.org/?Test=Value");
               Assert.AreEqual(evt["Http.Path"], "/?Test=Value");
               Assert.AreEqual(evt["Http.Method"], "HEAD");
               Assert.AreEqual(evt["Http.StatusCode"], 200);
               Assert.AreEqual(evt["Http.Status"], "200 OK");
               Assert.IsTrue((DateTime)evt["Http.Timestamp"] <= DateTime.Now);
               Assert.AreEqual(evt["Http.User"], "testuser");
               Assert.IsNotNull(evt["Http.SessionID"]);
               Assert.AreEqual(evt["Http.SessionID"], HttpContext.Current.Session.SessionID);
               Assert.AreEqual(evt["Http.IPAddress"], Dns.GetHostAddresses("localhost")[0].ToString());
               Assert.AreEqual(evt["Http.Request.TestHeader"], "TestHeaderValue");
               Assert.AreEqual(evt["Http.Session.TestVariable"], "TestVariableValue");
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
            "Http.Uri",
            "Http.Path",
            "Http.Method",
            "Http.StatusCode",
            "Http.Status",
            "Http.Timestamp",
            "Http.User",
            "Http.SessionID",
            "Http.IPAddress",
            "Http.Request.TestHeader",
            "Http.Session.TestVariable"
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
               i => 
               {
                  HttpContext.Current = MockHttp.CreateHttpContext();
                  log.Info("test");
               }
            );
            Log.Flush();
            // verify event properties
            for (Int32 i = 0; i < TestParallelIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Http.Uri"], "http://www.tempuri.org/?Test=Value");
               Assert.AreEqual(evt["Http.Path"], "/?Test=Value");
               Assert.AreEqual(evt["Http.Method"], "HEAD");
               Assert.AreEqual(evt["Http.StatusCode"], 200);
               Assert.AreEqual(evt["Http.Status"], "200 OK");
               Assert.IsTrue((DateTime)evt["Http.Timestamp"] <= DateTime.Now);
               Assert.AreEqual(evt["Http.User"], "testuser");
               Assert.IsNotNull(evt["Http.SessionID"]);
               Assert.AreEqual(evt["Http.IPAddress"], Dns.GetHostAddresses("localhost")[0].ToString());
               Assert.AreEqual(evt["Http.Request.TestHeader"], "TestHeaderValue");
               Assert.AreEqual(evt["Http.Session.TestVariable"], "TestVariableValue");
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }
   }
}
