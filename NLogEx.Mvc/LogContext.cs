//===========================================================================
// MODULE:  LogContext.cs
// PURPOSE: ASP.NET MVC log context
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
using System.Web;
using System.Web.SessionState;
// Project References

namespace NLogEx.Mvc
{
   /// <summary>
   /// The HTTP logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving HTTP context properties, 
   /// headers, and session variables during event logging.
   /// </remarks>
   public static class LogContext
   {
      #region HTTP Context
      /// <summary>
      /// The thread-local HTTP context
      /// </summary>
      private static HttpContext Http
      { 
         get { return HttpContext.Current; }
      }
      /// <summary>
      /// The current HTTP request
      /// </summary>
      private static HttpRequest Request
      { 
         get { return (Http != null) ? Http.Request : null; }
      }
      /// <summary>
      /// The current HTTP response
      /// </summary>
      private static HttpResponse Response
      { 
         get { return (Http != null) ? Http.Response : null; }
      }
      /// <summary>
      /// The current HTTP session
      /// </summary>
      private static HttpSessionState Session
      { 
         get { return (Http != null) ? Http.Session : null; }
      }
      #endregion

      #region Operations
      /// <summary>
      /// Establishes the HTTP context with the logging subsystem
      /// </summary>
      /// <param name="cfg">
      /// The requested request/session state variables to
      /// include in the context
      /// </param>
      public static void Register (Config cfg = null)
      {
         // create generic HTTP context properties
         Dictionary<String, Func<Object>> ctx = new Dictionary<String, Func<Object>>()
         {
            { "Uri", () => (Request != null) ? Request.Url : null },
            { "Path", () => (Request != null && Request.Url != null) ? Request.Url.PathAndQuery : null },
            { "Method", () => (Request != null) ? Request.HttpMethod : null },
            { "StatusCode", () => (Response != null) ? Response.StatusCode : 0 },
            { "Status", () => (Response != null) ? Response.Status : null },
            { "Timestamp", () => (Http != null) ? (DateTime?)Http.Timestamp : null },
            { "User", () => (Http != null) ? Http.User.Identity.Name : null },
            { "SessionID", () => (Session != null) ? Session.SessionID : null },
            { "IPAddress", () => (Request != null) ? 
                                    Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? 
                                    Request.UserHostAddress : 
                                    null }
         };
         // add requested request headers
         if (cfg != null && cfg.RequestHeaders != null)
            foreach (String header in cfg.RequestHeaders)
               ctx["Request." + header] = () => (Request != null) ? Request.Headers[header] : null;
         // add requested session variables
         if (cfg != null && cfg.SessionVariables != null)
            foreach (String variable in cfg.SessionVariables)
               ctx["Session." + variable] = () => (Session != null) ? Session[variable] : null;
         // establish the context
         Log.RegisterContext("Http", ctx);
      }
      #endregion

      /// <summary>
      /// HTTP log context configuration
      /// </summary>
      public sealed class Config
      {
         /// <summary>
         /// The list of request headers to include
         /// in the log context
         /// </summary>
         public IEnumerable<String> RequestHeaders { get; set; }
         /// <summary>
         /// The list of ASP.NET session variables
         /// to include in the log context
         /// </summary>
         public IEnumerable<String> SessionVariables { get; set; }
      }
   }
}
