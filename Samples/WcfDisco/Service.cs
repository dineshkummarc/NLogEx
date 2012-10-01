//===========================================================================
// MODULE:  Service.cs
// PURPOSE: WCF discovery logging sample service contract and implementation
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
using System.ServiceModel;
// Project References

namespace NLogEx.WcfDisco
{
   /// <summary>
   /// The service contract interface
   /// </summary>
   [ServiceContract(
      Namespace = "http://brentspell.us/Projects/NLogEx/Samples/WcfDisco/")
   ]
   public interface IService
   {
      /// <summary>
      /// Sample service operation
      /// </summary>
      [OperationContract]
      void Execute ();
   }

   /// <summary>
   /// The sample service implementation
   /// </summary>
   public sealed class Service : IService
   {
      /// <summary>
      /// Sample service operation
      /// </summary>
      public void Execute()
      {
      }
   }
}
