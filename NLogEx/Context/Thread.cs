//===========================================================================
// MODULE:  Thread.cs
// PURPOSE: thread logging context
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
   /// The thread logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// current thread properties during event logging.
   /// </remarks>
   public static class Thread
   {
      /// <summary>
      /// Establishes the thread context with the logging subsystem
      /// </summary>
      public static void Register ()
      {
         Log.RegisterContext(
            "Thread",
            new Dictionary<String, Func<Object>>()
            {
               { "ID", () => System.Threading.Thread.CurrentThread.ManagedThreadId },
               { "StackTrace", () => GetStackTrace() }
            }
         );
      }
      /// <summary>
      /// Retrieves the current thread stack trace
      /// </summary>
      /// <returns>
      /// The stack trace for the current thread,
      /// excluding any logging frames
      /// </returns>
      private static String GetStackTrace ()
      {
         return String.Join(
            "\r\n",
            System.Environment.StackTrace.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Skip(9)
         );
      }
   }
}
