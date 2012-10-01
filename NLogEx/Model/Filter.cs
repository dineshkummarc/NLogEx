//===========================================================================
// MODULE:  Filter.cs
// PURPOSE: log event filter
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
using System.Linq;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log filter
   /// </summary>
   /// <remarks>
   /// This class represents a simple mechanism for
   /// filtering the list of events that a given log
   /// instance will receive. Filters may specify a
   /// blacklist and whitelist of event property 
   /// name/value pairs.
   /// </remarks>
   public sealed class Filter
   {
      #region Properties
      /// <summary>
      /// The event property whitelist
      /// </summary>
      public IList<Property> IncludeProps { get; set; }
      /// <summary>
      /// The event property blacklist
      /// </summary>
      public IList<Property> ExcludeProps { get; set; }
      #endregion

      #region Operations
      /// <summary>
      /// Validates the filter instance
      /// </summary>
      public void Validate ()
      {
         this.IncludeProps = this.IncludeProps ?? new Property[0];
         this.ExcludeProps = this.ExcludeProps ?? new Property[0];
         if (this.IncludeProps.Any(p => p == null || String.IsNullOrWhiteSpace(p.Name)))
            throw new ArgumentException("IncludeProps");
         if (this.ExcludeProps.Any(p => p == null || String.IsNullOrWhiteSpace(p.Name)))
            throw new ArgumentException("ExcludeProps");
      }
      /// <summary>
      /// Evaluates a filter against an event context
      /// </summary>
      /// <param name="context">
      /// The event context to query
      /// </param>
      /// <returns>
      /// True if the event context matches the filter
      /// False otherwise
      /// </returns>
      internal Boolean Evaluate (EventContext context)
      {
         // evaluate property value inclusion/exclusion
         if (this.ExcludeProps.Any(p => Match(context, p)))
            return false;
         if (this.IncludeProps.Any())
            if (!this.IncludeProps.Any(p => Match(context, p)))
               return false;
         return true;
      }
      /// <summary>
      /// Matches an event context property to a property
      /// </summary>
      /// <param name="context">
      /// The event context containing the property to compare
      /// </param>
      /// <param name="prop">
      /// The property to match
      /// </param>
      /// <returns>
      /// True if the event context matches the property
      /// False otherwise
      /// </returns>
      private Boolean Match (EventContext context, Property prop)
      {
         Object eValue = context.GetProperty(prop.Name);
         Object pValue = prop.Value;
         if (eValue == null && pValue == null)
            return true;
         if (eValue == null || pValue == null)
            return false;
         try { return (prop.Except != prop.Comparer.Equals(eValue, pValue)); }
         catch { return false; }
      }
      #endregion

      /// <summary>
      /// The filter name/value property
      /// </summary>
      public sealed class Property
      {
         #region Construction/Disposal
         /// <summary>
         /// Initializes a new property instance
         /// </summary>
         /// <param name="name">
         /// Property name
         /// </param>
         /// <param name="value">
         /// Property value
         /// </param>
         /// <param name="comparer">
         /// Property comparer
         /// </param>
         /// <param name="except">
         /// Invert the comparer's result?
         /// </param>
         public Property (
            String name = null, 
            Object value = null,
            IEqualityComparer comparer = null,
            Boolean except = false)
         {
            this.Name = name;
            this.Value = value;
            this.Comparer = comparer ?? new Comparers.Default();
            this.Except = except;
         }
         /// <summary>
         /// Initializes a new property instance
         /// </summary>
         /// <param name="prop">
         /// The existing property object
         /// </param>
         /// <param name="value">
         /// Property comparer
         /// </param>
         /// <param name="comparer">
         /// Property comparer
         /// </param>
         /// <param name="except">
         /// Invert the comparer's result?
         /// </param>
         public Property (
            KeyValuePair<String, Object> prop,
            IEqualityComparer comparer = null,
            Boolean except = false)
            : this(prop.Key, prop.Value, comparer, except)
         {
         }
         /// <summary>
         /// Initializes a new property instance
         /// </summary>
         /// <param name="prop">
         /// The existing property object
         /// </param>
         /// <param name="value">
         /// Property comparer
         /// </param>
         /// <param name="comparer">
         /// Property comparer
         /// </param>
         /// <param name="except">
         /// Invert the comparer's result?
         /// </param>
         public Property (
            KeyValuePair<String, String> prop,
            IEqualityComparer comparer = null,
            Boolean except = false)
            : this(prop.Key, prop.Value, comparer, except)
         {
         }
         #endregion

         #region Properties
         /// <summary>
         /// The property name
         /// </summary>
         public String Name { get; private set; }
         /// <summary>
         /// The property value
         /// </summary>
         public Object Value { get; private set; }
         /// <summary>
         /// The property equality comparer
         /// </summary>
         public IEqualityComparer Comparer { get; private set; }
         /// <summary>
         /// Specifies whether to invert the comparer's result
         /// </summary>
         public Boolean Except { get; private set; }
         #endregion
      }
   }
}
