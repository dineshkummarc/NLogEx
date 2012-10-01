//===========================================================================
// MODULE:  TraceListener.cs
// PURPOSE: log-based System.Diagnostics trace listener
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
// Project References

namespace NLogEx
{
   /// <summary>
   /// The logging trace listener
   /// </summary>
   /// <remarks>
   /// This class bridges the .NET tracing framework and
   /// the logging subsystem, providing the ability to
   /// route .NET trace events to the log via configuration.
   /// </remarks>
   public sealed class TraceListener : System.Diagnostics.TraceListener
   {
      private static Log log = new Log(typeof(TraceListener));

      #region TraceListener Overrides
      /// <summary>
      /// Writes a simple trace message to the log
      /// </summary>
      /// <param name="message">
      /// The message to write
      /// </param>
      public override void Write (String message)
      {
         log.Trace(message);
      }
      /// <summary>
      /// Writes a simple trace message to the log
      /// </summary>
      /// <param name="message">
      /// The message to write
      /// </param>
      public override void WriteLine (String message)
      {
         log.Trace(message);
      }
      /// <summary>
      /// Writes a trace data event to the log
      /// </summary>
      /// <param name="eventCache">
      /// Trace event cache
      /// </param>
      /// <param name="source">
      /// Event source name
      /// </param>
      /// <param name="eventType">
      /// Event type
      /// </param>
      /// <param name="id">
      /// Event unique identifier
      /// </param>
      /// <param name="data">
      /// Trace data
      /// </param>
      public override void TraceData (
         TraceEventCache eventCache, 
         String source, 
         TraceEventType eventType, 
         Int32 id, 
         Object data)
      {
         log.EventEx(MapTraceType(eventType), data, "{0}: {1}", source, id);
      }
      /// <summary>
      /// Writes a trace data event to the log
      /// </summary>
      /// <param name="eventCache">
      /// Trace event cache
      /// </param>
      /// <param name="source">
      /// Event source name
      /// </param>
      /// <param name="eventType">
      /// Event type
      /// </param>
      /// <param name="id">
      /// Event unique identifier
      /// </param>
      /// <param name="data">
      /// Trace data
      /// </param>
      public override void TraceData (
         TraceEventCache eventCache, 
         String source, 
         TraceEventType eventType, 
         Int32 id, 
         params Object[] data)
      {
         log.EventEx(MapTraceType(eventType), data, "{0}: {1}", source, id);
      }
      /// <summary>
      /// Writes a trace data event to the log
      /// </summary>
      /// <param name="eventCache">
      /// Trace event cache
      /// </param>
      /// <param name="source">
      /// Event source name
      /// </param>
      /// <param name="eventType">
      /// Event type
      /// </param>
      /// <param name="id">
      /// Event unique identifier
      /// </param>
      public override void TraceEvent (
         TraceEventCache eventCache, 
         String source, 
         TraceEventType eventType, 
         Int32 id)
      {
         log.Event(MapTraceType(eventType), "{0}: {1}", source, id);
      }
      /// <summary>
      /// Writes a trace data event to the log
      /// </summary>
      /// <param name="eventCache">
      /// Trace event cache
      /// </param>
      /// <param name="source">
      /// Event source name
      /// </param>
      /// <param name="eventType">
      /// Event type
      /// </param>
      /// <param name="id">
      /// Event unique identifier
      /// </param>
      /// <param name="format">
      /// Event message format string
      /// </param>
      /// <param name="args">
      /// Event message format parameters
      /// </param>
      public override void TraceEvent (
         TraceEventCache eventCache, 
         String source, 
         TraceEventType eventType, 
         Int32 id, 
         String format, 
         params Object[] args)
      {
         log.Event(MapTraceType(eventType), "{0}: {1} - {2}", source, id, String.Format(format, args));
      }
      /// <summary>
      /// Writes a trace data event to the log
      /// </summary>
      /// <param name="eventCache">
      /// Trace event cache
      /// </param>
      /// <param name="source">
      /// Event source name
      /// </param>
      /// <param name="eventType">
      /// Event type
      /// </param>
      /// <param name="id">
      /// Event unique identifier
      /// </param>
      /// <param name="message">
      /// Event message
      /// </param>
      public override void TraceEvent (
         TraceEventCache eventCache, 
         String source, 
         TraceEventType eventType, 
         Int32 id, 
         String message)
      {
         log.Event(MapTraceType(eventType), "{0}: {1} - {2}", source, id, message);
      }
      #endregion

      #region Trace Utilities
      /// <summary>
      /// Maps a trace event type to a log event type
      /// </summary>
      /// <param name="type">
      /// The trace event type
      /// </param>
      /// <returns>
      /// The mapped log event type
      /// </returns>
      EventType MapTraceType (TraceEventType type)
      {
         switch (type)
         {
            case TraceEventType.Information:
               return EventType.Info;
            case TraceEventType.Warning:
               return EventType.Warning;
            case TraceEventType.Error:
               return EventType.Error;
            case TraceEventType.Critical:
               return EventType.Error;
            default:
               return EventType.Trace;
         }
      }
      #endregion
   }
}
