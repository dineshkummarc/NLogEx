﻿//===========================================================================
// MODULE:  AssemblyInfo.cs
// PURPOSE: assembly configuration properties
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// Project References

[assembly:Guid("BEBD119C-E418-41CC-9F5E-63C64B9052C7")]
[assembly:AssemblyTitle("NLogEx")]
[assembly:AssemblyDescription(".NET Logging Extensions Library")]
[assembly:AssemblyCompany("Brent M. Spell")]
[assembly:AssemblyProduct("NLogEx")]
[assembly:AssemblyCopyright("Copyright © 2012 Brent M. Spell. All Rights Reserved.")]
[assembly:CLSCompliant(true)]
[assembly:ComVisible(false)]
#if DEBUG
[assembly:AssemblyConfiguration("Debug")]
#else
[assembly:AssemblyConfiguration("Release")]
#endif
