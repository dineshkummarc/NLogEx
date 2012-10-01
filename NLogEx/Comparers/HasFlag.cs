//===========================================================================
// MODULE:  HasFlag.cs
// PURPOSE: flags enum log filter property comparer
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
using System.ComponentModel;
// Project References

namespace NLogEx.Comparers
{
   /// <summary>
   /// Flags enum property comparer
   /// </summary>
   /// <remarks>
   /// This comparer tests the [Flags] enumeration event property value (x)
   /// against a single flag value (y), returning true if the enum contains
   /// the specified flag.
   /// </remarks>
   /// <example>
   /// System.IO.FileAccess.ReadWrite hasflag System.IO.FileAccess.Read => true
   /// System.IO.FileAccess.Write hasflag System.IO.FileAccess.Read => false
   /// </example>
   public sealed class HasFlag : IEqualityComparer
   {
      private Converter converter;

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
         return ((Enum)x).HasFlag((Enum)this.converter.Convert(x, y));
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
