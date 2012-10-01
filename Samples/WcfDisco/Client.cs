//===========================================================================
// MODULE:  Client.cs
// PURPOSE: WCF discovery logging sample client channel
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
using System.ServiceModel;
// Project References

namespace NLogEx.WcfDisco
{
   /// <summary>
   /// Client channel
   /// </summary>
   /// <remarks>
   /// This class wraps the service channel for the IService service,
   /// using the bindings configured in App.Config.
   /// </remarks>
   public sealed class Client : ClientBase<IService>, IDisposable
   {
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      public Client ()
      {
         Open();
      }
      /// <summary>
      /// Closes the channel
      /// </summary>
      public void Dispose ()
      {
         try { base.Close(); }
         catch
         {
            try { base.Abort(); }
            catch { }
         }
      }
      /// <summary>
      /// The service interface
      /// </summary>
      public IService Server
      { 
         get { return (IService)base.Channel; } 
      }
   }
}
