//===========================================================================
// MODULE:  Config.cs
// PURPOSE: ASP.NET MVC logging configuration
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
using System.Web.Mvc;
// Project References

namespace NLogEx.Mvc
{
   /// <summary>
   /// The MVC logging configuration class
   /// </summary>
   /// <remarks>
   /// This class encapsulates the <NLogEx.Mvc> Web.Config section for 
   /// ASP.NET MVC applications. To use, add the <NLogEx.Mvc> section to
   /// your Web.Config file, and then call 
   ///   FilterProviders.Providers.Add(NLogEx.Mvc.Config.FilterProvider)
   /// in Global.Asax.Cs to register the configured action filter provider.
   /// </remarks>
   /// <example>
   ///   <configuration>
   ///      <configSections>
   ///         <section name="NLogEx.Mvc" type="NLogEx.Mvc.Config,NLogEx.Mvc"/>
   ///      </configSections>
   ///      <NLogEx.Mvc>
   ///         <filters>...</filters>
   ///      </NLogEx.Mvc>
   ///   </configuration>
   /// </example>
   public sealed class Config : ConfigurationSection
   {
      private static Lazy<Config> config =
         new Lazy<Config>(() => (Config)ConfigurationManager.GetSection("NLogEx.Mvc"));

      #region Configuration Properties
      [ConfigurationProperty("filters")]
      private FilterElement.Collection ConfigFilters
      {
         get { return ((FilterElement.Collection)base["filters"]); }
      }
      //---------------------------------------------------------------------
      // KLUDGE: the configuration manager does not allow attributes to
      // exist on a configuration section that are not also properties
      // on the section class
      // therefore, these properties must exist to support intellisense
      // in the MVC configuration sections
      //---------------------------------------------------------------------
      [ConfigurationProperty("xmlns")]
      private String Ns1 { get { return null; } }
      [ConfigurationProperty("xmlns:xsi")]
      private String Ns2 { get { return null; } }
      [ConfigurationProperty("xsi:noNamespaceSchemaLocation")]
      private String Ns3 { get { return null; } }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The configuration-based action filter provider
      /// </summary>
      public static IFilterProvider FilterProvider
      {
         get { return (config.Value != null) ? config.Value.ConfigFilters : null; }
      }
      #endregion
   }
}
