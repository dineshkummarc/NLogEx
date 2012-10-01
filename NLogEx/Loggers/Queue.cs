//===========================================================================
// MODULE:  Queue.cs
// PURPOSE: in-memory queue logger
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
   /// The in-memory queue logger
   /// </summary>
   /// <remarks>
   /// This class stores all logged events in
   /// a queue object, for later retrieval,
   /// useful for log testing.
   /// </remarks>
   public sealed class Queue : ILogger
   {
      private ConcurrentQueue<Event> events = new ConcurrentQueue<Event>();

      #region Operations
      /// <summary>
      /// Retrieves events from the log queue
      /// </summary>
      /// <returns>
      /// An enumeration of all received events
      /// </returns>
      public IEnumerable<Event> Dequeue ()
      {
         Event evt;
         while (this.events.TryDequeue(out evt))
            yield return evt;
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
            this.events.Enqueue(evt);
      }
      #endregion
   }
}
