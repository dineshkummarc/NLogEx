//===========================================================================
// MODULE:  PackageElement.cs
// PURPOSE: package deployment configuration
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
using System.Configuration;
// Project References

namespace NLogEx.Samples.PackageUpdater
{
   /// <summary>
   /// Deployed package configuration element
   /// </summary>
   /// <remarks>
   /// This class represents a local deployment of an individual
   /// NuGet package. The ID property indicates the symbolic name
   /// of the package (ex: EntityFramework), and the DeployRoot
   /// property specifies where the package is deployed locally.
   /// </remarks>
   internal sealed class PackageElement : ConfigurationElement
   {
      #region Configuration Properties
      /// <summary>
      /// The unique symbolic name of the package (ex: EntityFramework)
      /// </summary>
      [ConfigurationProperty("id", IsRequired = true)]
      public String ID
      {
         get { return (String)base["id"]; }
      }
      /// <summary>
      /// The folder in which the package is deployed locally
      /// </summary>
      [ConfigurationProperty("deployRoot", IsRequired = true)]
      public String DeployRoot
      {
         get { return System.IO.Path.GetFullPath((String)base["deployRoot"]); }
      }
      /// <summary>
      /// The package's source repository
      /// </summary>
      [ConfigurationProperty("source", DefaultValue = "https://nuget.org/api/v2/")]
      public String Source
      {
         get { return (String)base["source"]; }
      }
      /// <summary>
      /// The target .NET framework version/profile for the package
      /// </summary>
      [ConfigurationProperty("framework", DefaultValue = "net40")]
      public String Framework
      {
         get { return (String)base["framework"]; }
      }
      /// <summary>
      /// The name of the subfolder under DeployRoot in which to
      /// place files in the "lib" NuGet package folder
      /// </summary>
      [ConfigurationProperty("libPath", DefaultValue = ".")]
      public String LibPath
      {
         get { return (String)base["libPath"]; }
      }
      /// <summary>
      /// The name of the subfolder under DeployRoot in which to
      /// place files in the "tools" NuGet package folder
      /// </summary>
      [ConfigurationProperty("toolPath", DefaultValue = ".")]
      public String ToolPath
      {
         get { return (String)base["toolPath"]; }
      }
      /// <summary>
      /// The name of the subfolder under DeployRoot in which to
      /// place files in the "content" NuGet package folder
      /// </summary>
      [ConfigurationProperty("contentPath", DefaultValue = ".")]
      public String ContentPath
      {
         get { return (String)base["contentPath"]; }
      }
      /// <summary>
      /// The name of the subfolder under DeployRoot in which to
      /// place files in an unknown NuGet package folder
      /// </summary>
      [ConfigurationProperty("otherPath", DefaultValue = "")]
      public String OtherPath
      {
         get { return (String)base["otherPath"]; }
      }
      #endregion

      /// <summary>
      /// The package configuration collection
      /// </summary>
      public sealed class Collection : ConfigurationElementCollection
      {
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
            return ((PackageElement)element).ID;
         }
         /// <summary>
         /// Creates a new typed collection element
         /// </summary>
         /// <returns>
         /// The new element
         /// </returns>
         protected override ConfigurationElement CreateNewElement ()
         {
            return new PackageElement();
         }
         #endregion
      }
   }
}
