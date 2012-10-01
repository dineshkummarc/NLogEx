//===========================================================================
// MODULE:  FormatterElement.cs
// PURPOSE: log formatter configuration
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
using System.Text.RegularExpressions;
// Project References

namespace NLogEx
{
   /// <summary>
   /// Log filter configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a <formatter/> element within
   /// the log application configuration section. It
   /// is used to construct a runtime formatter instance.
   /// </remarks>
   /// <example>
   ///   <NlogEx>
   ///     <instances>
   ///        <add>
   ///           ...
   ///           <properties>
   ///              <add>Event.Time</add>
   ///              <add>Event.Type</add>
   ///              <add>Event.Source</add>
   ///              <add>Event.Message</add>
   ///           </properties>
   ///           <logger>
   ///              <formatter 
   ///                 format="{0:MM/dd/yyyy HH:mm:ss} {1,-7} {2} {3}" 
   ///                 hang="3"/>
   ///           </logger>
   ///        </add>
   ///        ...
   ///     </instances>
   ///   </NlogEx>
   /// </example>
   internal sealed class FormatterElement : ConfigurationElement
   {
      #region Configuration Properties
      /// <summary>
      /// The format string for the log line
      /// </summary>
      [ConfigurationProperty("format")]
      public String String
      {
         get { return Regex.Unescape((String)base["format"] ?? ""); }
      }
      /// <summary>
      /// Specifies whether to wrap text to the next line if its length
      /// exceeds the Width property
      /// </summary>
      [ConfigurationProperty("wrap")]
      public Boolean? Wrap
      {
         get { return (Boolean?)base["wrap"]; }
      }
      /// <summary>
      /// Specifies the width of each line of formatted text
      /// </summary>
      [ConfigurationProperty("width")]
      public Int32? Width
      {
         get { return (Int32?)base["width"]; }
      }
      /// <summary>
      /// Specifies the number of characters to indent log messages
      /// </summary>
      [ConfigurationProperty("indent")]
      public Int32? Indent
      {
         get { return (Int32?)base["indent"]; }
      }
      /// <summary>
      /// Specifies the number of characters to hanging indent log messages
      /// </summary>
      [ConfigurationProperty("hang")]
      public Int32? Hang
      {
         get { return (Int32?)base["hang"]; }
      }
      #endregion
   }
}
