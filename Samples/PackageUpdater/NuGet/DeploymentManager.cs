//===========================================================================
// MODULE:  DeploymentManager.cs
// PURPOSE: NuGet package deployment manager
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
using System.Runtime.InteropServices;
using NuGet;
// Project References

namespace NLogEx.Samples.PackageUpdater
{
   /// <summary>
   /// Package deployment manager
   /// </summary>
   /// <remarks>
   /// This class checks for package updates and downloads/deploys
   /// updated NuGet packages using the NuGet core library.
   /// Deployed packages do not behave the same as projects, due to 
   /// the flattened target directory organization, so we can't use 
   /// the built-in NuGet.ProjectManager or NuGet.ProjectInstaller 
   /// classes. The deployment manager is similar to the 
   /// System.Web.WebPages.Administration.PackageManager.WebProjectManager
   /// class, but more flexible as the target lib/tools/content paths can be
   /// customized via configuration.
   /// The deployment manager supports updating assemblies and other files
   /// that are currently loaded/in use, via the Windows API MoveFileEx. Any
   /// package files that are in use will be updated in-place, and will
   /// take effect the next time the associated application restarts.
   /// </remarks>
   internal sealed class DeploymentManager
   {
      static Log Log = new Log(typeof(DeploymentManager));

      #region Properties
      /// <summary>
      /// The package source repository URI/path
      /// </summary>
      public String Source { get; set; }
      /// <summary>
      /// The local package repository root folder name
      /// </summary>
      public String RepositoryRoot { get; set; }
      /// <summary>
      /// The deployment root folder for the current package
      /// </summary>
      public String DeploymentRoot { get; set; }
      /// <summary>
      /// The package symbolic name
      /// </summary>
      public String PackageID { get; set; }
      /// <summary>
      /// The target framework name for the package
      /// </summary>
      public String TargetFramework { get; set; }
      /// <summary>
      /// The relative path to the subfolder target under the
      /// deployment root for files in the "lib" NuGet package folder
      /// </summary>
      public String LibPath { get; set; }
      /// <summary>
      /// The relative path to the subfolder target under the
      /// deployment root for files in the "tools" NuGet package folder
      /// </summary>
      public String ToolPath { get; set; }
      /// <summary>
      /// The relative path to the subfolder target under the
      /// deployment root for files in the "content" NuGet package folder
      /// </summary>
      public String ContentPath { get; set; }
      /// <summary>
      /// The relative path to the subfolder target under the
      /// deployment root for files in an unknown NuGet package folder
      /// </summary>
      public String OtherPath { get; set; }
      /// <summary>
      /// The temporary path used to move files that are in use
      /// when the package is updated
      /// </summary>
      public String TempPath { get; set; }
      #endregion

