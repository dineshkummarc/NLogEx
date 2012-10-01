//===========================================================================
// MODULE:  Smtp.cs
// PURPOSE: SMTP mail message logger
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
using System.Net.Mail;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The SMTP mail logger
   /// </summary>
   /// <remarks>
   /// This class sends an SMTP message to a
   /// configured mailbox for each batch of
   /// events received.
   /// </remarks>
   public sealed class Smtp : ILogger
   {
      #region Configuration Properties
      /// <summary>
      /// Mail from address
      /// </summary>
      public String From { get; set; }
      /// <summary>
      /// Mail to address
      /// </summary>
      public String To { get; set; }
      /// <summary>
      /// Mail message subject
      /// </summary>
      public String Subject { get; set; }
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
         if (String.IsNullOrWhiteSpace(this.From))
            throw new ConfigException(this, "From");
         if (String.IsNullOrWhiteSpace(this.To))
            throw new ConfigException(this, "To");
         if (String.IsNullOrWhiteSpace(this.Subject))
            throw new ConfigException(this, "Subject");
         if (this.Formatter == null)
            throw new ConfigException(this, "Formatter");
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         new SmtpClient().Send(
            new MailMessage(this.From, this.To)
            {
               Subject = this.Subject,
               Body = this.Formatter.FormatEventListString(events.Distinct())
            }
         );
      }
      #endregion
   }
}
