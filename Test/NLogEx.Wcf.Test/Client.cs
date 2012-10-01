//===========================================================================
// MODULE:  Client.cs
// PURPOSE: logging test WCF client channel wrapper
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
// Project References

namespace NLogEx.Wcf.Test
{
   /// <summary>
   /// Client test channel wrapper
   /// </summary>
   public sealed class SimplexClient : ClientBase<ISimplexServer>, IDisposable
   {
      public SimplexClient (Binding binding, String address)
         : base(binding, new EndpointAddress(address))
      {
         Open();
      }
      public SimplexClient (ServiceEndpoint endpoint)
         : base(endpoint)
      {
         Open();
      }
      public void Dispose ()
      {
         try { Close(); }
         catch
         {
            try { Abort(); }
            catch { }
         }
      }
      public ISimplexServer Server
      {
         get { return (ISimplexServer)base.Channel; }
      }
      public IContextChannel Context
      {
         get { return (IContextChannel)base.Channel; }
      }

      public sealed class Callback : ICallback
      {
         private static Log Log = new Log(typeof(DuplexClient.Callback));

         void ICallback.AutoLog ()
         {
         }
         void ICallback.ManualLog (String message)
         {
            Log.Info(message);
         }
      }
   }

   /// <summary>
   /// Client test channel wrapper
   /// </summary>
   public sealed class DuplexClient : DuplexClientBase<IDuplexServer>, IDisposable
   {
      public DuplexClient (Binding binding, String address)
         : base(new Callback(), binding, new EndpointAddress(address))
      {
         Open();
      }
      public DuplexClient (ServiceEndpoint endpoint)
         : base(new Callback(), endpoint)
      {
         Open();
      }
      public void Dispose ()
      {
         try { Close(); }
         catch
         {
            try { Abort(); }
            catch { }
         }
      }
      public IDuplexServer Server
      {
         get { return (IDuplexServer)base.Channel; }
      }
      public IContextChannel Context
      {
         get { return (IContextChannel)base.Channel; }
      }

      public sealed class Callback : ICallback
      {
         private static Log Log = new Log(typeof(DuplexClient.Callback));

         void ICallback.AutoLog ()
         {
         }
         void ICallback.ManualLog (String message)
         {
            Log.Info(message);
         }
      }
   }
}
