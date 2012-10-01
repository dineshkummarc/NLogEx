//===========================================================================
// MODULE:  LoggerElement.cs
// PURPOSE: logger configuration
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

namespace NLogEx
{
   /// <summary>
   /// Logger configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a <logger/> element within
   /// the log application configuration section. It
   /// is used to construct a runtime logger instance
   /// and initialize its properties.
   /// </remarks>
   /// <example>
   ///   <NlogEx>
   ///      <instances>
   ///         <add>
   ///            <properties>...</properties>
   ///            <filter>...</filter>
   ///            <logger type="MyNamespace.MyClass,MyAssembly">
   ///               <formatter>...</formatter>
   ///            </logger>
   ///         </add>
   ///      </instances>
   ///   </NlogEx>
   /// </example>
   internal sealed class LoggerElement : ConfigurationElement
   {
      #region Configuration Properties
      [ConfigurationProperty("properties")]
      private PropertyElement.Collection ConfigProperties
      {
         get { return (PropertyElement.Collection)base["properties"]; }
      }
      /// <summary>
      /// The CLR type name of the logger
      /// </summary>
      [ConfigurationProperty("type", IsRequired = true)]
      public String Type
      {
         get { return (String)base["type"]; }
      }
      /// <summary>
      /// The log message formatter
      /// </summary>
      [ConfigurationProperty("formatter")]
      private FormatterElement ConfigFormatter
      {
         get { return (FormatterElement)base["formatter"]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The list of configuration properties for the logger
      /// </summary>
      public new List<KeyValuePair<String, String>> Properties
      {
         get
         {
            return new List<KeyValuePair<String, String>>(
               this.ConfigProperties
                  .Cast<PropertyElement>()
                  .Select(p => new KeyValuePair<String, String>(p.Name, p.Value))
            );
         }
      }
      /// <summary>
      /// The configured log formatter for this logger
      /// </summary>
      public Formatter Formatter
      {
         get
         {
            Formatter formatter = new Formatter();
            if (this.ConfigFormatter != null)
            {
               formatter.Format = this.ConfigFormatter.String ?? formatter.Format;
               formatter.Wrap = this.ConfigFormatter.Wrap ?? formatter.Wrap;
               formatter.Indent = this.ConfigFormatter.Indent ?? formatter.Indent;
               formatter.Hang = this.ConfigFormatter.Hang ?? formatter.Hang;
               formatter.Width = this.ConfigFormatter.Width ?? formatter.Width;
            }
            return formatter;
         }
      }
      #endregion
   }
}
