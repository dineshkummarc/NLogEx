//===========================================================================
// MODULE:  TestLogAttribute.cs
// PURPOSE: MVC log action filter attribute unit test driver
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx.Mvc.Test
{
   [TestClass]
   public class TestLogAttribute
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;

      [TestMethod]
      public void TestActionLog ()
      {
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Message",
            "Event.Exception",
            "Action.Controller",
            "Action.Name",
            "Action.Duration",
            "Action.RouteParams"
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
            var controllerContext = CreateControllerContext();
            var actionDescriptor = CreateActionDescriptor();
            // enabled log
            for (Int32 i = 0; i < TestIterations; i++)
            {
               new LogAttribute().OnActionExecuting(
                  new ActionExecutingContext(
                     controllerContext,
                     actionDescriptor,
                     new Dictionary<String, Object>()
                  )
               );
               System.Threading.Thread.Sleep(1);   // ensure duration > 0
               new LogAttribute().OnActionExecuted(
                  new ActionExecutedContext(
                     controllerContext,
                     actionDescriptor,
                     false,
                     null
                  )
               );
            }
            for (Int32 i = 0; i < TestIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(TestController));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.AreEqual(evt["Action.Controller"], "Test");
               Assert.AreEqual(evt["Action.Name"], "Test");
               Assert.IsTrue((Int64)evt["Action.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Action.Duration"] < 10000000);
               Assert.AreEqual(evt["Action.RouteParams"], "TestRouteParam=TestRouteValue");
            }
            Assert.IsFalse(queue.Dequeue().Any());
            // disabled log
            for (Int32 i = 0; i < TestIterations; i++)
            {
               new LogAttribute() { Disabled = true }.OnActionExecuting(
                  new ActionExecutingContext(
                     controllerContext,
                     actionDescriptor,
                     new Dictionary<String, Object>()
                  )
               );
               new LogAttribute() { Disabled = true }.OnActionExecuted(
                  new ActionExecutedContext(
                     controllerContext,
                     actionDescriptor,
                     false,
                     null
                  )
               );
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      [TestMethod]
      public void TestException ()
      {
         var queue = new Loggers.Queue();
         var props = new List<String>()
         {
            "Event.Type",
            "Event.Source",
            "Event.Message",
            "Event.Exception",
            "Action.Controller",
            "Action.Name",
            "Action.Duration",
            "Action.RouteParams"
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
            var controllerContext = CreateControllerContext();
            var actionDescriptor = CreateActionDescriptor();
            for (Int32 i = 0; i < TestIterations; i++)
            {
               new LogAttribute().OnActionExecuting(
                  new ActionExecutingContext(
                     controllerContext,
                     actionDescriptor,
                     new Dictionary<String, Object>()
                  )
               );
               System.Threading.Thread.Sleep(1);   // ensure duration > 0
               new LogAttribute().OnActionExecuted(
                  new ActionExecutedContext(
                     controllerContext,
                     actionDescriptor,
                     false,
                     new Exception("testmessage")
                  )
               );
            }
            for (Int32 i = 0; i < TestIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(TestController));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.AreEqual(evt["Action.Controller"], "Test");
               Assert.AreEqual(evt["Action.Name"], "Test");
               Assert.IsTrue((Int64)evt["Action.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Action.Duration"] < 10000000);
               Assert.AreEqual(evt["Action.RouteParams"], "TestRouteParam=TestRouteValue");
               evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Error);
               Assert.AreEqual(evt["Event.Source"], typeof(TestController));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsInstanceOfType(evt["Event.Exception"], typeof(Exception));
               Assert.AreEqual(((Exception)evt["Event.Exception"]).Message, "testmessage");
               Assert.AreEqual(evt["Action.Controller"], "Test");
               Assert.AreEqual(evt["Action.Name"], "Test");
               Assert.IsTrue((Int64)evt["Action.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Action.Duration"] < 10000000);
               Assert.AreEqual(evt["Action.RouteParams"], "TestRouteParam=TestRouteValue");
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
            "Action.Controller",
            "Action.Name",
            "Action.Duration",
            "Action.RouteParams"
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
            var actionDescriptor = CreateActionDescriptor();
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  var controllerContext = CreateControllerContext();
                  new LogAttribute().OnActionExecuting(
                     new ActionExecutingContext(
                        controllerContext,
                        actionDescriptor,
                        new Dictionary<String, Object>()
                     )
                  );
                  System.Threading.Thread.Sleep(1);   // ensure duration > 0
                  new LogAttribute().OnActionExecuted(
                     new ActionExecutedContext(
                        controllerContext,
                        actionDescriptor,
                        false,
                        null
                     )
                  );
               }
            );
            Log.Flush();
            for (Int32 i = 0; i < TestParallelIterations; i++)
            {
               var evt = queue.Dequeue().First();
               Assert.AreEqual(evt["Event.Type"], EventType.Trace);
               Assert.AreEqual(evt["Event.Source"], typeof(TestController));
               Assert.IsNotNull(evt["Event.Message"]);
               Assert.IsNull(evt["Event.Exception"]);
               Assert.AreEqual(evt["Action.Controller"], "Test");
               Assert.AreEqual(evt["Action.Name"], "Test");
               Assert.IsTrue((Int64)evt["Action.Duration"] > 0);
               Assert.IsTrue((Int64)evt["Action.Duration"] < 10000000);
               Assert.AreEqual(evt["Action.RouteParams"], "TestRouteParam=TestRouteValue");
            }
            Assert.IsFalse(queue.Dequeue().Any());
         }
      }

      private ControllerContext CreateControllerContext ()
      {
         var routeParams = new RouteData();
         routeParams.Values.Add("TestRouteParam", "TestRouteValue");
         return new ControllerContext(
            new RequestContext(
               new HttpContextWrapper(
                  new HttpContext(
                     new HttpRequest("", "http://www.tempuri.org/", ""),
                     new HttpResponse(new System.IO.StringWriter())
                  )
               ),
               routeParams
            ),
            new TestController()
         );
      }

      private ActionDescriptor CreateActionDescriptor ()
      {
         return new ReflectedActionDescriptor(
            typeof(TestController).GetMethod("Test"),
            "Test",
            new ReflectedControllerDescriptor(typeof(TestController))
         );
      }

      private sealed class TestController : Controller
      {
         public void Test ()
         {
         }
      }
   }
}
