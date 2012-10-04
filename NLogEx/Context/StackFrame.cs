//===========================================================================
// MODULE:  StackFrame.cs
// PURPOSE: runtime stack frame logging context
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
   /// The stack frame logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// the properties of the runtime stack at the
   /// point at which an event was logged.
   /// </remarks>
   public static class StackFrame
   {
      /// <summary>
      /// The current CLR stack frame object
      /// </summary>
      public static System.Diagnostics.StackFrame Frame
      {
         get { return new System.Diagnostics.StackFrame(7, true); }
      }
      /// <summary>
      /// Establishes the stack frame context with the logging subsystem
      /// </summary>
      public static void Register ()
      {
         Log.RegisterContext(
            "StackFrame",
            new Dictionary<String, Func<Object>>()
            {
               { "File", () => Frame.GetFileName() },
               { "Line", () => Frame.GetFileLineNumber() },
               { "Type", () => Frame.GetMethod().DeclaringType.FullName },
               { "Method", () => Frame.GetMethod().Name },
            }
         );
      }
   }
}
