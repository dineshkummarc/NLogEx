//===========================================================================
// MODULE:  FilterElement.cs
// PURPOSE: ASP.NET MVC action filter/filter provider configuration
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
using System.Linq;
using System.Web.Mvc;
// Project References

namespace NLogEx.Mvc
{
   /// <summary>
   /// The action filter element
   /// </summary>
   /// <remarks>
   /// This class represents a MVC action filter described in web.config. 
   /// Filters are registered with the MVC subsystem via the IProviderFilter
   /// interface implemented by the collection. Action filters are 
   /// identified by their CLR type, and may be optionally filtered (haha) 
   /// by controller name and/or action name.
   /// </remarks>
   /// <example>
   ///   <configuration>
   ///      <NLogEx.Mvc>
   ///         <filters>
   ///            <!-- filter for all controllers/actions -->
   ///            <add type="System.Web.Mvc.HandleErrorAttribute, System.Web.Mvc, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
   ///            <!-- filter for specific controller -->
   ///            <add type="NLogEx.Mvc.LogAttribute,NLogEx.Mvc"
   ///               controller="MyController"/>
   ///            <!-- filter for specific controller/action -->
   ///            <add type="NLogEx.Mvc.LogAttribute,NLogEx.Mvc"
   ///               controller="MyOtherController"
   ///               action="Index"/>
   ///         </filters>
   ///      </NLogEx.Mvc>
   ///   </configuration>
   /// </example>
   internal sealed class FilterElement : ConfigurationElement
   {
      private System.Web.Mvc.Filter filter;

      #region Configuration Properties
      [ConfigurationProperty("type", IsRequired = true)]
      private String ConfigType
      {
         get { return (String)base["type"]; }
      }
      [ConfigurationProperty("controller")]
      private String Controller
      {
         get { return (String)base["controller"]; }
      }
      [ConfigurationProperty("action")]
      private String Action
      {
         get { return (String)base["action"]; }
      }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// Converts the configured type name to a System.Type
      /// </summary>
      public Type RuntimeType
      {
         get { return Type.GetType(this.ConfigType, true); }
      }
      /// <summary>
      /// The runtime filter instance
      /// </summary>
      public System.Web.Mvc.Filter Filter
      {
         get
         {
            if (this.filter == null)
               this.filter = new System.Web.Mvc.Filter(
                  Activator.CreateInstance(this.RuntimeType),
                  FilterScope.Last,
                  null
               );
            return this.filter;
         }
      }
      #endregion

      /// <summary>
      /// The filter configuration collection
      /// </summary>
      public sealed class Collection : ConfigurationElementCollection, IFilterProvider
      {
         #region IFilterProvider Implementation
         /// <summary>
         /// Loads the configuration-based filters and
         /// returns them to the MVC runtime
         /// </summary>
         /// <param name="controllerCtx">
         /// The current controller
         /// </param>
         /// <param name="actionDesc">
         /// The current action
         /// </param>
         /// <returns>
         /// An enumeration of configured action filters
         /// </returns>
         public IEnumerable<System.Web.Mvc.Filter> GetFilters (
            ControllerContext controllerCtx,
            ActionDescriptor actionDesc)
         {
            String controller = actionDesc.ControllerDescriptor.ControllerName;
            String action = actionDesc.ActionName;
            return this
               .Cast<FilterElement>()
               // match the controller name or all controllers
               .Where(f => 
                  String.IsNullOrEmpty(f.Controller) || 
                  String.Compare(f.Controller, controller, false) == 0
               )
               // match the action name or all actions
               .Where(f => 
                  String.IsNullOrEmpty(f.Action) || 
                  String.Compare(f.Action, action, false) == 0
               )
               // action attributes override configured attributes
               .Where(f => 
                  !actionDesc.GetCustomAttributes(f.RuntimeType, true).Any()
               )
               .Select(f => f.Filter);
         }
         #endregion

         #region ConfigurationElementCollection Overrides
         /// <summary>
         /// Retrieves a key value for the collection
         /// </summary>
         /// <param name="element">
         /// The configuration collection element
         /// </param>
         /// <returns>
         /// The collection element's key
         /// </returns>
         protected override Object GetElementKey (ConfigurationElement element)
         {
            return ((FilterElement)element).ConfigType;
         }
         /// <summary>
         /// Creates a new typed collection element
         /// </summary>
         /// <returns>
         /// The new element
         /// </returns>
         protected override ConfigurationElement CreateNewElement ()
         {
            return new FilterElement();
         }
         #endregion
      }
   }
}
