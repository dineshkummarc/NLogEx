//===========================================================================
// MODULE:  IsSubclassOf.cs
// PURPOSE: subclass log filter property comparer
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
// Project References

namespace NLogEx.Comparers
{
   /// <summary>
   /// Subclass property comparer
   /// </summary>
   /// <remarks>
   /// This comparer matches an event property (x) to a filter property (y)
   /// if the type x is a subclass of the type name specified in y.
   /// </remarks>
   /// <example>
   /// new System.IO.MemoryStream() issubclassof "System.IO.Stream" => true
   /// 42 issubclassof "System.String" => false
   /// </example>
   public sealed class IsSubclassOf : IEqualityComparer
   {
      Type superType = null;

      #region IEqualityComparer Implementation
      /// <summary>
      /// Compares two properties for equality
      /// </summary>
      /// <param name="x">
      /// The event property value
      /// </param>
      /// <param name="y">
      /// The filter property value
      /// </param>
      /// <returns>
      /// True if the properties match
      /// False otherwise
      /// </returns>
      public new Boolean Equals (Object x, Object y)
      {
         // convert the LHS/RHS to type objects
         Type xType = x as Type ?? x.GetType();
         if (this.superType == null)
            this.superType = y as Type ?? TypeLoader.Load(Convert.ToString(y));
         // determine the type relationship
         return (xType == this.superType || xType.IsSubclassOf(this.superType));
      }
      /// <summary>
      /// Calculates an event property hash code
      /// </summary>
      /// <param name="obj">
      /// The event property value
      /// </param>
      /// <returns>
      /// The hash code for the property value
      /// </returns>
      public Int32 GetHashCode (Object obj)
      {
         return obj.GetHashCode();
      }
      #endregion
   }
}
