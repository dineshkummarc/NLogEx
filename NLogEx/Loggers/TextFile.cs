//===========================================================================
// MODULE:  TextFile.cs
// PURPOSE: rotating text file logger
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
using System.IO;
using System.Linq;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The text file logger
   /// </summary>
   /// <remarks>
   /// This class writes line-formatted events
   /// to a text file, optionally rotating the
   /// text file so it does not exceed a configured
   /// capacity.
   /// </remarks>
   public sealed class TextFile : ILogger
   {
      #region Configuration Properties
      /// <summary>
      /// Log file name/path
      /// </summary>
      public String FileName { get; set; }
      /// <summary>
      /// Optional maximum log file capacity
      /// </summary>
      public Int64 Capacity { get; set; }
      /// <summary>
      /// Event message formatter
      /// </summary>
      public Formatter Formatter { get; set; }
      #endregion

      #region ILogger Implementation
      /// <summary>
      /// Logger initialization
      /// </summary>
      /// <param name="properties">
      /// Log event properties
      /// </param>
      public void Initialize (IEnumerable<String> properties)
      {
         // validate the logger
         if (String.IsNullOrWhiteSpace(this.FileName))
            throw new ConfigException(this, "FileName");
         if (this.Capacity < 0)
            throw new ConfigException(this, "Capacity");
         if (this.Formatter == null)
            throw new ConfigException(this, "Formatter");
         // ensure the log file is writable
         Open().Close();
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         lock (this)
         {
            FileStream stream = Open();
            try
            {
               // if we have reached capacity, rotate the log file
               if (this.Capacity > 0 && stream.Length > this.Capacity)
               {
                  stream.Close();
                  Rotate();
                  stream = Open();
               }
               // write all events to the log
               using (StreamWriter writer = new StreamWriter(stream))
                  writer.Write(this.Formatter.FormatEventListString(events));
            }
            finally
            {
               stream.Close();
            }
         }
      }
      #endregion

      #region File Utilities
      /// <summary>
      /// Opens the text log file
      /// </summary>
      /// <returns>
      /// The text log file stream
      /// </returns>
      private FileStream Open ()
      {
         return new FileStream(this.FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
      }
      /// <summary>
      /// Rotates the text log file, renaming it as
      /// {file}.old{ext}
      /// </summary>
      private void Rotate ()
      {
         try
         {
            String oldName = Path.Combine(
               Path.GetDirectoryName(this.FileName),
               String.Format(
                  "{0}.old{1}", 
                  Path.GetFileNameWithoutExtension(this.FileName),
                  Path.GetExtension(this.FileName)
               )
            );
            File.Delete(oldName);
            File.Move(this.FileName, oldName);
         }
         catch { }
      }
      #endregion
   }
}
