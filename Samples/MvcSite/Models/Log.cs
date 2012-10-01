//===========================================================================
// MODULE:  Log.cs
// PURPOSE: log event model
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
using System.Collections.Concurrent;
using System.Linq;
// Project References

namespace MvcSite.Models
{
   /// <summary>
   /// The event log model
   /// </summary>
   /// <remarks>
   /// The model state consists of the current page number and a
   /// selection of the events from the global queue based on the
   /// page number and page length.
   /// Individual events are identified by their position in the queue
   /// and can be fetched by ID using the Fetch method.
   /// </remarks>
   public sealed class Log
   {
      private Int32 pageLength = 10;
      private Int32 pageNumber = 0;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new model instance
      /// </summary>
      /// <param name="pageNumber">
      /// The current page number
      /// </param>
      public Log (Int32 pageNumber = 0)
      {
         this.pageNumber = Math.Max(Math.Min(pageNumber, this.PageCount - 1), 0);
      }
      #endregion

      #region Model Properties
      /// <summary>
      /// The current page of events from the queue,
      /// in reverse publication order
      /// </summary>
      public IEnumerable<Event> Events
      {
         get
         { 
            return Logger.Queue
               .Select((e, i) => new Event(i, e))
               .Reverse()
               .Skip(this.PageNumber * this.PageLength)
               .Take(this.PageLength);
         }
      }
      /// <summary>
      /// The current page number
      /// </summary>
      public Int32 PageNumber
      {
         get { return this.pageNumber; }
      }
      /// <summary>
      /// The number of events per page
      /// </summary>
      public Int32 PageLength
      {
         get { return this.pageLength; }
      }
      /// <summary>
      /// The total number of event pages
      /// </summary>
      public Int32 PageCount
      {
         get { return (Int32)Math.Ceiling((Double)Logger.Queue.Count / this.PageLength); }
      }
      #endregion

      #region Model Operations
      /// <summary>
      /// Fetches an individual event from the queue
      /// </summary>
      /// <param name="id">
      /// ID of the event to fetch
      /// </param>
      /// <returns>
      /// The requested event
      /// </returns>
      public static Event Fetch (Int32 id)
      {
         return Logger.Queue.Skip(id).Select(e => new Event(id, e)).First();
      }
      #endregion

      /// <summary>
      /// The log event model
      /// </summary>
      public struct Event
      {
         private Int32 id;
         private NLogEx.Event logEvent;

         /// <summary>
         /// Initializes a new event instance
         /// </summary>
         /// <param name="id">
         /// Event ID
         /// </param>
         /// <param name="logEvent">
         /// Event properties
         /// </param>
         public Event (Int32 id, NLogEx.Event logEvent)
         {
            this.id = id;
            this.logEvent = logEvent;
         }
         /// <summary>
         /// Event identifier
         /// </summary>
         public Int32 ID
         { 
            get { return this.id; } 
         }
         /// <summary>
         /// Event type/severity
         /// </summary>
         public NLogEx.EventType Type
         {
            get { return (NLogEx.EventType)this.logEvent["Event.Type"]; }
         }
         /// <summary>
         /// Event publication time
         /// </summary>
         public DateTime Time
         {
            get { return ((DateTime)this.logEvent["Event.Time"]).ToLocalTime(); }
         }
         /// <summary>
         /// Event text message
         /// </summary>
         public String Message
         {
            get { return (String)this.logEvent["Event.Message"]; }
         }
         /// <summary>
         /// MVC action duration
         /// </summary>
         public Double Duration
         {
            get
            {
               var duration = (Int64?)this.logEvent["Action.Duration"];
               return (Double)(duration ?? 0) / 1000;
            }
         }
         /// <summary>
         /// The full list of event properties
         /// </summary>
         public IList<KeyValuePair<String, Object>> Properties
         {
            get { return this.logEvent.Properties; }
         }
      }

      /// <summary>
      /// Event queue logger
      /// </summary>
      /// <remarks>
      /// This class encapsulates a site-wide in-memory event log that 
      /// collects any NLogEx events logged during site execution. The
      /// log is registered with NLogEx via Web.Config.
      /// </remarks>
      public sealed class Logger : NLogEx.ILogger
      {
         public static ConcurrentQueue<NLogEx.Event> Queue =
            new ConcurrentQueue<NLogEx.Event>();

         #region ILogger Implementation
         /// <summary>
         /// Logger initialization
         /// </summary>
         /// <param name="properties">
         /// Event properties included in the log
         /// </param>
         public void Initialize (IEnumerable<String> properties)
         {
         }
         /// <summary>
         /// Writes a set of events to the event queue
         /// </summary>
         /// <param name="events">
         /// The events to log, in order of publication
         /// </param>
         public void Log (IList<NLogEx.Event> events)
         {
            foreach (var evt in events)
               Queue.Enqueue(evt);
         }
         #endregion
      }
   }
}
