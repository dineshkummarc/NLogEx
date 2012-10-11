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
using System.Data;
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
   /// This class serializes log events to a configured database table. 
   /// </remarks>
   public sealed class DBTable : ILogger
   {
      private DbDataAdapter adapter;
      private DataSet dataset;

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
         if (String.IsNullOrWhiteSpace(this.Columns))
            this.Columns = String.Join(
               ", ", 
               properties.Select(p => p.Substring(p.IndexOf('.') + 1))
            );
         // construct the generic SELECT statement,
         // used to generate the parameterized insert statement
         StringBuilder selectCommand = new StringBuilder();
         selectCommand.Append("SELECT ");
         selectCommand.Append(this.Columns);
         selectCommand.Append(String.Format(" FROM {0} WHERE (0 = 1);", this.Table));
         // create the data adapter
         DbProviderFactory factory = DbProviderFactories.GetFactory(this.Provider);
         this.adapter = factory.CreateDataAdapter();
         this.adapter.SelectCommand = factory.CreateCommand();
         this.adapter.SelectCommand.Connection = factory.CreateConnection();
         this.adapter.SelectCommand.Connection.ConnectionString = this.ConnectionString;
         this.adapter.SelectCommand.CommandText = selectCommand.ToString();
         // load the empty dataset to validate the target table
         this.dataset = new DataSet();
         this.adapter.SelectCommand.Connection.Open();
         try { this.adapter.Fill(this.dataset); }
         finally { this.adapter.SelectCommand.Connection.Close(); }
         // generate the insert statement from the select statement
         DbCommandBuilder builder = factory.CreateCommandBuilder();
         builder.DataAdapter = this.adapter;
         this.adapter.InsertCommand = builder.GetInsertCommand();
      }
      /// <summary>
      /// Log event dispatch
      /// </summary>
      /// <param name="events">
      /// The list of events to log
      /// </param>
      public void Log (IList<Event> events)
      {
         // connect the adapter to the log database
         this.adapter.InsertCommand.Connection.Open();
         try
         {
            // insert the events into the table
            foreach (Event evt in events)
            {
               DataRow row = this.dataset.Tables[0].NewRow();
               for (Int32 i = 0; i < evt.Properties.Count; i++)
                  row[i] = MapProperty(evt.Properties[i].Value);
               this.dataset.Tables[0].Rows.Add(row);
            }
            // commit all inserts and clear the dataset
            this.adapter.Update(this.dataset);
            this.dataset.Clear();
         }
         finally
         {
            this.adapter.InsertCommand.Connection.Close();
         }
      }
      #endregion

      #region SQL Operations
      /// <summary>
      /// Maps an event property to an ADO.NET parameter
      /// </summary>
      /// <param name="prop">
      /// The property value to map
      /// </param>
      /// <returns>
      /// The mapped ADO.NET parameter value
      /// </returns>
      private Object MapProperty (Object prop)
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
