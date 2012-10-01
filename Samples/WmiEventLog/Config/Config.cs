//===========================================================================
// MODULE:  Config.cs
// PURPOSE: WMI event log configuration
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

namespace NLogEx.Samples.WmiEventLog
{
   /// <summary>
   /// WMI event log configuration class
   /// </summary>
   /// <remarks>
   /// This class contains the list of WMI events to log. See App.Config for
   /// a sample.
   /// </remarks>
   internal sealed class Config : ConfigurationSection
   {
      private static Lazy<Config> config =
         new Lazy<Config>(() => (Config)ConfigurationManager.GetSection("NLogEx.Samples.WmiEventLog"));

      #region Configuration Properties
      [ConfigurationProperty("queries")]
      private QueryElement.Collection ConfigQueries
      {
         get { return (QueryElement.Collection)base["queries"]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The list of WMI queries to subscribe
      /// </summary>
      internal static IEnumerable<QueryElement> Queries
      {
         get { return config.Value.ConfigQueries.Cast<QueryElement>(); }
      }
      #endregion
   }
}
