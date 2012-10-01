//===========================================================================
// MODULE:  Event.cs
// PURPOSE: log event
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
// Project References

namespace NLogEx
{
   #region Common Definitions
   /// <summary>
   /// The log event type
   /// </summary>
   public enum EventType
   {
      Trace,      // debug tracing event
      Info,       // informational success event
      Warning,    // non-critical warning event
      Error       // failure event
   }
   #endregion

   /// <summary>
   /// The log event
   /// </summary>
   /// <remarks>
   /// This class represents a single immutable log event delivered
   /// to a logger instance. Each event consists of an ordered
   /// read-only set of named property values representing 
   /// those properties sampled by a log instance.
   /// </remarks>
   [Serializable]
   public sealed class Event
   {
      private IList<KeyValuePair<String, Object>> props;
      [NonSerialized]
      private Hashtable map;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new event instance
      /// </summary>
      /// <param name="props">
      /// The list of properties to attach
      /// </param>
      public Event (IEnumerable<KeyValuePair<String, Object>> props)
      {
         this.props = props.ToList().AsReadOnly();
         this.map = new Hashtable(
            this.props.ToDictionary(p => p.Key, p => p.Value),
            StringComparer.OrdinalIgnoreCase
         );
      }
      /// <summary>
      /// Initializes a new event instance
      /// </summary>
      /// <param name="context">
      /// The current event context
      /// </param>
      /// <param name="propNames">
      /// The list of properties to sample
      /// from the event context
      /// </param>
      internal Event (EventContext context, IEnumerable<String> propNames)
      {
         this.props = propNames
            .Select(p => new KeyValuePair<String, Object>(p, context.GetProperty(p)))
            .ToList()
            .AsReadOnly();
         this.map = new Hashtable(
            this.props.ToDictionary(p => p.Key, p => p.Value),
            StringComparer.OrdinalIgnoreCase
         );
      }
      /// <summary>
      /// Deserializes an event from a stream
      /// </summary>
      /// <param name="info">
      /// The serialized event properties
      /// </param>
      /// <param name="context">
      /// The current serialization context
      /// </param>
      public Event (SerializationInfo info, StreamingContext context)
         : this((IList<KeyValuePair<String, Object>>)
            info.GetValue("props", typeof(IList<KeyValuePair<String, Object>>))
         )
      {
      }
      #endregion

      #region Event Properties
      /// <summary>
      /// Retrieves an event property value
      /// </summary>
      /// <param name="name">
      /// The name of the property to retrieve
      /// </param>
      /// <returns>
      /// The property value
      /// </returns>
      public Object this[String name]
      {
         get { return (!String.IsNullOrWhiteSpace(name)) ? this.map[name] : null; }
      }
      /// <summary>
      /// The ordered list of event properties
      /// </summary>
      public IList<KeyValuePair<String, Object>> Properties
      {
         get { return this.props; }
      }
      /// <summary>
      /// The ordered list of event property names
      /// </summary>
      public IEnumerable<String> Names
      { 
         get { return this.props.Select(p => p.Key); }
      }
      /// <summary>
      /// The ordered list of event property values
      /// </summary>
      public IEnumerable<Object> Values
      {
         get { return this.props.Select(p => p.Value); }
      }
      #endregion

      #region Object Overrides
      /// <summary>
      /// Converts the event to a string representation
      /// </summary>
      /// <returns>
      /// The event string
      /// </returns>
      public override String ToString ()
      {
         StringBuilder str = new StringBuilder();
         foreach (KeyValuePair<String, Object> prop in props)
            str.AppendLine(String.Format("{0}: {1}", prop.Key, prop.Value));
         return str.ToString();
      }
      /// <summary>
      /// Compares the event to another object
      /// </summary>
      /// <param name="obj">
      /// The object to compare
      /// </param>
      /// <returns>
      /// True if the objects are equal
      /// False otherwise
      /// </returns>
      public override Boolean Equals (Object obj)
      {
         if (Object.ReferenceEquals(this, obj))
            return true;
         Event evt = obj as Event;
         if (evt == null)
            return false;
         if (this.props.Count != evt.props.Count)
            return false;
         for (Int32 i = 0; i < this.props.Count; i++)
         {
            if (this.props[i].Key != evt.props[i].Key)
               return false;
            if (!Object.Equals(this.props[i].Value, evt.props[i].Value))
               return false;
         }
         return true;
      }
      /// <summary>
      /// Computes a hash code for the event
      /// </summary>
      /// <returns>
      /// The event hash code
      /// </returns>
      public override Int32 GetHashCode ()
      {
         Int32 hash = 0;
         foreach (KeyValuePair<String, Object> prop in this.props)
         {
            hash ^= prop.Key.GetHashCode();
            if (prop.Value != null)
               hash ^= prop.Value.GetHashCode();
         }
         return hash;
      }
      #endregion
   }
}
