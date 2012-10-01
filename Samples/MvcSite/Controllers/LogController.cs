//===========================================================================
// MODULE:  BaseController.cs
// PURPOSE: site home (default) controller
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
using System.Web.Mvc;
// Project References

namespace MvcSite.Controllers
{
   /// <summary>
   /// The home controller
   /// </summary>
   [OutputCache(CacheProfile = "Dynamic")]
   public class LogController : BaseController
   {
      //
      // GET: /Home/
      [HttpGet]
      public ActionResult Index (Int32? id)
      {
         return View(new Models.Log(id ?? 0));
      }
      //
      // GET: /Home/Event/{id}
      [HttpGet]
      public ActionResult Event (Int32 id)
      {
         return View(Models.Log.Fetch(id));
      }
      //
      // GET: /Home/NoLog
      [HttpGet]
      [NLogEx.Mvc.Log(Disabled = true)]
      public ActionResult NoLog()
      {
         return RedirectToAction("Index");
      }
      //
      // GET: /Home/Warning
      [HttpGet]
      public ActionResult Warning ()
      {
         new NLogEx.Log(GetType()).Warn("This is a warning");
         return RedirectToAction("Index");
      }
      //
      // GET: /Home/Error
      [HttpGet]
      [HandleError(View = "Error")]
      public ActionResult Error ()
      {
         throw new Exception("This is an exception");
      }
   }
}
