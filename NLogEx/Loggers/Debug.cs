//===========================================================================
// MODULE:  Debug.cs
// PURPOSE: debug console logger
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
// Compiler Symbols
#define DEBUG
// System References
using System;
using System.Collections.Generic;
using System.Linq;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The debug console logger
   /// </summary>
   /// <remarks>
   /// This class writes all log events to the 
   /// debugger console (System.Diagnostics).
   /// </remarks>
   public sealed class Debug : ILogger
   {
      #region Configuration Properties
      /// <summary>
      /// The log message formatter
      /// </summary>
      public Formatter Formatter { get; set; }
      #endregion

      #region ILogger Implementation
      /// <summary>
      /// Log initialization
      /// </summary>
      /// <param name="properties">
      /// Log event properties
      /// </param>
      public void Initialize (IEnumerable<String> properties)
      {
         if (this.Formatter == null)
            throw new ConfigException(this, "Formatter");
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         System.Diagnostics.Debug.Write(this.Formatter.FormatEventListString(events));
      }
      #endregion
   }
}
