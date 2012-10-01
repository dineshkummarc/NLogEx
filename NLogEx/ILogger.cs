//===========================================================================
// MODULE:  ILogger.cs
// PURPOSE: logger interface
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

namespace NLogEx
{
   /// <summary>
   /// The logger interface
   /// </summary>
   /// <remarks>
   /// This is the primary extension point in the
   /// logging framework. Implementers represent the
   /// sinks of application-generated log events.
   /// </remarks>
   public interface ILogger
   {
      /// <summary>
      /// Initializes the logger instance
      /// Called after the logger's properties have 
      /// been configured
      /// </summary>
      /// <param name="properties">
      /// The list of properties the logger will receive
      /// </param>
      void Initialize (IEnumerable<String> properties);
      /// <summary>
      /// Writes a list of log events to the log
      /// </summary>
      /// <param name="events">
      /// The log events to dispatch
      /// </param>
      void Log (IList<Event> events);
   }
}
