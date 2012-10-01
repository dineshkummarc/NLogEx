//===========================================================================
// MODULE:  Process.cs
// PURPOSE: application process logging context
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
// Project References

namespace NLogEx.Context
{
   /// <summary>
   /// The process logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// process-related properties during event logging.
   /// </remarks>
   public static class Process
   {
      /// <summary>
      /// Establishes the process context with the logging subsystem
      /// </summary>
      public static void Register ()
      {
         Log.RegisterContext(
            "Process",
            new Dictionary<String, Func<Object>>()
            {
               { "ID", () => UsingCurrent(p => p.Id) },
               { "Name", () => UsingCurrent(p => p.ProcessName) },
               { "StartTime", () => UsingCurrent(p => p.StartTime) },
               { "PeakVirtualMemory", () => UsingCurrent(p => p.PeakVirtualMemorySize64) },
               { "PeakPagedMemory", () => UsingCurrent(p => p.PeakPagedMemorySize64) },
               { "PeakWorkingSetMemory", () => UsingCurrent(p => p.PeakWorkingSet64) },
               { "ProcessorTime", () => UsingCurrent(p => p.TotalProcessorTime) },
               { "PrivilegedProcessorTime", () => UsingCurrent(p => p.PrivilegedProcessorTime) },
               { "UserProcessorTime", () => UsingCurrent(p => p.UserProcessorTime) },
            }
         );
      }
      /// <summary>
      /// Executes a delegate within the context
      /// of the current process state
      /// </summary>
      /// <typeparam name="T">
      /// The delegate return type
      /// </typeparam>
      /// <param name="func">
      /// The delegate to execute
      /// </param>
      /// <returns>
      /// The delegate return value
      /// </returns>
      private static T UsingCurrent<T> (Func<System.Diagnostics.Process, T> func)
      {
         using (System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess())
            return func(p);
      }
   }
}
