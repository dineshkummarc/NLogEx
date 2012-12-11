//===========================================================================
// MODULE:  LogModule.cs
// PURPOSE: ASP.NET logging HTTP module
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
// Project Reference

namespace NLogEx.Mvc
{
   /// <summary>
   /// Log HTTP module
   /// </summary>
   /// <remarks>
   /// This class implements IHttpModule for logging all ASP.NET requests
   /// that come through the web server. All requests are traced, and those
   /// that fail are also logged as errors. To register, add the following
   /// to web.config:
   ///   <configuration>
   ///      <system.webServer>
   ///         <modules>
   ///            <add name="NLogEx" type="NLogEx.Mvc.LogModule,NLogEx.Mvc"/>
   ///         </modules>
   ///      </<system.webServer>
   ///   </configuration>
   /// </remarks>
   public sealed class LogModule : IHttpModule
   {
      private static readonly String contextName = typeof(LogModule).FullName;
      private static readonly Log log = new Log(typeof(LogModule));

      #region IHttpModule Implementation
      /// <summary>
      /// Initializes the HTTP module, attaching event handlers
      /// </summary>
      /// <param name="context">
      /// The ASP.NET application context
      /// </param>
      public void Init (HttpApplication context)
      {
         context.BeginRequest += HandleBeginRequest;
         context.EndRequest += HandleEndRequest;
      }
      /// <summary>
      /// Module cleanup
      /// </summary>
      public void Dispose ()
      {
      }
      #endregion

      #region Application Event Handlers
      /// <summary>
      /// ASP.NET BeginRequest event handler
      /// </summary>
      /// <param name="source">
      /// The application context
      /// </param>
      /// <param name="args">
      /// Event parameters
      /// </param>
      public void HandleBeginRequest (Object source, EventArgs args)
      {
         HttpContext context = ((HttpApplication)source).Context;
         context.Items[contextName] = new Request(context);
      }
      /// <summary>
      /// ASP.NET EndRequest event handler
      /// </summary>
      /// <param name="source">
      /// The application context
      /// </param>
      /// <param name="args">
      /// Event parameters
      /// </param>
      public void HandleEndRequest (Object source, EventArgs args)
      {
         // stop the request timer
         HttpContext context = ((HttpApplication)source).Context;
         Request request = (Request)context.Items[contextName];
         if (request != null)
         {
            request.Stop();
            context.Items.Remove(contextName);
            // trace the request, and log an error if it failed
            log.TraceEx(request, context.Request.Path);
            if (context.Response.StatusCode >= 400 || context.Error != null)
               log.ErrorEx(request, context.Error);
         }
      }
      #endregion

      /// <summary>
      /// HTTP request log context
      /// </summary>
      public class Request
      {
         private Int64 started;
         private Int64 stopped;

         /// <summary>
         /// Initializes a new request instance
         /// </summary>
         /// <param name="context">
         /// The request HTTP context
         /// </param>
         public Request (HttpContext context)
         {
            this.started = Log.GetTimestamp();
         }
         /// <summary>
         /// Request execution duration, in microseconds
         /// </summary>
         public Int64 Duration
         {
            get { return this.stopped - this.started; }
         }
         /// <summary>
         /// Stops the timer for the request
         /// </summary>
         public void Stop ()
         {
            this.stopped = Log.GetTimestamp();
         }
      }
   }
}
