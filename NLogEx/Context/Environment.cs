//===========================================================================
// MODULE:  Environment.cs
// PURPOSE: application environment logging context
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
// Project References

namespace NLogEx.Context
{
   /// <summary>
   /// The environment logging context
   /// </summary>
   /// <remarks>
   /// This class provides support for retrieving
   /// common application environment properties
   /// and environment variables during event logging.
   /// </remarks>
   public static class Environment
   {
      /// <summary>
      /// Establishes the environment context with the logging subsystem
      /// </summary>
      /// <param name="variables">
      /// The list of environment variables to include in the context
      /// </param>
      public static void Register (IEnumerable<String> variables = null)
      {
         // create generic environment context properties
         Dictionary<String, Func<Object>> ctx = new Dictionary<String, Func<Object>>()
         {
            { "MachineName", () => System.Environment.MachineName },
            { "UserName", () => System.Environment.UserName },
            { "UserDomainName", () => System.Environment.UserDomainName },
            { "CurrentDirectory", () => System.Environment.CurrentDirectory },
            { "OSVersion", () => System.Environment.OSVersion },
            { "ProcessorCount", () => System.Environment.ProcessorCount }
         };
         // add requested environment variables
         if (variables != null)
            foreach (String variable in variables)
               ctx.Add(variable, () => System.Environment.GetEnvironmentVariable(variable));
         // establish the context
         Log.RegisterContext("Environment", ctx);
      }
   }
}
