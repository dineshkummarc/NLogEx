//===========================================================================
// MODULE:  Buffer.cs
// PURPOSE: log event buffer
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

namespace NLogEx.Loggers
{
   /// <summary>
   /// The log buffer
   /// </summary>
   /// <remarks>
   /// This class collects log events up to a
   /// maximum count before dispatching them to
   /// the attached logger instance.
   /// </remarks>
   internal sealed class Buffer : ILogger
   {
      private ILogger logger;
      private ConcurrentQueue<Event> queue;
      private Int32 size;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new buffer instance
      /// </summary>
      /// <param name="logger">
      /// The logger implementation
      /// </param>
      /// <param name="size">
      /// The number of events to buffer
      /// </param>
      public Buffer (ILogger logger, Int32 size)
      {
         this.logger = logger;
         this.queue = new ConcurrentQueue<Event>();
         this.size = size;
      }
      #endregion

      #region Operations
      /// <summary>
      /// Clears the buffer, and dispatches
      /// all events to the attached logger
      /// </summary>
      public void Flush ()
      {
         List<Event> events = new List<Event>(this.queue.Count);
         // drain the event queue
         Event evt;
         while (this.queue.TryDequeue(out evt))
            events.Add(evt);
         // dispatch the events to the logger
         if (events.Any())
            this.logger.Log(events);
      }
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
            this.queue.Enqueue(evt);
         if (this.queue.Count > this.size)
            Flush();
      }
      #endregion
   }
}
