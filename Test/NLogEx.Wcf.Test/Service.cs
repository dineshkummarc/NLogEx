//===========================================================================
// MODULE:  Service.cs
// PURPOSE: logging test WCF server
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
// Project References

namespace NLogEx.Wcf.Test
{
   /// <summary>
   /// Test client callback
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the unit test driver
   /// to support testing WCF client callback logging.
   /// </remarks>
   public interface ICallback
   {
      /// <summary>
      /// Callback dispatch with asutomatic logging
      /// </summary>
      [OperationContract]
      void AutoLog ();
      /// <summary>
      /// Callback dispatch with manual logging
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      [OperationContract]
      void ManualLog (String message);
   }

   /// <summary>
   /// Test server contract (simplex)
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the WCF logging framework.
   /// </remarks>
   [ServiceContract(
      Namespace = "http://brentspell.us/Projects/NLogEx/Test/Wcf",
      SessionMode = SessionMode.Required)
   ]
   public interface ISimplexServer
   {
      /// <summary>
      /// Server execution
      /// </summary>
      [OperationContract]
      void AutoLog ();
      /// <summary>
      /// Server execution with manual logging
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      [OperationContract]
      void ManualLog (String message);
      /// <summary>
      /// Throws an exception
      /// </summary>
      /// <param name="message">
      /// The exception message
      /// </param>
      [OperationContract]
      void Throw (String message);
   }

   /// <summary>
   /// Test server contract (dumplex)
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the WCF logging framework.
   /// </remarks>
   [ServiceContract(
      Namespace = "http://brentspell.us/Projects/NLogEx/Test/Wcf",
      CallbackContract = typeof(ICallback),
      SessionMode = SessionMode.Required)
   ]
   public interface IDuplexServer
   {
      /// <summary>
      /// Server execution with callback
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      [OperationContract]
      void CallbackAutoLog ();
      /// <summary>
      /// Server execution with callback logging
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      [OperationContract]
      void CallbackManualLog (String message);
   }

   /// <summary>
   /// Test server
   /// </summary>
   /// <remarks>
   /// This class implements the test contract interface.
   /// </remarks>
   [ServiceBehavior(
      ConcurrencyMode = ConcurrencyMode.Multiple,
      IncludeExceptionDetailInFaults = true)
   ]
   public class Server : ISimplexServer, IDuplexServer
   {
      private static Log Log = new Log(typeof(Server));

      #region ISimplexServer Implementation
      /// <summary>
      /// Server execution
      /// </summary>
      public void AutoLog ()
      {
      }
      /// <summary>
      /// Server execution with manual logging
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      public void ManualLog (String message)
      {
         Log.Info(message);
      }
      /// <summary>
      /// Throws an exception
      /// </summary>
      /// <param name="message">
      /// The exception message
      /// </param>
      public void Throw (String message)
      {
         throw new Exception(message);
      }
      #endregion

      #region IDulexServer Implementation
      /// <summary>
      /// Server execution
      /// </summary>
      public void CallbackAutoLog ()
      {
         OperationContext.Current
            .GetCallbackChannel<ICallback>()
            .AutoLog();
      }
      /// <summary>
      /// Server execution with manual logging
      /// </summary>
      /// <param name="message">
      /// The message to log
      /// </param>
      public void CallbackManualLog (String message)
      {
         OperationContext.Current
            .GetCallbackChannel<ICallback>()
            .ManualLog(message);
      }
      #endregion
   }
}
