//===========================================================================
// MODULE:  Config.cs
// PURPOSE: log configuration
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log configuration class
   /// </summary>
   /// <remarks>
   /// This class represents the NLogEx application configuration 
   /// section in an App.Config/Web.Config file.
   /// It is used to register log instances with the log framework
   /// via deployment configuration
   /// </remarks>
   /// <example>
   ///   <configuration>
   ///      <configSections>
   ///         <section name="NLogEx" type="NLogEx.Config,NLogEx"/>
   ///      </configSections>
   ///      <NLogEx>
   ///         <instances>
   ///            <add>...</add>
   ///         </instances>
   ///      </NLogEx>
   ///   </configuration>
   /// </example>
   public sealed class Config : ConfigurationSection
   {
      private static Lazy<Config> config =
         new Lazy<Config>(() => (Config)ConfigurationManager.GetSection("NLogEx"));

      #region Configuration Properties
      [ConfigurationProperty("instances")]
      private InstanceElement.Collection ConfigInstances
      {
         get { return ((InstanceElement.Collection)base["instances"]); }
      }
      //---------------------------------------------------------------------
      // KLUDGE: the configuration manager does not allow attributes to
      // exist on a configuration section that are not also properties
      // on the section class
      // therefore, these properties must exist to support intellisense
      // in application configuration sections
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
      /// The list of configured instances
      /// </summary>
      public static IEnumerable<Instance> Instances
      {
         get
         {
            if (config.Value != null && config.Value.ConfigInstances != null)
               foreach (InstanceElement elem in config.Value.ConfigInstances.Cast<InstanceElement>())
                  yield return CreateInstance(elem);
         }
      }
      /// <summary>
      /// Materializes a runtime log instance from configuration
      /// </summary>
      /// <param name="elem">
      /// The instance configuration element
      /// </param>
      /// <returns>
      /// The configured log instance
      /// </returns>
      private static Instance CreateInstance (InstanceElement elem)
      {
         // create the logger for this instance
         // bind the logger configuration properties
         ILogger logger = TypeLoader.CreateInstance<ILogger>(elem.Logger.Type);
         foreach (KeyValuePair<String, String> cfgProp in elem.Logger.Properties)
         {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(logger)
               .Cast<PropertyDescriptor>()
               .FirstOrDefault(p => String.Compare(p.Name, cfgProp.Key, true) == 0);
            if (prop == null)
               throw new ConfigException(logger, cfgProp.Key);
            try
            {
               prop.SetValue(logger, prop.Converter.ConvertFrom(cfgProp.Value));
            }
            catch (Exception e)
            {
               throw new ConfigException(logger, cfgProp.Key, e);
            }
         }
         // if the logger supports a Formatter property,
         // then create and bind the formatter
         PropertyInfo fmtProp = logger.GetType().GetProperty(
            "Formatter",
            BindingFlags.Public | BindingFlags.Instance
         );
         if (fmtProp != null)
            fmtProp.SetValue(logger, elem.Logger.Formatter, null);
         // return the new log instance
         return new Instance()
         {
            Properties = elem.Properties,
            Logger = logger,
            Buffer = elem.Buffer,
            Synchronous = elem.Synchronous,
            Filter = new Filter()
            {
               IncludeProps = elem.Filter.IncludeProperties
                  .Select(
                     p => new Filter.Property(
                        p.Name, 
                        p.Value, 
                        TypeLoader.CreateInstance<IEqualityComparer>(p.Comparer), 
                        p.Except
                     )
                  ).ToList(),
               ExcludeProps = elem.Filter.ExcludeProperties
                  .Select(
                     p => new Filter.Property(
                        p.Name, 
                        p.Value,
                        TypeLoader.CreateInstance<IEqualityComparer>(p.Comparer),
                        p.Except
                     )
                  ).ToList()
            }
         };
      }
      #endregion
   }
}
