//===========================================================================
// MODULE:  InstanceElement.cs
// PURPOSE: log instance configuration
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
   /// Log instance configuration element
   /// </summary>
   /// <remarks>
   /// This class represents an <add/> element within
   /// the log application configuration section, within 
   /// the <instances/> element. It is used to construct 
   /// a runtime log instance.
   /// </remarks>
   /// <example>
   ///   <NlogEx>
   ///      <instances>
   ///         <add buffer="10" synchronous="true">
   ///            <properties>
   ///               <add>Event.Type</add>
   ///               <add>Custom.Property</add>
   ///            </properties>
   ///            <filter>...</filter>
   ///            <logger type="..."/>
   ///         </add>
   ///         ...
   ///      </instances>
   ///   </NlogEx>
   /// </example>
   internal sealed class InstanceElement : ConfigurationElement
   {
      #region Configuration Properties
      [ConfigurationProperty("properties", IsRequired = true)]
      private ValueElement.Collection ConfigProperties
      {
         get { return (ValueElement.Collection)base["properties"]; }
      }
      /// <summary>
      /// The logger to configure for this instance
      /// </summary>
      [ConfigurationProperty("logger", IsRequired = true)]
      public LoggerElement Logger
      {
         get { return (LoggerElement)base["logger"]; }
      }
      /// <summary>
      /// The filter to apply to log events
      /// </summary>
      [ConfigurationProperty("filter")]
      public FilterElement Filter
      {
         get { return (FilterElement)base["filter"]; }
      }
      /// <summary>
      /// The maximum number of events to buffer
      /// before dispatching them to the logger
      /// </summary>
      [ConfigurationProperty("buffer")]
      public Int32 Buffer
      {
         get { return (Int32)base["buffer"]; }
      }
      /// <summary>
      /// Specifies whether events are delivered to the logger
      /// synchronously on the Log() call, or on a separate thread
      /// </summary>
      [ConfigurationProperty("synchronous")]
      public Boolean Synchronous
      {
         get { return (Boolean)base["synchronous"]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The list of event properties to include in
      /// log events for this instance
      /// </summary>
      public new List<String> Properties
      {
         get
         {
            return ConfigProperties
               .Cast<ValueElement>()
               .Select(p => p.Value)
               .ToList();
         }
      }
      #endregion

      /// <summary>
      /// The instance configuration collection
      /// </summary>
      public sealed class Collection : ConfigurationElementCollection
      {
         #region ConfigurationElementCollection Overrides
         /// <summary>
         /// Retrieves a key value for the collection
         /// </summary>
         /// <param name="element">
         /// The configuration collection element
         /// </param>
         /// <returns>
         /// The collection element's key
         /// </returns>
         protected override Object GetElementKey (ConfigurationElement element)
         {
            return new Object();
         }
         /// <summary>
         /// Creates a new typed collection element
         /// </summary>
         /// <returns>
         /// The new element
         /// </returns>
         protected override ConfigurationElement CreateNewElement ()
         {
            return new InstanceElement();
         }
         #endregion
      }
   }
}
