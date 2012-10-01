//===========================================================================
// MODULE:  Instance.cs
// PURPOSE: log instance
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
   /// The event log instance
   /// </summary>
   /// <remarks>
   /// This class represents an event logger and its 
   /// configuration. Event log instances are registered
   /// with the logging subsystem and are used to determine
   /// how to process incoming events.
   /// </remarks>
   public sealed class Instance
   {
      #region Properties
      /// <summary>
      /// The list of properties to sample from
      /// the event context
      /// </summary>
      public IList<String> Properties { get; set; }
      /// <summary>
      /// The event filter
      /// </summary>
      public Filter Filter { get; set; }
      /// <summary>
      /// The event logger
      /// </summary>
      public ILogger Logger { get; set; }
      /// <summary>
      /// Specifies whether and how many events
      /// to buffer before calling the logger
      /// </summary>
      public Int32 Buffer { get; set; }
      /// <summary>
      /// Specifies whether to send events
      /// synchronously to the logger or queue
      /// them up for async deliver
      /// </summary>
      public Boolean Synchronous { get; set; }
      #endregion

      #region Operations
      /// <summary>
      /// Validates the log instance
      /// </summary>
      public void Validate ()
      {
         if (this.Properties == null || !this.Properties.Any())
            throw new ArgumentException("this.Properties");
         foreach (String prop in this.Properties)
         {
            if (String.IsNullOrWhiteSpace(prop))
               throw new ArgumentException("this.Properties");
            Int32 sepIdx = prop.IndexOf('.');
            if (sepIdx <= 0 || sepIdx == prop.Length)
               throw new ArgumentException("this.Properties");
         }
         if (this.Filter == null)
            this.Filter = new Filter();
         this.Filter.Validate();
         if (this.Logger == null)
            throw new ArgumentException("Logger");
         if (this.Buffer < 0)
            throw new ArgumentException("Buffer");
      }
      #endregion
   }
}
