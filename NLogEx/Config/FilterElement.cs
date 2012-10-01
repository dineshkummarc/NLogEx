//===========================================================================
// MODULE:  FilterElement.cs
// PURPOSE: log filter configuration
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
   /// Log filter configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a <filter/> element within
   /// the log application configuration section. It
   /// is used to construct a runtime filter instance.
   /// </remarks>
   /// <example>
   ///   <NlogEx>
   ///     <instances>
   ///        <add>
   ///           <properties>...</properties>
   ///           <filter>
   ///              <include 
   ///                 except="true"
   ///                 name="Event.Type" 
   ///                 value="Error"/>
   ///              <exclude 
   ///                 name="Event.Source" 
   ///                 comparer="NLogEx.Comparers.IsSubclassOf,NLogEx"
   ///                 value="MyNamespace.MyClass,MyAssembly"/>
   ///              ...
   ///           </filter>
   ///           <logger type="..."/>
   ///        </add>
   ///        ...
   ///     </instances>
   ///   </NlogEx>
   /// </example>
   internal sealed class FilterElement : ConfigurationElement
   {
      #region Configuration Properties
      [ConfigurationProperty("", IsDefaultCollection = true)]
      [ConfigurationCollection(
         typeof(PropertyElement.Collection),
         AddItemName = "include",
         RemoveItemName = "exclude")
      ]
      private PropertyElement.Collection ConfigProperties
      {
         get { return (PropertyElement.Collection)base[""]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The list of including property filters
      /// </summary>
      public List<PropertyElement> IncludeProperties
      {
         get
         {
            return ConfigProperties.Cast<PropertyElement>().ToList();
         }
      }
      /// <summary>
      /// The list of excluding property filters
      /// </summary>
      public List<PropertyElement> ExcludeProperties
      {
         get
         {
            return ConfigProperties.Removed.Cast<PropertyElement>().ToList();
         }
      }
      #endregion

      /// <summary>
      /// Filter property configuration element
      /// </summary>
      /// <remarks>
      /// This class represents the configuration element for a 
      /// filter property, including its name, value, comparer, and except
      /// flag.
      /// </remarks>
      internal sealed class PropertyElement : ConfigurationElement
      {
         #region Configuration Properties
         /// <summary>
         /// Name of the event property to filter
         /// </summary>
         [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
         public String Name
         {
            get { return (String)base["name"]; }
         }
         /// <summary>
         /// Property filter value
         /// </summary>
         [ConfigurationProperty("value", IsRequired = false, IsKey = true)]
         public String Value
         {
            get { return (String)base[new ConfigurationProperty("value", typeof(String), null)]; }
         }
         /// <summary>
         /// Filter comparer type name
         /// </summary>
         [ConfigurationProperty(
            "comparer", 
            DefaultValue = "NLogEx.Comparers.Default,NLogEx",
            IsKey = true)
         ]
         public String Comparer
         {
            get { return (String)base["comparer"]; }
         }
         /// <summary>
         /// Specifies whether to invert the comparsion result
         /// </summary>
         [ConfigurationProperty("except", IsKey = true)]
         public Boolean Except
         {
            get { return (Boolean)base["except"]; }
         }
         #endregion

         /// <summary>
         /// The filter property configuration collection
         /// </summary>
         /// <remarks>
         /// This class tracks any <remove/> elements in the
         /// Removed collection, to support blacklisting and
         /// whitelisting in the configuration collection.
         /// </remarks>
         public sealed class Collection : ConfigurationElementCollection
         {
            private List<PropertyElement> removed = new List<PropertyElement>();

            public List<PropertyElement> Removed { get { return this.removed; } }

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
               return ((PropertyElement)element).Name + ((PropertyElement)element).Value;
            }
            /// <summary>
            /// Creates a new typed collection element
            /// </summary>
            /// <returns>
            /// The new element
            /// </returns>
            protected override ConfigurationElement CreateNewElement ()
            {
               return new PropertyElement();
            }
            /// <summary>
            /// Collection delete callback
            /// </summary>
            /// <param name="element">
            /// The element to delete
            /// </param>
            /// <returns>
            /// True to delete the element
            /// False otherwise
            /// </returns>
            protected override Boolean IsElementRemovable (ConfigurationElement element)
            {
               if (base.IsElementRemovable(element))
               {
                  removed.Add((PropertyElement)element);
                  return true;
               }
               return false;
            }
            #endregion
         }
      }
   }
}
