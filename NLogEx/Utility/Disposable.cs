//===========================================================================
// MODULE:  Disposable.cs
// PURPOSE: disposable action utility
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
// Project References

namespace NLogEx
{
   /// <summary>
   /// Disposable action
   /// </summary>
   /// <remarks>
   /// This class represents a callback used to
   /// implement the IDisposable interface. This
   /// is useful for implementing statless completion 
   /// and de-registration tokens and leases without 
   /// requiring a dedicated class.
   /// </remarks>
   public struct Disposable : IDisposable
   {
      private Action dispose;
      /// <summary>
      /// Initializes a new disposable instance
      /// </summary>
      /// <param name="disposer">
      /// The disposer delegate to call
      /// </param>
      public Disposable (Action disposer)
      {
         this.dispose = disposer;
      }
      /// <summary>
      /// Calls the disposer delegate
      /// </summary>
      public void Dispose ()
      {
         if (this.dispose != null)
            this.dispose();
         this.dispose = null;
      }
   }
}
