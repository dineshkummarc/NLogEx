//===========================================================================
// MODULE:  Converter.cs
// PURPOSE: comparer type converter
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
using System.ComponentModel;
// Project References

namespace NLogEx.Comparers
{
   /// <summary>
   /// Type converter for comparer classes
   /// </summary>
   internal struct Converter
   {
      private TypeConverter converter;

      /// <summary>
      /// Converts a source value to a target type
      /// </summary>
      /// <param name="target">
      /// The target object whose type is to be used in the conversion
      /// </param>
      /// <param name="source">
      /// The source value to convert
      /// </param>
      /// <returns>
      /// The converted value
      /// </returns>
      public Object Convert (Object target, Object source)
      {
         return Convert(target.GetType(), source);
      }
      /// <summary>
      /// Converts a source value to a target type
      /// </summary>
      /// <param name="targetType">
      /// The type to use in the conversion
      /// </param>
      /// <param name="source">
      /// The source value to convert
      /// </param>
      /// <returns>
      /// The converted value
      /// </returns>
      public Object Convert (Type targetType, Object source)
      {
         // automatically convert to string, as StringConverter is useless
         if (targetType == typeof(String))
            return System.Convert.ToString(source);
         // perform identity conversion
         Type sourceType = source.GetType();
         if (sourceType == targetType)
            return source;
         // convert using type converter
         if (this.converter == null)
            this.converter = TypeDescriptor.GetConverter(targetType);
         return this.converter.ConvertFrom(source);
      }
   }
}
