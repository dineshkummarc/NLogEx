//===========================================================================
// MODULE:  Service.cs
// PURPOSE: package updater Windows service
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
using System.IO;
using System.ServiceProcess;
using System.Timers;
// Project References

namespace NLogEx.Samples.PackageUpdater
{
   /// <summary>
   /// The windows service
   /// </summary>
   /// <remarks>
   /// This service wakes up on a timer and updates all NuGet packages
   /// registered in App.Config.
   /// </remarks>
   [System.ComponentModel.DesignerCategory("Code")]
   internal sealed class Service : ServiceBase
   {
      private Timer timer;

      #region ServiceBase Overrides
      /// <summary>
      /// Service startup override
      /// </summary>
      /// <param name="args">
      /// Service command line parameters
      /// </param>
      protected override void OnStart (String[] args)
      {
         this.timer = new Timer()
         {
            AutoReset = true,
            Interval = Config.Frequency.TotalMilliseconds
         };
         this.timer.Elapsed += (o, a) => Update();
         this.timer.Start();
      }
      /// <summary>
      /// Service stop override
      /// </summary>
      protected override void OnStop ()
      {
         this.timer.Stop();
         lock (this) { }   // wait for any updates to complete
      }
      /// <summary>
      /// Service shutdown override
      /// </summary>
      protected override void OnShutdown ()
      {
         OnStop();
      }
      /// <summary>
      /// Service control code handler
      /// </summary>
      /// <param name="code">
      /// Service control code
      /// </param>
      protected override void OnCustomCommand (Int32 code)
      {
         if (code == 200)
            Update();
      }
      #endregion

      #region Operations
      /// <summary>
      /// Checks for updates to the configured packages,
      /// and applies any that are available
      /// </summary>
      public void Update ()
      {
         lock (this)
            foreach (var package in Config.Packages)
               new DeploymentManager()
               {
                  Source = package.Source,
                  RepositoryRoot = Path.Combine(Config.Repository, package.ID),
                  DeploymentRoot = package.DeployRoot,
                  PackageID = package.ID,
                  TargetFramework = package.Framework,
                  LibPath = package.LibPath,
                  ToolPath = package.ToolPath,
                  ContentPath = package.ContentPath,
                  OtherPath = package.OtherPath,
                  TempPath = Path.GetTempPath()
               }.Update();
      }
      #endregion
   }
}
