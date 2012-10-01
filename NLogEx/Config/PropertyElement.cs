//===========================================================================
// MODULE:  PropertyElement.cs
// PURPOSE: generic property (name/value) configuration
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
   /// Property configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a generic name/value pair,
   /// serialized as a single element with a name attribute,
   /// and whose content is the property value.
   /// </remarks>
   /// <example>
   ///   <myproperties>
   ///      <clear/>
   ///      <add name="myprop1">myvalue1</add>
   ///      <add name="myprop2">myvalue2</add>
   ///      <remove name="myprop1"/>
   ///   </myproperties>
   /// </example>
   internal sealed class PropertyElement : ConfigurationElement
   {
      #region Configuration Properties
      /// <summary>
      /// Configuration property name
      /// </summary>
      public String Name { get; private set; }
      /// <summary>
      /// Configuration property value
      /// </summary>
      public String Value { get; private set; }
      #endregion

      #region ConfigurationElement Overrides
      /// <summary>
      /// Manually reads the element from the configuration XML
      /// </summary>
      /// <param name="reader">
      /// The XML reader for the current element
      /// </param>
      /// <param name="serializeCollectionKey">
      /// True to serialize only the collection key properties
      /// False otherwise
      /// </param>
      protected override void DeserializeElement (
         System.Xml.XmlReader reader, 
         Boolean serializeCollectionKey)
      {
         this.Name = (String)reader.GetAttribute("name");
         this.Value = reader.ReadElementContentAsString();
         if (String.IsNullOrWhiteSpace(this.Name))
            throw new ConfigException(this, "Name");
      }
      #endregion

      /// <summary>
      /// The property configuration collection
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
            return ((PropertyElement)element).Name;
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
