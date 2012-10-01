//===========================================================================
// MODULE:  LogAttribute.cs
// PURPOSE: ASP.NET MVC logging action filter/attribute
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
using System.Web.Mvc;
using System.Web.Routing;
// Project References

namespace NLogEx.Mvc
{
   /// <summary>
   /// The action log attribute
   /// </summary>
   /// <remarks>
   /// This attribute causes the associated controller class or action 
   /// method to be dispatched to the logging subsystem. To use, apply the
   /// attribute directly to a controller class or action method, register
   /// it directly using GlobalFilterCollection.Add(), or add it to 
   /// web.config like the following example.
   /// </remarks>
   /// <example>
   ///   <configuration>
   ///      <NLogEx.Mvc>
   ///         <filters>
   ///            <add type="NLogEx.Mvc.LogAttribute,NLogEx.Mvc"
   ///               controller="MyController"
   ///               action="MyAction"/>
   ///         </filters>
   ///      </NLogEx.Mvc>
   ///   </configuration>
   /// </example>
   [AttributeUsage(
      AttributeTargets.Class | AttributeTargets.Method,
      AllowMultiple = false,
      Inherited = true)
   ]
   public sealed class LogAttribute : ActionFilterAttribute
   {
      private const String ContextKeyName = "NLogEx.Mvc.Action";
      private Log Log = null;

      #region Properties
      /// <summary>
      /// Enable/disable the log
      /// </summary>
      public Boolean Disabled { get; set; }
      /// <summary>
      /// Enable/disable the log
      /// </summary>
      public Boolean Enabled
      {
         get { return !this.Disabled; }
         set { this.Disabled = !value; }
      }
      #endregion

      #region ActionFilterAttribute Overrides
      /// <summary>
      /// Action execution begin callback
      /// </summary>
      /// <param name="context">
      /// The current action context
      /// </param>
      public override void OnActionExecuting (ActionExecutingContext context)
      {
         base.OnActionExecuting(context);
         if (this.Enabled)
         {
            // lazy allocate the log with the controller type name
            if (this.Log == null)
               this.Log = new Log(context.Controller.GetType());
            // create the context for this action
            context.HttpContext.Items[ContextKeyName] = new Action(
               context.ActionDescriptor.ControllerDescriptor.ControllerName,
               context.ActionDescriptor.ActionName,
               context.RouteData
            );
         }
      }
      /// <summary>
      /// Action execution end callback
      /// </summary>
      /// <param name="context">
      /// The current action context
      /// </param>
      public override void OnActionExecuted (ActionExecutedContext context)
      {
         base.OnActionExecuted(context);
         if (this.Enabled)
         {
            // lazy allocate the log with the controller type name
            if (this.Log == null)
               this.Log = new Log(context.Controller.GetType());
            // retrieve the action context and stop the timer
            Action action = (Action)context.HttpContext.Items[ContextKeyName];
            action.Stop();
            context.HttpContext.Items.Remove(ContextKeyName);
            // trace the action completed and
            // log any exceptions that occurred
            this.Log.TraceEx(action, "{0}.{1}", action.Controller, action.Name);
            if (context.Exception != null)
               this.Log.ErrorEx(action, context.Exception);
         }
      }
      #endregion

      /// <summary>
      /// Action log context
      /// </summary>
      private sealed class Action
      {
         private Int64 started;
         private Int64 stopped;
         private String controller;
         private String name;
         private RouteData routeParams;

         /// <summary>
         /// Initializes a new action instance
         /// </summary>
         /// <param name="controller">
         /// The action's controller name
         /// </param>
         /// <param name="name">
         /// The action's name
         /// </param>
         /// <param name="routeParams">
         /// MVC routing data
         /// </param>
         public Action (String controller, String name, RouteData routeParams)
         {
            this.started = Log.GetTimestamp();
            this.controller = controller;
            this.name = name;
            this.routeParams = routeParams;
         }
         /// <summary>
         /// Action controller name
         /// </summary>
         public String Controller
         { 
            get { return this.controller; } 
         }
         /// <summary>
         /// Action name
         /// </summary>
         public String Name
         {
            get { return this.name; }
         }
         /// <summary>
         /// Action execution duration, in microseconds
         /// </summary>
         public Int64 Duration
         {
            get { return this.stopped - this.started; }
         }
         /// <summary>
         /// Action routing parameters
         /// </summary>
         public String RouteParams
         {
            get
            {
               return String.Join(
                  ";",
                  routeParams.Values
                     .Select(r => String.Format("{0}={1}", r.Key, r.Value))
               );
            }
         }
         /// <summary>
         /// Stops the timer for the action
         /// </summary>
         public void Stop ()
         {
            this.stopped = Log.GetTimestamp();
         }
      }
   }
}
