//===========================================================================
// MODULE:  MockHttp.cs
// PURPOSE: Mock HTTP request, context, application
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
using System.Web;
using System.Web.SessionState;
// Project References

namespace NLogEx.Mvc.Test
{
   public static class MockHttp
   {
      static Application globalApp = new Application();

      public static HttpContext CreateHttpContext ()
      {
         HttpContext context = new HttpContext(new Request())
         {
            User = new GenericPrincipal(
               new GenericIdentity("testuser"),
               new String[0]
            )
         };
         SessionStateUtility.AddHttpSessionStateToContext(
            context,
            new HttpSessionStateContainer(
               "id",
               new SessionStateItemCollection(),
               new HttpStaticObjectsCollection(),
               Int32.MaxValue,
               true,
               HttpCookieMode.AutoDetect,
               SessionStateMode.InProc,
               false
            )
         );
         context.Session["TestVariable"] = "TestVariableValue";
         return context;
      }

      public static Application CreateGlobalApplication ()
      {
         return globalApp;
      }

      public sealed class Application : HttpApplication
      {
         public void DispatchRequest ()
         {
            HttpContext.Current = MockHttp.CreateHttpContext();
            HttpContext.Current.ApplicationInstance = this;
            GetType().BaseType.GetField(
               "_context", 
               BindingFlags.NonPublic | BindingFlags.Instance
            ).SetValue(this, HttpContext.Current);
            var beginRequest = GetType().BaseType.GetField(
               "EventBeginRequest", 
               BindingFlags.NonPublic | BindingFlags.Static
            ).GetValue(null);
            var endRequest = GetType().BaseType.GetField(
               "EventEndRequest",
               BindingFlags.NonPublic | BindingFlags.Static
            ).GetValue(null);
            globalApp.Events[beginRequest].DynamicInvoke(this, new EventArgs());
            System.Threading.Thread.Sleep(1);   // nonzero duration
            globalApp.Events[endRequest].DynamicInvoke(this, new EventArgs());
         }
      }

      private sealed class Request : HttpWorkerRequest
      {
         public override String GetHttpVerbName ()
         {
            return "HEAD";
         }
         public override String GetHttpVersion ()
         {
            return "1.1";
         }
         public override String GetRawUrl ()
         {
            return "/?Test=Value";
         }
         public override String GetLocalAddress ()
         {
            return "www.tempuri.org";
         }
         public override Int32 GetLocalPort ()
         {
            return 80;
         }
         public override String GetUriPath ()
         {
            return "/";
         }
         public override String GetQueryString ()
         {
            return "Test=Value";
         }
         public override String GetRemoteAddress ()
         {
            return Dns.GetHostAddresses("localhost")[0].ToString();
         }
         public override Int32 GetRemotePort ()
         {
            return 12345;
         }
         public override String[][] GetUnknownRequestHeaders ()
         {
            return new[] { new[] { "TestHeader", "TestHeaderValue" } };
         }
         public override void SendKnownResponseHeader (int index, String value)
         {
         }
         public override void SendResponseFromFile (IntPtr handle, long offset, long length)
         {
         }
         public override void SendResponseFromFile (String filename, long offset, long length)
         {
         }
         public override void SendResponseFromMemory (byte[] data, int length)
         {
         }
         public override void SendStatus (int statusCode, String statusDescription)
         {
         }
         public override void SendUnknownResponseHeader (String name, String value)
         {
         }
         public override void FlushResponse (Boolean finalFlush)
         {
         }
         public override void EndOfRequest ()
         {
         }
      }
   }
}
