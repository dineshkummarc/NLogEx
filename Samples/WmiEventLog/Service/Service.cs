//===========================================================================
// MODULE:  Service.cs
// PURPOSE: WMI event log Windows service
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
using System.Dynamic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
// Project References

namespace NLogEx.Samples.WmiEventLog
{
   /// <summary>
   /// The windows service
   /// </summary>
   /// <remarks>
   /// This service listens for all WMI events registered in App.Config
   /// and logs them via NLogEx.
   /// </remarks>
   [System.ComponentModel.DesignerCategory("Code")]
   internal sealed class Service : ServiceBase
   {
      private List<ManagementEventWatcher> watchers =
         new List<ManagementEventWatcher>();
      private Log Log = new Log(typeof(ManagementEventWatcher));

      #region ServiceBase Overrides
      /// <summary>
      /// Service startup override
      /// </summary>
      /// <param name="args">
      /// Service command line parameters
      /// </param>
      protected override void OnStart (String[] args)
      {
         Register();
      }
      /// <summary>
      /// Service stop override
      /// </summary>
      protected override void OnStop ()
      {
         Unregister();
      }
      /// <summary>
      /// Service shutdown override
      /// </summary>
      protected override void OnShutdown ()
      {
         OnStop();
      }
      #endregion

      #region Operations
      /// <summary>
      /// Registers for the configured WMI events
      /// </summary>
      public void Register ()
      {
         lock (this)
         {
            foreach (var query in Config.Queries)
            {
               var scope = new ManagementScope(query.Scope);
               scope.Options.EnablePrivileges = true;
               scope.Connect();
               var watcher = new ManagementEventWatcher(
                  scope, 
                  new EventQuery(query.Query)
               );
               watcher.EventArrived += HandleEvent;
               watcher.Start();
               this.watchers.Add(watcher);
            }
         }
      }
      /// <summary>
      /// Unregisters the configured WMI events
      /// </summary>
      public void Unregister ()
      {
         lock (this)
         {
            foreach (var watcher in this.watchers)
               try { watcher.Dispose(); }
               catch { }
            this.watchers.Clear();
         }
      }
      /// <summary>
      /// WMI event handler
      /// </summary>
      /// <param name="o">
      /// The object that raised the event
      /// </param>
      /// <param name="a">
      /// The event parameters
      /// </param>
      private void HandleEvent (Object o, EventArrivedEventArgs a)
      {
         Dictionary<String, Object> evtProps = new Dictionary<String, Object>()
         {
            { "Context", "WmiEvent" },
            { "Server", a.NewEvent.SystemProperties["__SERVER"].Value },
            { "Namespace", a.NewEvent.SystemProperties["__NAMESPACE"].Value },
            { "Class", a.NewEvent.SystemProperties["__CLASS"].Value }
         };
         AddProperties(null, evtProps, a.NewEvent.Properties);
         Log.InfoEx(
            evtProps,
            "{0}/{1}",
            a.NewEvent.SystemProperties["__NAMESPACE"].Value,
            a.NewEvent.SystemProperties["__CLASS"].Value
         );
      }
      /// <summary>
      /// Recursively adds WMI event properties to a dictionary
      /// </summary>
      /// <param name="prefix">
      /// The recursive event property name prefix
      /// </param>
      /// <param name="evtProps">
      /// The event property collection to construct
      /// </param>
      /// <param name="wmiProps">
      /// The WMI property collection source
      /// </param>
      private void AddProperties (
         String prefix,
         IDictionary<String, Object> evtProps,
         PropertyDataCollection wmiProps)
      {
         foreach (var prop in wmiProps)
         {
            var name = String.Format(
               (prefix != null) ? "{0}.{1}" : "{1}",
               prefix,
               prop.Name
            );
            var wmiObject = prop.Value as ManagementBaseObject;
            if (wmiObject != null)
               AddProperties(name, evtProps, wmiObject.Properties);
            else
               evtProps.Add(name, prop.Value);
         }
      }
      #endregion
   }
}
