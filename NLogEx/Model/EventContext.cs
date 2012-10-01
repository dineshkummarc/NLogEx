//===========================================================================
// MODULE:  EventContext.cs
// PURPOSE: log event context
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
using System.Reflection;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log event context
   /// </summary>
   /// <remarks>
   /// This class represents the union of event property 
   /// sources sampled at the time of event publication.
   /// These consist of event-affine properties, either
   /// intrinsic (type, etc) or explicit; the event source
   /// properties specified during log construction, and
   /// any globally-registered properties. The event context
   /// provides a single point for retrieving these properties
   /// when initializing a new Event object instance.
   /// </remarks>
   internal sealed class EventContext
   {
      private KeyValuePair<String, Object> sourceProps;
      private KeyValuePair<String, Object> eventProps;
      private Dictionary<String, Object> globalProps;

      #region Properties
      /// <summary>
      /// Event type
      /// </summary>
      public EventType Type { get; set; }
      /// <summary>
      /// Event source runtime type
      /// </summary>
      public Type Source { get; set; }
      /// <summary>
      /// Event local high precision timestamp
      /// </summary>
      public Int64 Timestamp { get; set; }
      /// <summary>
      /// Event publication time (UTC)
      /// </summary>
      public DateTime Time { get; set; }
      /// <summary>
      /// Error event exception
      /// </summary>
      public Exception Exception { get; set; }
      /// <summary>
      /// Event message
      /// </summary>
      public String Message
      {
         get
         {
            return
               (this.MessageFormat != null) ?
                  (this.MessageParams != null) ?
                     String.Format(this.MessageFormat, this.MessageParams) :
                     this.MessageFormat :
                  (this.Exception != null) ?
                     this.Exception.Message :
                     null;
         }
      }
      /// <summary>
      /// Event message format string
      /// </summary>
      internal String MessageFormat { get; set; }
      /// <summary>
      /// Event message format parameters
      /// </summary>
      internal Object[] MessageParams { get; set; }
      #endregion

      #region Operations
      /// <summary>
      /// Attaches the properties of the event
      /// source object associated with the log
      /// </summary>
      /// <param name="type">
      /// The event source type
      /// </param>
      /// <param name="props">
      /// The event source properties
      /// </param>
      public void SetSourceProps (Type type, Object props)
      {
         // use the unqualified type name as the context name
         this.sourceProps = new KeyValuePair<String, Object>(type.Name, props);
      }
      /// <summary>
      /// Attaches the event-affine properties
      /// </summary>
      /// <param name="props">
      /// The event property object, whose
      /// properties/fields are included in
      /// the context
      /// </param>
      public void SetEventProps (Object props)
      {
         String propCtx = null;
         if (props != null)
         {
            Type propType = props.GetType();
            // if the property object is not an anonymous type, then 
            // use its unqualified class name as the context name
            // otherwise, retrieve a "Context" property as the context
            // name, falling back on "Event"
            Boolean isAnonymous = propType.GetCustomAttributes(false)
               .OfType<System.Runtime.CompilerServices.CompilerGeneratedAttribute>()
               .Any();
            if (!isAnonymous && !(props is IDictionary<String, Object>))
               propCtx = propType.Name;
            else
            {
               Object ctxProp = null;
               GetProperty(out ctxProp, props, "Context");
               propCtx = (ctxProp ?? "Event").ToString();
            }
         }
         this.eventProps = new KeyValuePair<String, Object>(propCtx, props);
      }
      /// <summary>
      /// Attaches sampled ambient context properties
      /// </summary>
      /// <param name="globalProps">
      /// The property list to attach
      /// </param>
      public void SetGlobalProps (Dictionary<String, Object> globalProps)
      {
         this.globalProps = globalProps;
      }
      /// <summary>
      /// Retrieves an event context property
      /// </summary>
      /// <param name="name">
      /// The qualified name of the property
      /// </param>
      /// <returns>
      /// The context property value
      /// </returns>
      public Object GetProperty (String name)
      {
         // parse the property name
         Int32 propSepIdx = name.IndexOf('.');
         if (propSepIdx == -1)
            throw new ArgumentException("name");
         String propCtx = name.Substring(0, propSepIdx);
         String propName = name.Substring(propSepIdx + 1);
         Object propValue = null;
         // first, attempt to retrieve from intrinsic properties
         if (String.Compare(propCtx, "Event", true) == 0)
            if (GetProperty(out propValue, this, propName, MemberTypes.Property))
               return propValue;
         // next, attempt to retrieve from the event context object
         if (String.Compare(propCtx, this.eventProps.Key, true) == 0)
            if (GetProperty(out propValue, this.eventProps.Value, propName))
               return propValue;
         // next, attempt to retrieve from the source context object
         if (String.Compare(propCtx, this.sourceProps.Key, true) == 0)
            if (GetProperty(out propValue, this.sourceProps.Value, propName))
               return propValue;
         // finally, attempt to retrieve from the global context
         if (this.globalProps != null)
            if (this.globalProps.TryGetValue(name, out propValue))
               return propValue;
         return null;
      }
      /// <summary>
      /// Retrieves an event property from a context object
      /// using reflection
      /// </summary>
      /// <param name="value">
      /// Return the property value via here
      /// </param>
      /// <param name="obj">
      /// The context object to query
      /// </param>
      /// <param name="name">
      /// The property name to query
      /// </param>
      /// <param name="types">
      /// The types of object members to query
      /// </param>
      /// <returns>
      /// True if the property was found on the object
      /// False otherwise
      /// </returns>
      private Boolean GetProperty (
         out Object value,
         Object obj, 
         String name, 
         MemberTypes types = MemberTypes.Field | MemberTypes.Property)
      {
         IDictionary<String, Object> dict = obj as IDictionary<String, Object>;
         if (dict != null)
            return dict.TryGetValue(name, out value);
         if (obj != null)
         {
            BindingFlags flags =
               BindingFlags.Public |
               BindingFlags.Instance |
               BindingFlags.FlattenHierarchy | 
               BindingFlags.IgnoreCase | 
               BindingFlags.ExactBinding;
            MemberInfo ctxProp = obj.GetType()
               .GetMember(name.Substring(name.IndexOf('.') + 1), types, flags)
               .FirstOrDefault();
            if (ctxProp != null)
            {
               if (ctxProp.MemberType.HasFlag(MemberTypes.Field))
                  value = ((FieldInfo)ctxProp).GetValue(obj);
               else
                  value = ((PropertyInfo)ctxProp).GetValue(obj, null);
               return true;
            }
         }
         value = null;
         return false;
      }
      #endregion
   }
}
