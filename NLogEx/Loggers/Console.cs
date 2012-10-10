//===========================================================================
// MODULE:  Console.cs
// PURPOSE: System.Console.Out logger
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
using System.Runtime.InteropServices;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The console logger
   /// </summary>
   /// <remarks>
   /// This class writes all received events to the
   /// current application console, creating a new
   /// console if the application is a Windows app.
   /// </remarks>
   public sealed class Console : ILogger
   {
      #region Windows Console API
      [DllImport(
         "kernel32.dll",
         EntryPoint = "AllocConsole",
         SetLastError = true,
         CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)
      ]
      private static extern int AllocConsole ();
      #endregion

      #region Configuration Properties
      /// <summary>
      /// Event message formatter
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
         // validate the logger
         if (this.Formatter == null)
            throw new ConfigException(this, "Formatter");
         // create a new console window if none exists
         AllocConsole();
         // configure the formatter to match the console width
         if (this.Formatter.Wrap && this.Formatter.Width == 0)
            this.Formatter.Width = System.Console.BufferWidth - 1;
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         lock (System.Console.Out)
         {
            // save off the current console foreground color
            ConsoleColor fgSave = System.Console.ForegroundColor;
            try
            {
               // write the events to the console and
               // then restore the foreground color
               foreach (Event evt in events)
               {
                  System.Console.ForegroundColor = MapEventColor(evt);
                  System.Console.Out.Write(this.Formatter.FormatEventString(evt));
               }
            }
            finally { System.Console.ForegroundColor = fgSave; }
         }
      }
      #endregion

      #region Console Operations
      /// <summary>
      /// Maps an an event type to a console color
      /// </summary>
      /// <param name="evt">
      /// The log event to map
      /// </param>
      /// <returns>
      /// The text color to use to display the event
      /// </returns>
      private ConsoleColor MapEventColor (Event evt)
      {
         switch ((EventType?)evt["Event.Type"])
         {
            case EventType.Trace:
               return ConsoleColor.Cyan;
            case EventType.Info:
               return ConsoleColor.White;
            case EventType.Warning:
               return ConsoleColor.Yellow;
            case EventType.Error:
               return ConsoleColor.Red;
            default:
               return System.Console.ForegroundColor;
         }
      }
      #endregion
   }
}
