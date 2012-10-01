//===========================================================================
// MODULE:  TypeLoader.cs
// PURPOSE: log type loader utility
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
using System.Reflection;
// Project References

namespace NLogEx
{
   /// <summary>
   /// Logger custom type loader
   /// </summary>
   /// <remarks>
   /// This class centralizes the type loading policy for the logging
   /// framework, ensuring that types that are loaded dynamically
   /// are done so consistently.
   /// </remarks>
   internal static class TypeLoader
   {
      /// <summary>
      /// Attempts to load a type by name
      /// </summary>
      /// <remarks>
      /// This method consults the entry point assembly first, which is
      /// the most likely location for custom logging types.
      /// </remarks>
      /// <param name="name">
      /// The type name, optionally assembly-qualified
      /// </param>
      /// <param name="type">
      /// Return the loaded type via here
      /// </param>
      /// <returns>
      /// True if the type was loaded successfully
      /// False otherwise
      /// </returns>
      public static Boolean TryLoad (String name, out Type type)
      {
         type = null;
         // first, attempt to load the type from the entry point
         Assembly entry = Assembly.GetEntryAssembly();
         if (entry != null)
            type = entry.GetType(name, false);
         // finally, fall back on the generic type loader
         if (type == null)
            type = Type.GetType(name, false);
         return type != null;
      }
      /// <summary>
      /// Loads a type by name
      /// </summary>
      /// <remarks>
      /// This method consults the entry point assembly first, which is
      /// the most likely location for custom logging types.
      /// </remarks>
      /// <param name="name">
      /// The type name, optionally assembly-qualified
      /// </param>
      /// <returns>
      /// The loaded type
      /// </returns>
      public static Type Load (String name)
      {
         Type type = null;
         if (!TryLoad(name, out type))
            throw new TypeLoadException(
               String.Format("Failed to load type {0}", name)
            );
         return type;
      }
      /// <summary>
      /// Creates an instance of a named type
      /// </summary>
      /// <param name="name">
      /// The type name, optionally assembly-qualified
      /// </param>
      /// <typeparam name="T">
      /// Cast the result to this type
      /// </typeparam>
      /// <returns>
      /// The new object instance
      /// </returns>
      public static T CreateInstance<T> (String name)
      {
         return (T)Activator.CreateInstance(Load(name));
      }
   }
}
