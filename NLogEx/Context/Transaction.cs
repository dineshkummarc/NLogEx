//===========================================================================
// MODULE:  Transaction.cs
// PURPOSE: transaction logging context
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
   /// The transaction logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// current transaction properties during event logging.
   /// </remarks>
   public static class Transaction
   {
      /// <summary>
      /// Retrieves properties of the current transaction
      /// </summary>
      private static System.Transactions.TransactionInformation Current
      {
         get 
         {
            return (System.Transactions.Transaction.Current != null) ?
               System.Transactions.Transaction.Current.TransactionInformation : 
               null;
         }
      }
      /// <summary>
      /// Establishes the transaction context with the logging subsystem
      /// </summary>
      public static void Register ()
      {
         Log.RegisterContext(
            "Transaction",
            new Dictionary<String, Func<Object>>()
            {
               { "LocalID", () => (Current != null) ? Current.LocalIdentifier : null },
               { "GlobalID", () => (Current != null && Current.DistributedIdentifier != Guid.Empty) ? (Guid?)Current.DistributedIdentifier : null },
               { "Started", () => (Current != null) ? (DateTime?)Current.CreationTime : null },
               { "State", () => (Current != null) ? (System.Transactions.TransactionStatus?)Current.Status : null },
            }
         );
      }
   }
}
