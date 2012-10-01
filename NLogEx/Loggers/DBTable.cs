//===========================================================================
// MODULE:  DBTable.cs
// PURPOSE: ADO.NET database table logger
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
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
// Project References

namespace NLogEx.Loggers
{
   /// <summary>
   /// The database table logger
   /// </summary>
   /// <remarks>
   /// This class serializes log events to a
   /// configured database table. 
   /// </remarks>
   public sealed class DBTable : ILogger
   {
      private String insertCommand;

      #region Logger Configuration
      /// <summary>
      /// The name of the ADO.NET provider to connect
      /// </summary>
      public String Provider { get; set; }
      /// <summary>
      /// The name of the connection string to load from
      /// App.Config for the log database
      /// </summary>
      public String ConnectionName { get; set; }
      /// <summary>
      /// The connection string for the log database
      /// </summary>
      public String ConnectionString { get; set; }
      /// <summary>
      /// The name of the table to receive log events
      /// </summary>
      public String Table { get; set; }
      /// <summary>
      /// The optional list of column name overrides
      /// </summary>
      public String Columns { get; set; }
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
         // validate configuration properties
         if (String.IsNullOrWhiteSpace(this.Provider))
            this.Provider = "System.Data.SqlClient";
         if (String.IsNullOrWhiteSpace(this.ConnectionString))
         {
            if (String.IsNullOrWhiteSpace(this.ConnectionName))
               throw new ConfigException(this, "ConnectionName");
            this.ConnectionString = ConfigurationManager
               .ConnectionStrings[this.ConnectionName].ConnectionString;
            if (String.IsNullOrWhiteSpace(this.ConnectionString))
               throw new ConfigException(this, "ConnectionName");
         }
         if (String.IsNullOrWhiteSpace(this.Table))
            throw new ConfigException(this, "Table");
         // ensure we can connect to the log database
         using (DbCommand command = CreateCommand(String.Format("SELECT TOP 1 * FROM {0};", this.Table)))
            command.ExecuteNonQuery();
         // construct the table insert statement
         if (this.insertCommand == null)
         {
            StringBuilder command = new StringBuilder();
            command.Append(String.Format("INSERT {0} (", this.Table));
            if (!String.IsNullOrWhiteSpace(this.Columns))
               command.Append(this.Columns);
            else
               command.Append(String.Join(", ", properties.Select(p => p.Substring(p.LastIndexOf('.') + 1))));
            command.Append(") VALUES (");
            command.Append(String.Join(", ", properties.Select((p, i) => String.Format("@p{0}", i))));
            command.Append(");");
            this.insertCommand = command.ToString();
         }
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         // insert the events into the table
         using (DbCommand command = CreateCommand(this.insertCommand))
         {
            for (Int32 i = 0; i < events[0].Properties.Count; i++)
            {
               DbParameter param = command.CreateParameter();
               param.ParameterName = String.Format("@p{0}", i);
               command.Parameters.Add(param);
            }
            foreach (Event evt in events)
            {
               Int32 paramIdx = 0;
               foreach (Object prop in evt.Values)
                  command.Parameters[paramIdx++].Value = MapProperty(prop);
               command.ExecuteNonQuery();
            }
         }
      }
      #endregion

      #region SQL Operations
      /// <summary>
      /// Connects to the log database and creates a
      /// new ADO.NET command
      /// </summary>
      /// <param name="commandText">
      /// The SQL command text
      /// </param>
      /// <returns>
      /// The new command instance
      /// </returns>
      public DbCommand CreateCommand (String commandText)
      {
         DbProviderFactory factory = DbProviderFactories.GetFactory(this.Provider);
         DbConnection connect = factory.CreateConnection();
         connect.ConnectionString = this.ConnectionString;
         connect.Open();
         DbCommand command = connect.CreateCommand();
         command.Disposed += (o, a) => connect.Dispose();
         command.CommandText = commandText;
         return command;
      }
      /// <summary>
      /// Maps an event property to an ADO.NET parameter
      /// </summary>
      /// <param name="prop">
      /// The property value to map
      /// </param>
      /// <returns>
      /// The mapped ADO.NET parameter value
      /// </returns>
      public Object MapProperty (Object prop)
      {
         if (prop == null)
            return DBNull.Value;
         if (prop is ValueType)
            return prop;
         return prop.ToString();
      }
      #endregion
   }
}