      #region Install Operations
      /// <summary>
      /// Checks for updates to the current package and
      /// its dependencies, and applies them
      /// </summary>
      public void Update ()
      {
         try
         {
            Log.Trace("Checking for updates for package {0}", this.PackageID);
            var updates = ListOperations();
            if (updates.Any())
            {
               Log.Info(
                  "{0} updates found for package {1}", 
                  updates.Count(op => op.Action == PackageAction.Install), 
                  this.PackageID
               );
               foreach (var op in ListOperations())
                  if (op.Action == PackageAction.Install)
                     Install(op.Package);
                  else
                     Uninstall(op.Package);
            }
         }
         catch (Exception e)
         {
            Log.Error(e);
         }
      }
      /// <summary>
      /// Checks for package updates
      /// </summary>
      /// <returns>
      /// The list of operations (if any) that should be applied to the 
      /// current package or its dependents to bring it up to date
      /// </returns>
      private IList<PackageOperation> ListOperations ()
      {
         var sourceRepo = new PackageRepositoryFactory().CreateRepository(this.Source);
         var localRepo = new PackageRepositoryFactory().CreateRepository(this.RepositoryRoot);
         var targetFramework = VersionUtility.ParseFrameworkName(this.TargetFramework);
         // locate the current version of the package in the source repository
         var pkg = PackageHelper.ResolvePackage(
            sourceRepo,
            localRepo,
            this.PackageID,
            null,
            false
         );
         // walk the package, and emit any update operations
         var walker = new UpdateWalker(
            localRepo,
            sourceRepo,
            new DependentsWalker(localRepo, targetFramework),
            NullConstraintProvider.Instance,
            targetFramework,
            new Logger(),
            true,
            false
         );
         return walker.ResolveOperations(pkg).ToList();
      }
      /// <summary>
      /// Installs a new package into the current deployment directory,
      /// either the root package for the deployment, or a dependency
      /// </summary>
      /// <param name="package">
      /// The package to install
      /// </param>
      private void Install (IPackage package)
      {
         Log.Trace("Installing package {0}, {1}", package.Id, package.Version);
         // install all files into the deployment directory
         foreach (var file in package.GetFiles())
            if (IsCompatible(file))
               InstallFile(package, file);
         // add the package to the local repository
         new PackageRepositoryFactory()
            .CreateRepository(this.RepositoryRoot)
            .AddPackage(package);
         Log.Info("Package {0}, {1} installed successfully", package.Id, package.Version);
      }
      /// <summary>
      /// Removes a package from the current deployment directory,
      /// in preparation for an install of a newer version
      /// </summary>
      /// <param name="package">
      /// The package to remove
      /// </param>
      private void Uninstall (IPackage package)
      {
         Log.Trace("Removing package {0}, {1}", package.Id, package.Version);
         // remove all files in the package from the deployment directory
         foreach (var file in package.GetFiles())
            if (IsCompatible(file))
               UninstallFile(package, file);
         // remove the package from the local repository
         new PackageRepositoryFactory()
            .CreateRepository(this.RepositoryRoot)
            .RemovePackage(package);
         Log.Trace("Package {0}, {1} removed successfully", package.Id, package.Version);
      }
      /// <summary>
      /// Installs a new package file
      /// </summary>
      /// <param name="package">
      /// The package being installed
      /// </param>
      /// <param name="file">
      /// The file to install
      /// </param>
      private void InstallFile (IPackage package, IPackageFile file)
      {
         // map the package file to its deployment location (or suppress it)
         var path = GetFullPath(file);
         if (path == null)
            Log.Trace("Skipping file {0}", file);
         else
         {
            Log.Trace("Extracting file {0} to {1}", file, path);
            // extract the package file and stamp it with the package date
            ExtractFile(file.GetStream(), path);
            File.SetLastWriteTimeUtc(
               path,
               (package.Published ?? DateTimeOffset.UtcNow).UtcDateTime
            );
         }
      }
      /// <summary>
      /// Removes a package file
      /// </summary>
      /// <param name="package">
      /// The package being removed
      /// </param>
      /// <param name="file">
      /// The file to remove
      /// </param>
      private void UninstallFile (IPackage package, IPackageFile file)
      {
         var path = GetFullPath(file);
         if (path != null)
         {
            Log.Trace("Deleting file {0}", path);
            DeleteFile(path);
         }
      }
      /// <summary>
      /// Determines whether a package file is compatible
      /// with the current package's .NET framework version
      /// </summary>
      /// <param name="file">
      /// The package file to test
      /// </param>
      /// <returns>
      /// True if the package file has no target framework or if the target 
      /// framework is compatible with the current framwork 
      /// False otherwise
      /// </returns>
      private Boolean IsCompatible (IPackageFile file)
      {
         if (file.TargetFramework == null)
            return true;
         var pkgFw = VersionUtility.ParseFrameworkName(this.TargetFramework);
         if (VersionUtility.IsCompatible(file.TargetFramework, new[] { pkgFw }))
            return true;
         return false;
      }
      /// <summary>
      /// Constructs the path to the deployment location of a package file
      /// </summary>
      /// <param name="file">
      /// The package file being installed or uninstalled
      /// </param>
      /// <returns>
      /// The package file's target path, if it should be installed
      /// Null otherwise
      /// </returns>
      private String GetFullPath (IPackageFile file)
      {
         var typePath = GetTypeRelativePath(file);
         if (!String.IsNullOrEmpty(typePath))
            return Path.Combine(
               this.DeploymentRoot, 
               typePath, 
               file.EffectivePath
            );
         return null;
      }
      /// <summary>
      /// Maps a package file to a relative deployment path
      /// </summary>
      /// <param name="file">
      /// The package file being installed or uninstalled
      /// </param>
      /// <returns>
      /// The package file's relative deployment path, if it should be 
      /// installed
      /// "" otherwise
      /// </returns>
      private String GetTypeRelativePath (IPackageFile file)
      {
         var typeIdx = file.Path.IndexOf(Path.DirectorySeparatorChar);
         var type = (typeIdx != -1) ? 
            file.Path.Substring(0, typeIdx).ToLower() : 
            null;
         if (type == "lib")
            return this.LibPath;
         else if (type == "tools")
            return this.ToolPath;
         else if (type == "content")
            return this.ContentPath;
         else
            return this.OtherPath;
      }
      #endregion

