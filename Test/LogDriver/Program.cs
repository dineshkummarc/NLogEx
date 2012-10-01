//===========================================================================
// MODULE:  Program.cs
// PURPOSE: log configuration test client driver program
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

namespace NLogEx.LogDriver
{
   /// <summary>
   /// The log configuration test program
   /// </summary>
   /// <remarks>
   /// This program raises several log events that are dispatched
   /// to the log instances registered in app.config.
   /// </remarks>
   static class Program
   {
      static Log Log = new Log(typeof(Program));

      /// <summary>
      /// Program entry point
      /// </summary>
      static void Main ()
      {
         Console.WriteLine("NLogEx Logger Test");
         Console.WriteLine("   Press escape to exit.");
         // register framework log contexts
         Context.Environment.Register(new[] { "INCLUDE" });
         Context.Process.Register();
         Context.Thread.Register();
         Context.Transaction.Register();
         // log test events
         Log.Trace("Trace Message");
         Log.Info("Info Message");
         Log.Warn(new Exception("Warning Message"));
         Log.Error(new Exception("Exception Message"), "Error Message");
         // log periodic events
         new System.Timers.Timer()
         {
            Enabled = true,
            AutoReset = true,
            Interval = 5000
         }.Elapsed += (o, a) => Log.Info("Timer elapsed");
         // wait until the user terminates
         for ( ; ; )
            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
               break;
      }
   }
}
