//===========================================================================
// MODULE:  QueryElement.cs
// PURPOSE: WMI query configuration
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
using System.Configuration;
// Project References

namespace NLogEx.Samples.WmiEventLog
{
   /// <summary>
   /// WMI query configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a WMI query to use to subscribe to a WMI
   /// event.
   /// </remarks>
   internal sealed class QueryElement : ConfigurationElement
   {
      #region Configuration Properties
      /// <summary>
      /// The scope of the query
      /// </summary>
      [ConfigurationProperty("scope", DefaultValue = @"root\WMI")]
      public String Scope
      {
         get { return (String)base["scope"]; }
      }
      /// <summary>
      /// The query string
      /// </summary>
      [ConfigurationProperty("query", IsRequired = true)]
      public String Query
      {
         get { return (String)base["query"]; }
      }
      #endregion

      /// <summary>
      /// The package configuration collection
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
            return ((QueryElement)element).Query;
         }
         /// <summary>
         /// Creates a new typed collection element
         /// </summary>
         /// <returns>
         /// The new element
         /// </returns>
         protected override ConfigurationElement CreateNewElement ()
         {
            return new QueryElement();
         }
         #endregion
      }
   }
}
