//===========================================================================
// MODULE:  Windows.cs
// PURPOSE: Windows event logger
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
using System.Diagnostics;
using System.Linq;
using System.Text;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The Windows event logger
   /// </summary>
   public sealed class Windows : ILogger
   {
      #region Configuration Properties
      /// <summary>
      /// Name of the Windows event log to write,
      /// defaults to Application
      /// </summary>
      public String LogName { get; set; }
      /// <summary>
      /// Optional event source override, defaults
      /// to the actual event source
      /// </summary>
      public String Source { get; set; }
      /// <summary>
      /// Event message formattter
      /// </summary>
      public Formatter Formatter { get; set; }
      #endregion

      #region ILogger Implementation
      /// <summary>
      /// Logger initialization
      /// </summary>
      /// <param name="properties">
      /// Log event properties
      /// </param>
      public void Initialize (IEnumerable<String> properties)
      {
         if (this.Formatter == null)
            throw new ConfigException(this, "Formatter");
         if (String.IsNullOrWhiteSpace(this.LogName))
            this.LogName = "Application";
         // construct the default format string
         if (String.IsNullOrWhiteSpace(Formatter.Format))
         {
            StringBuilder format = new StringBuilder();
            Int32 propIdx = -1;
            foreach (String prop in properties)
            {
               propIdx++;
               if (prop == "Event.Type")
                  continue;
               else if (prop == "Event.Source")
                  continue;
               else if (prop == "Event.Message")
                  format.Append(String.Format("{{{0}}}\r\n\r\n", propIdx));
               else if (prop == "Event.Exception")
                  format.Append(String.Format("{{{0}}}\r\n\r\n", propIdx));
               else
                  format.Append(String.Format("{0}: {{{1}}}\r\n", prop, propIdx));
            }
            this.Formatter.Format = format.ToString();
         }
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         foreach (Event evt in events)
         {
            // ensure the Windows event source exists
            String source = Convert.ToString(this.Source ?? evt["Event.Source"] ?? GetType().FullName);
            if (!EventLog.SourceExists(source))
               try { EventLog.CreateEventSource(source, this.LogName); }
               catch { }
            // write the event to the log
            EventLog.WriteEntry(source, Formatter.FormatEventString(evt), MapEventType(evt));
         }
      }
      #endregion

      #region Event Log Operations
      /// <summary>
      /// Converts a log event type to a Windows event type
      /// </summary>
      /// <param name="evt">
      /// The event to map
      /// </param>
      /// <returns>
      /// The Windows event type
      /// </returns>
      private EventLogEntryType MapEventType (Event evt)
      {
         switch ((EventType?)evt["Event.Type"])
         {
            case EventType.Warning:
               return EventLogEntryType.Warning;
            case EventType.Error:
               return EventLogEntryType.Error;
            default:
               return EventLogEntryType.Information;
         }
      }
      #endregion
   }
}
