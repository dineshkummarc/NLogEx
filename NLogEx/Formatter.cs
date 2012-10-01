//===========================================================================
// MODULE:  Formatter.cs
// PURPOSE: log text formatter
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
using System.Text;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log text formatter
   /// </summary>
   /// <remarks>
   /// This class provides text formatting support
   /// (indentation, wrapping, etc) common to many 
   /// log representations.
   /// </remarks>
   public sealed class Formatter
   {
      #region Formatter Configuration
      /// <summary>
      /// The String.Format-style log event format 
      /// string, with the placeholders representing
      /// the ordered event properties
      /// </summary>
      public String Format { get; set; }
      /// <summary>
      /// Specifies whether to wrap words at the
      /// line width boundary
      /// </summary>
      public Boolean Wrap { get; set; }
      /// <summary>
      /// The maximum line width, for wrapped text
      /// </summary>
      public Int32 Width { get; set; }
      /// <summary>
      /// The line indent level
      /// </summary>
      public Int32 Indent { get; set; }
      /// <summary>
      /// The line hanging indent level
      /// </summary>
      public Int32 Hang { get; set; }
      #endregion

      #region Formatter Operations
      /// <summary>
      /// Validates the formatter's configuration properties
      /// </summary>
      public void Validate ()
      {
         if (this.Indent < 0)
            throw new ConfigException(this, "Indent");
         if (this.Hang < 0)
            throw new ConfigException(this, "Indent");
         if (!this.Wrap && this.Width != 0)
            throw new ConfigException(this, "Width");
         if (this.Width < 0)
            throw new ConfigException(this, "Width");
         if (this.Width > 0 && this.Width <= this.Indent + this.Hang)
            throw new ConfigException(this, "Width");
      }
      /// <summary>
      /// Formats a list of log events
      /// </summary>
      /// <param name="events">
      /// The list of events to format
      /// </param>
      /// <returns>
      /// The list of formatted text lines
      /// </returns>
      public IEnumerable<String> FormatEventList (IEnumerable<Event> events)
      {
         foreach (Event evt in events)
            foreach (String line in FormatEvent(evt))
               yield return line;
      }
      /// <summary>
      /// Formats a list of log events
      /// </summary>
      /// <param name="events">
      /// The list of events to format
      /// </param>
      /// <returns>
      /// The formatted event text
      /// </returns>
      public String FormatEventListString (IEnumerable<Event> events)
      {
         if (events == null)
            return null;
         StringBuilder str = new StringBuilder();
         foreach (String line in FormatEventList(events))
            str.AppendLine(line);
         return str.ToString();
      }
      /// <summary>
      /// Formats a log event
      /// </summary>
      /// <param name="evt">
      /// The log event to format
      /// </param>
      /// <returns>
      /// The list of formatted text lines
      /// </returns>
      public IEnumerable<String> FormatEvent (Event evt)
      {
         // construct the format string, if not specified
         String format = this.Format;
         if (String.IsNullOrWhiteSpace(format))
            if (evt.Properties.Count == 1)
               format = "{0}";
            else
               format = String.Join("\r\n", evt.Names.Select((p, i) => String.Format("{0}: {{{1}}}", p, i)));
         return FormatMessage(String.Format(format, evt.Values.ToArray()));
      }
      /// <summary>
      /// Formats a log event
      /// </summary>
      /// <param name="evt">
      /// The log event to format
      /// </param>
      /// <returns>
      /// The formatted event text
      /// </returns>
      public String FormatEventString (Event evt)
      {
         StringBuilder str = new StringBuilder();
         foreach (String line in FormatEvent(evt))
            str.AppendLine(line);
         return str.ToString();
      }
      /// <summary>
      /// Formats a string message
      /// </summary>
      /// <param name="message">
      /// The message to format
      /// </param>
      /// <returns>
      /// The list of formatted text lines
      /// </returns>
      public IEnumerable<String> FormatMessage (String message)
      {
         // split the formatted string into its consituent lines
         // if wrapping, split the lines at the console buffer boundary
         // prefix each line with the configured indent
         Int32 lineNumber = 0;
         if (message != null)
            foreach (String line in message.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None))
               foreach (String segment in FormatLine(line, lineNumber++))
                  yield return segment;
      }
      /// <summary>
      /// Formats a string message
      /// </summary>
      /// <param name="message">
      /// The message to format
      /// </param>
      /// <returns>
      /// The formatted message text
      /// </returns>
      public String FormatMessageString (String message)
      {
         if (message == null)
            return null;
         StringBuilder str = new StringBuilder();
         foreach (String line in FormatMessage(message))
            str.AppendLine(line);
         return str.ToString();
      }
      /// <summary>
      /// Formats a single text line
      /// </summary>
      /// <param name="line">
      /// The line to format
      /// </param>
      /// <param name="lineNumber">
      /// The current line number
      /// </param>
      /// <returns>
      /// The list of formatted text lines
      /// </returns>
      public IEnumerable<String> FormatLine (String line, Int32 lineNumber)
      {
         // validate the dynamic page width
         if (this.Wrap && this.Width == 0)
            throw new ConfigException(this, "Width");
         if (this.Wrap && this.Width <= this.Indent + this.Hang)
            throw new ConfigException(this, "Width");
         Int32 maxWidth = (this.Wrap) ? this.Width - this.Indent - this.Hang : Int32.MaxValue;
         Int32 curWidth = 0;
         Int32 curIndex = 0;
         if (line != null)
         {
            while (curIndex < line.Length)
            {
               // if the current segment fits within the wrap
               // width, include the entire segment
               if (maxWidth >= line.Length - curIndex)
                  curWidth = line.Length - curIndex;
               else
               {
                  // otherwise, find the last space in the segment
                  // and truncate the segment at that point
                  curWidth = maxWidth;
                  while (curWidth > 0 && !Char.IsWhiteSpace(line[curIndex + curWidth]))
                     curWidth--;
                  // if no whitespace was found, truncate the segment
                  // at the end of the line
                  if (curWidth == 0)
                     curWidth = maxWidth;
               }
               // indent and return the current segment
               String segment = line.Substring(curIndex, curWidth);
               yield return FormatSegmentString(segment, lineNumber);
               // set the hanging indent for following lines
               if (this.Wrap && lineNumber == 0)
                  maxWidth -= this.Hang;
               // skip any space on the next wrapped segment
               curIndex += curWidth;
               while (curIndex < line.Length && Char.IsWhiteSpace(line[curIndex]))
                  curIndex++;
            }
            if (curIndex == 0)
               yield return FormatSegmentString(line, lineNumber);
         }
      }
      /// <summary>
      /// Formats a single text line
      /// </summary>
      /// <param name="line">
      /// The line to format
      /// </param>
      /// <param name="lineNumber">
      /// The current line number
      /// </param>
      /// <returns>
      /// The formatted line text
      /// </returns>
      public String FormatLineString (String line, Int32 lineNumber)
      {
         if (line == null)
            return null;
         StringBuilder str = new StringBuilder();
         foreach (String seg in FormatLine(line, lineNumber))
            str.AppendLine(seg);
         return str.ToString();
      }
      /// <summary>
      /// Formats a single (possibly wrapped) text line
      /// </summary>
      /// <param name="segment">
      /// The segment to format
      /// </param>
      /// <param name="lineNumber">
      /// The current line number
      /// </param>
      /// <returns>
      /// The formatted line text
      /// </returns>
      public String FormatSegmentString (String segment, Int32 lineNumber)
      {
         // indent and return the segment
         Int32 indent = (lineNumber == 0) ? this.Indent : this.Indent + this.Hang;
         return (segment != null) ? new String(' ', indent) + segment : null;
      }
      #endregion
   }
}
