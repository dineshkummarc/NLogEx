//===========================================================================
// MODULE:  Config.cs
// PURPOSE: package updater configuration
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
using System.Configuration;
using System.Linq;
// Project References

namespace NLogEx.Samples.PackageUpdater
{
   /// <summary>
   /// Package configuration class
   /// </summary>
   /// <remarks>
   /// This class contains the list of NuGet deployments to
   /// monitor and continuously upgrade. See App.Config for
   /// a sample.
   /// </remarks>
   internal sealed class Config : ConfigurationSection
   {
      private static Lazy<Config> config =
         new Lazy<Config>(() => (Config)ConfigurationManager.GetSection("NLogEx.Samples.PackageUpdater"));

      #region Configuration Properties
      [ConfigurationProperty("repository", DefaultValue = "packages")]
      private String ConfigRepository
      {
         get { return System.IO.Path.GetFullPath((String)base["repository"]); }
      }
      [ConfigurationProperty("frequency", DefaultValue = "00:05:00")]
      private TimeSpan ConfigFrequency
      {
         get { return (TimeSpan)base["frequency"]; }
      }
      [ConfigurationProperty("packages")]
      private PackageElement.Collection ConfigPackages
      {
         get { return (PackageElement.Collection)base["packages"]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// Path to the root of the local package repository
      /// </summary>
      public static String Repository
      {
         get { return config.Value.ConfigRepository; }
      }
      /// <summary>
      /// The package update frequency
      /// </summary>
      public static TimeSpan Frequency
      {
         get { return config.Value.ConfigFrequency; }
      }
      /// <summary>
      /// The list of package deployments to monitor
      /// </summary>
      internal static IEnumerable<PackageElement> Packages
      {
         get { return config.Value.ConfigPackages.Cast<PackageElement>(); }
      }
      #endregion
   }
}
