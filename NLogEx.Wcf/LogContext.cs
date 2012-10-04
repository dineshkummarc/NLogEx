//===========================================================================
// MODULE:  LogContext.cs
// PURPOSE: WCF logging context
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
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
// Project References

namespace NLogEx.Wcf
{
   /// <summary>
   /// The WCF logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// common WCF channel/operation context properties
   /// and during event logging.
   /// </remarks>
   public sealed class LogContext
   {
      #region WCF Context
      /// <summary>
      /// The current WCF operation context
      /// </summary>
      private static OperationContext Operation
      {
         get { return OperationContext.Current; }
      }
      #endregion

      #region Operations
      /// <summary>
      /// Establishes the WCF context with the logging subsystem
      /// </summary>
      public static void Register ()
      {
         Log.RegisterContext(
            "Wcf",
            new Dictionary<String, Func<Object>>()
            {
               { "SessionID", () => (Operation != null) ? Operation.SessionId : null },
               { "LocalAddress", () => (Operation != null) ? Convert.ToString(Operation.Channel.LocalAddress) : null },
               { "RemoteAddress", () => (Operation != null) ? Convert.ToString(Operation.Channel.RemoteAddress) : null },
            }
         );
      }
      #endregion
   }
}
