//===========================================================================
// MODULE:  Program.cs
// PURPOSE: NuGet automatic package update program/service
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
using System.Reflection;
// Project References

namespace NLogEx.Samples.PackageUpdater
{
   /// <summary>
   /// The NuGet package update program
   /// </summary>
   /// <remarks>
   /// This program starts a Windows service that continuously monitors a
   /// set of deployed NuGet packages registered in App.Config. It 
   /// automatically updates these packages as new versions are published
   /// at the NuGet source.
   /// The program can be set up as a Windows service for a poor man's
   /// continuous integration server. Simply modify your application's 
   /// release build process to publish your packages to a hosted NuGet 
   /// source, and the package updater will apply these updates as they
   /// become available.
   /// </remarks>
   static class Program
   {
      /// <summary>
      /// Program entry point
      /// </summary>
      static void Main ()
      {
         // ensure we are running in the application directory
         Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location
         );
         // start up the service instance
         var service = new Service();
         if (!System.Diagnostics.Debugger.IsAttached)
            Service.Run(service);
         else
            service.Update();
      }
   }
}
