//===========================================================================
// MODULE:  ConfigException.cs
// PURPOSE: log configuration exception
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
using System.Linq;
using System.Runtime.Serialization;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log configuration exception
   /// </summary>
   /// <remarks>
   /// This exception represents a failure to configure
   /// a log instance or logger property.
   /// </remarks>
   [Serializable]
   public sealed class ConfigException : Exception
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new exception instance
      /// </summary>
      public ConfigException ()
         : base("Invalid log configuration")
      {
      }
      /// <summary>
      /// Initializes a new exception instance
      /// </summary>
      /// <param name="config">
      /// The object being configured
      /// </param>
      /// <param name="property">
      /// The name of the property that failed
      /// </param>
      /// <param name="inner">
      /// The inner exception object
      /// </param>
      public ConfigException (Object config, String property, Exception inner = null)
         : base(
            String.Format(
               "Invalid log configuration: {0}.{1}", 
               config.GetType().FullName, 
               property
            ), 
            inner
         )
      {
      }
      /// <summary>
      /// Initializes a new exception instance
      /// </summary>
      /// <param name="message">
      /// The exception message
      /// </param>
      /// <param name="inner">
      /// The inner exception
      /// </param>
      public ConfigException (String message, Exception inner = null)
         : base(message, inner) 
      {
      }
      /// <summary>
      /// Initializes a new exception instance
      /// </summary>
      /// <param name="info">
      /// The serialized exception state
      /// </param>
      /// <param name="context">
      /// The current serialization context
      /// </param>
      public ConfigException (SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }
      #endregion
   }
}
