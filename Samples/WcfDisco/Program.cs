//===========================================================================
// MODULE:  Program.cs
// PURPOSE: WCF discovery logging sample client/server program
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

namespace NLogEx.WcfDisco
{
   /// <summary>
   /// The WCF disco logging program
   /// </summary>
   /// <remarks>
   /// This program uses the bindings configured in App.Config to start
   /// up and call a sample WCF service using the WCF endpoint discovery
   /// mechanism. All calls through the discovery endpoint are logged via
   /// the configured LogBehavior in App.Config.
   /// </remarks>
   static class Program
   {
      /// <summary>
      /// Program entry point
      /// </summary>
      static void Main ()
      {
         Console.WriteLine("NLogEx WCF Discover Logging");
         Console.Write("   Starting up the service host...");
         using (var host = new ServiceHost(typeof(Service)))
         {
            host.Open();
            Console.WriteLine("done.");
            Console.WriteLine("   Multiple calls through shared client...");
            using (var client = new Client())
               for (Int32 i = 0; i < 10; i++)
                  client.Server.Execute();
            System.Threading.Thread.Sleep(100);
            Console.WriteLine("   Done.");
            Console.WriteLine("   Multiple calls through dedicated clients...");
            for (Int32 i = 0; i < 10; i++)
               using (var client = new Client())
                  client.Server.Execute();
            System.Threading.Thread.Sleep(100);
            Console.WriteLine("   Done.");
         }
      }
   }
}