      #region File Utilities
      /// <summary>
      /// Extracts a stream to a file
      /// </summary>
      /// <remarks>
      /// This operation attempts to write over the target file
      /// even if it is in use by a running application by
      /// renaming it under the Temp directory and using MoveFileEx
      /// to schedule the deletion of the temporary file
      /// </remarks>
      /// <param name="stream">
      /// The stream to extract
      /// </param>
      /// <param name="targetPath">
      /// The location to extract to
      /// </param>
      private void ExtractFile (Stream stream, String targetPath)
      {
         // ensure the directory exists or delete the file
         if (!File.Exists(targetPath))
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
         else
            DeleteFile(targetPath);
         // extract the source over the target
         using (var target =
               new FileStream(
                  targetPath,
                  FileMode.Create,
                  FileAccess.Write,
                  FileShare.None
               )
            )
            stream.CopyTo(target);
      }
      /// <summary>
      /// Deletes a file
      /// </summary>
      /// <remarks>
      /// This operation ensures that the file is deleted
      /// by renaming it if it is in use, and then using
      /// MoveFileEx to delete the renamed file at the next startup
      /// </remarks>
      /// <param name="path">
      /// The path to the file to delete
      /// </param>
      private void DeleteFile (String path)
      {
         if (File.Exists(path))
         {
            // attempt to delete the file directly
            try
            {
               File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
               File.Delete(path);
            }
            catch
            {
               // if the file could not be deleted, 
               // attempt to move it to the temp path
               try
               {
                  var newPath = Path.Combine(this.TempPath, Path.GetRandomFileName());
                  File.Move(path, newPath);
                  path = newPath;
               }
               catch { }
               // finally, schedule it to be deleted at the next startup
               try
               {
                  MoveFileEx(path, null, MoveFileExOptions.DelayUntilReboot);
               }
               catch { }
            }
         }
      }
      #endregion

      #region Native Methods
      // enumerations
      [Flags]
      private enum MoveFileExOptions : uint
      {
         None = 0x00000000,               // for initialization
         DelayUntilReboot = 0x00000004    // delay the move until the next startup
      }
      // operations
      [return: MarshalAs(UnmanagedType.Bool)]
      [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern Boolean MoveFileEx (
         String existingFileName,
         String newFileName,
         [MarshalAs(UnmanagedType.U4)]
         MoveFileExOptions flags);
      #endregion

      /// <summary>
      /// The NuGet logger adapter for routing events to the NLogEx log
      /// </summary>
      private sealed class Logger : NuGet.ILogger
      {
         #region ILogger Implementation
         /// <summary>
         /// Logs a NuGet installer event
         /// </summary>
         /// <param name="level">
         /// Event severity
         /// </param>
         /// <param name="message">
         /// Event message format string
         /// </param>
         /// <param name="args">
         /// Event message format parameters
         /// </param>
         public void Log (MessageLevel level, String message, params Object[] args)
         {
            // only log nuget errors - all tracing is done by the manager
            switch (level)
            {
               case MessageLevel.Error:
                  DeploymentManager.Log.Error(message, args);
                  break;
               default:
                  DeploymentManager.Log.Trace(message, args);
                  break;
            }
         }
         #endregion
      }
   }
}
