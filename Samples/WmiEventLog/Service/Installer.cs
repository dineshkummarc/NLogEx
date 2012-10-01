//===========================================================================
// MODULE:  Installer.cs
// PURPOSE: installer for the WMI event log Windows service
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
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
// Project References

namespace NLogEx.Samples.WmiEventLog
{
   /// <summary>
   /// WMI event log service installer
   /// </summary>
   /// <remarks>
   /// This class configures the installation of the
   /// Windows service, which can be done via installutil.
   /// </remarks>
   [RunInstaller(true)]
   [System.ComponentModel.DesignerCategory("Code")]
   public sealed class Installer : System.Configuration.Install.Installer
   {
      ServiceProcessInstaller processInstaller;
      ServiceInstaller serviceInstaller;

      #region Construction/Disposal
      /// <summary>
      /// Initializes the installer
      /// </summary>
      public Installer ()
      {
         Installers.Add(
            this.processInstaller = new ServiceProcessInstaller()
            {
               Account = ServiceAccount.LocalSystem
            }
         );
         Installers.Add(
            this.serviceInstaller = new ServiceInstaller()
            {
               ServiceName = "NLogEx.Samples.WmiEventLog",
               Description = "WMI Event Log",
               StartType = ServiceStartMode.Automatic,
               DelayedAutoStart = true
            }
         );
      }
      #endregion
   }
}
