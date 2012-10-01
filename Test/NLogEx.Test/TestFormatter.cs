//===========================================================================
// MODULE:  TestFormatter.cs
// PURPOSE: log text formatter unit test driver
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestFormatter
   {
      [TestMethod]
      public void TestProperties ()
      {
         Formatter formatter;
         // default properties
         formatter = new Formatter();
         formatter.Validate();
         // invalid message indent
         formatter = new Formatter() { Indent = -1 };
         AssertException(formatter.Validate);
         // invalid hanging indent
         formatter = new Formatter() { Hang = -1 };
         AssertException(formatter.Validate);
         // invalid wrapping width
         formatter = new Formatter() { Width = 80 };
         AssertException(formatter.Validate);
         formatter = new Formatter() 
         { 
            Wrap = true,
            Width = -1
         };
         AssertException(formatter.Validate);
         formatter = new Formatter()
         {
            Wrap = true,
            Indent = 3,
            Hang = 3,
            Width = 5
         };
         AssertException(formatter.Validate);
         // wrapped format with default width
         formatter = new Formatter() { Wrap = true };
         formatter.Validate();
         // wrapped format with custom width
         formatter = new Formatter()
         {
            Wrap = true,
            Width = 80
         };
         formatter.Validate();
         // indented format
         formatter = new Formatter()
         {
            Indent = 2,
            Hang = 3
         };
         formatter.Validate();
         // format string
         formatter = new Formatter() { Format = "{0}" };
         formatter.Validate();
      }

      [TestMethod]
      public void TestFormatSegment ()
      {
         Formatter formatter;
         // unindented segment
         formatter = new Formatter();
         Assert.AreEqual(formatter.FormatSegmentString(null, 0), null);
         Assert.AreEqual(formatter.FormatSegmentString(null, 1), null);
         Assert.AreEqual(formatter.FormatSegmentString("", 0), "");
         Assert.AreEqual(formatter.FormatSegmentString("", 1), "");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 0), " ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 1), " ");
         Assert.AreEqual(formatter.FormatSegmentString("test", 0), "test");
         Assert.AreEqual(formatter.FormatSegmentString("test", 1), "test");
         // indented segment
         formatter = new Formatter() { Indent = 2 };
         Assert.AreEqual(formatter.FormatSegmentString(null, 0), null);
         Assert.AreEqual(formatter.FormatSegmentString(null, 1), null);
         Assert.AreEqual(formatter.FormatSegmentString("", 0), "  ");
         Assert.AreEqual(formatter.FormatSegmentString("", 1), "  ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 0), "   ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 1), "   ");
         Assert.AreEqual(formatter.FormatSegmentString("test", 0), "  test");
         Assert.AreEqual(formatter.FormatSegmentString("test", 1), "  test");
         // hanging indented segment
         formatter = new Formatter() { Hang = 2 };
         Assert.AreEqual(formatter.FormatSegmentString(null, 0), null);
         Assert.AreEqual(formatter.FormatSegmentString(null, 1), null);
         Assert.AreEqual(formatter.FormatSegmentString("", 0), "");
         Assert.AreEqual(formatter.FormatSegmentString("", 1), "  ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 0), " ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 1), "   ");
         Assert.AreEqual(formatter.FormatSegmentString("test", 0), "test");
         Assert.AreEqual(formatter.FormatSegmentString("test", 1), "  test");
         // indented + hanging indented segment
         formatter = new Formatter() { Indent = 1, Hang = 2 };
         Assert.AreEqual(formatter.FormatSegmentString(null, 0), null);
         Assert.AreEqual(formatter.FormatSegmentString(null, 1), null);
         Assert.AreEqual(formatter.FormatSegmentString("", 0), " ");
         Assert.AreEqual(formatter.FormatSegmentString("", 1), "   ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 0), "  ");
         Assert.AreEqual(formatter.FormatSegmentString(" ", 1), "    ");
         Assert.AreEqual(formatter.FormatSegmentString("test", 0), " test");
         Assert.AreEqual(formatter.FormatSegmentString("test", 1), "   test");
      }

      [TestMethod]
      public void TestFormatLine ()
      {
         Formatter formatter;
         // invalid line format
         formatter = new Formatter() { Wrap = true };
         AssertException(() => formatter.FormatLineString("test", 0));
         formatter = new Formatter() { Wrap = true, Indent = 3, Hang = 3, Width = 5 };
         AssertException(() => formatter.FormatLineString("test", 0));
         // unwrapped, unindented line
         formatter = new Formatter();
         Assert.AreEqual(formatter.FormatLineString(null, 0), null);
         Assert.AreEqual(formatter.FormatLineString(null, 1), null);
         Assert.AreEqual(formatter.FormatLineString("", 0), "\r\n");
         Assert.AreEqual(formatter.FormatLineString("", 1), "\r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 0), " \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 1), " \r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), "test\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "test\r\n");
         // unwrapped, indented line
         formatter = new Formatter() { Indent = 2 };
         Assert.AreEqual(formatter.FormatLineString(null, 0), null);
         Assert.AreEqual(formatter.FormatLineString(null, 1), null);
         Assert.AreEqual(formatter.FormatLineString("", 0), "  \r\n");
         Assert.AreEqual(formatter.FormatLineString("", 1), "  \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 0), "   \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 1), "   \r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), "  test\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "  test\r\n");
         // unwrapped, hanging indented line
         formatter = new Formatter() { Hang = 2 };
         Assert.AreEqual(formatter.FormatLineString(null, 0), null);
         Assert.AreEqual(formatter.FormatLineString(null, 1), null);
         Assert.AreEqual(formatter.FormatLineString("", 0), "\r\n");
         Assert.AreEqual(formatter.FormatLineString("", 1), "  \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 0), " \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 1), "   \r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), "test\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "  test\r\n");
         // unwrapped, indented + hanging indented line
         formatter = new Formatter() { Indent = 1, Hang = 2 };
         Assert.AreEqual(formatter.FormatLineString(null, 0), null);
         Assert.AreEqual(formatter.FormatLineString(null, 1), null);
         Assert.AreEqual(formatter.FormatLineString("", 0), " \r\n");
         Assert.AreEqual(formatter.FormatLineString("", 1), "   \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 0), "  \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 1), "    \r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), " test\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "   test\r\n");
         // wrapped, unclipped, unindented line
         formatter = new Formatter() { Wrap = true, Width = Int32.MaxValue };
         Assert.AreEqual(formatter.FormatLineString(null, 0), null);
         Assert.AreEqual(formatter.FormatLineString(null, 1), null);
         Assert.AreEqual(formatter.FormatLineString("", 0), "\r\n");
         Assert.AreEqual(formatter.FormatLineString("", 1), "\r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 0), " \r\n");
         Assert.AreEqual(formatter.FormatLineString(" ", 1), " \r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), "test\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "test\r\n");
         // wrapped, length-clipped, unindented line
         formatter = new Formatter() { Wrap = true, Width = 3 };
         Assert.AreEqual(formatter.FormatLineString("t", 0), "t\r\n");
         Assert.AreEqual(formatter.FormatLineString("te", 0), "te\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes", 0), "tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 0), "tes\r\nt\r\n");
         Assert.AreEqual(formatter.FormatLineString("testt", 0), "tes\r\ntt\r\n");
         Assert.AreEqual(formatter.FormatLineString("testte", 0), "tes\r\ntte\r\n");
         Assert.AreEqual(formatter.FormatLineString("testtes", 0), "tes\r\ntte\r\ns\r\n");
         Assert.AreEqual(formatter.FormatLineString("testtest", 0), "tes\r\ntte\r\nst\r\n");
         // wrapped, word-clipped, unindented line
         formatter = new Formatter() { Wrap = true, Width = 3 };
         Assert.AreEqual(formatter.FormatLineString("t ", 0), "t \r\n");
         Assert.AreEqual(formatter.FormatLineString("t t", 0), "t t\r\n");
         Assert.AreEqual(formatter.FormatLineString("t te", 0), "t\r\nte\r\n");
         Assert.AreEqual(formatter.FormatLineString("t tes", 0), "t\r\ntes\r\n");
         Assert.AreEqual(formatter.FormatLineString("t tes ", 0), "t\r\ntes\r\n");
         Assert.AreEqual(formatter.FormatLineString("te ", 0), "te \r\n");
         Assert.AreEqual(formatter.FormatLineString("te t", 0), "te\r\nt\r\n");
         Assert.AreEqual(formatter.FormatLineString("te te", 0), "te\r\nte\r\n");
         Assert.AreEqual(formatter.FormatLineString("te tes", 0), "te\r\ntes\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes ", 0), "tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes t", 0), "tes\r\nt\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes te", 0), "tes\r\nte\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes tes", 0), "tes\r\ntes\r\n");
         // wrapped, length-clipped, indented line
         formatter = new Formatter() { Wrap = true, Width = 6, Indent = 1, Hang = 2 };
         Assert.AreEqual(formatter.FormatLineString("t", 1), "   t\r\n");
         Assert.AreEqual(formatter.FormatLineString("te", 1), "   te\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes", 1), "   tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("test", 1), "   tes\r\n   t\r\n");
         Assert.AreEqual(formatter.FormatLineString("testt", 1), "   tes\r\n   tt\r\n");
         Assert.AreEqual(formatter.FormatLineString("testte", 1), "   tes\r\n   tte\r\n");
         Assert.AreEqual(formatter.FormatLineString("testtes", 1), "   tes\r\n   tte\r\n   s\r\n");
         Assert.AreEqual(formatter.FormatLineString("testtest", 1), "   tes\r\n   tte\r\n   st\r\n");
         // wrapped, word-clipped, indented line
         formatter = new Formatter() { Wrap = true, Width = 6, Indent = 1, Hang = 2 };
         Assert.AreEqual(formatter.FormatLineString("t ", 1), "   t \r\n");
         Assert.AreEqual(formatter.FormatLineString("t t", 1), "   t t\r\n");
         Assert.AreEqual(formatter.FormatLineString("t te", 1), "   t\r\n   te\r\n");
         Assert.AreEqual(formatter.FormatLineString("t tes", 1), "   t\r\n   tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("t tes ", 1), "   t\r\n   tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("te ", 1), "   te \r\n");
         Assert.AreEqual(formatter.FormatLineString("te t", 1), "   te\r\n   t\r\n");
         Assert.AreEqual(formatter.FormatLineString("te te", 1), "   te\r\n   te\r\n");
         Assert.AreEqual(formatter.FormatLineString("te tes", 1), "   te\r\n   tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes ", 1), "   tes\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes t", 1), "   tes\r\n   t\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes te", 1), "   tes\r\n   te\r\n");
         Assert.AreEqual(formatter.FormatLineString("tes tes", 1), "   tes\r\n   tes\r\n");
      }

      [TestMethod]
      public void TestFormatMessage ()
      {
         Formatter formatter;
         // invalid formatter
         formatter = new Formatter() { Wrap = true };
         AssertException(() => formatter.FormatMessageString("test"));
         formatter = new Formatter() { Wrap = true, Indent = 3, Hang = 3, Width = 5 };
         AssertException(() => formatter.FormatMessageString("test"));
         // single line message
         formatter = new Formatter();
         Assert.AreEqual(formatter.FormatMessageString(null), null);
         Assert.AreEqual(formatter.FormatMessageString(""), "\r\n");
         Assert.AreEqual(formatter.FormatMessageString(" "), " \r\n");
         Assert.AreEqual(formatter.FormatMessageString("test"), "test\r\n");
         // multi-line message
         formatter = new Formatter();
         Assert.AreEqual(formatter.FormatMessageString("test\r\n"), "test\r\n\r\n");
         Assert.AreEqual(formatter.FormatMessageString("test\r\ntest"), "test\r\ntest\r\n");
         // indented message
         formatter = new Formatter() { Indent = 2, Hang = 1 };
         Assert.AreEqual(formatter.FormatMessageString("test"), "  test\r\n");
         Assert.AreEqual(formatter.FormatMessageString("test\r\n"), "  test\r\n   \r\n");
         Assert.AreEqual(formatter.FormatMessageString("test\r\ntest"), "  test\r\n   test\r\n");
         Assert.AreEqual(formatter.FormatMessageString("test\r\ntest\r\ntest"), "  test\r\n   test\r\n   test\r\n");
         // wrapped message
         formatter = new Formatter() { Wrap = true, Width = 3 };
         Assert.AreEqual(formatter.FormatMessageString("test"), "tes\r\nt\r\n");
         Assert.AreEqual(formatter.FormatMessageString("tes test"), "tes\r\ntes\r\nt\r\n");
         Assert.AreEqual(formatter.FormatMessageString("te test"), "te\r\ntes\r\nt\r\n");
      }

      [TestMethod]
      public void TestFormatEvent ()
      {
         Formatter formatter;
         Event evt = new Event(
            new[]
            {
               new KeyValuePair<String, Object>("Event.Message", "test"),
               new KeyValuePair<String, Object>("Event.Time", "1")
            }
         );
         // invalid formatter
         formatter = new Formatter() { Wrap = true };
         AssertException(() => formatter.FormatEventString(evt));
         formatter = new Formatter() { Wrap = true, Indent = 3, Hang = 3, Width = 5 };
         AssertException(() => formatter.FormatEventString(evt));
         // default format string
         formatter = new Formatter();
         Assert.IsNotNull(formatter.FormatEventString(evt));
         // custom format string
         formatter = new Formatter() { Format = "{0}{1}" };
         Assert.AreEqual(formatter.FormatEventString(evt), "test1\r\n");
         // indented event
         formatter = new Formatter() { Format = "{0}\r\n{1}", Indent = 2, Hang = 1 };
         Assert.AreEqual(formatter.FormatEventString(evt), "  test\r\n   1\r\n");
         // wrapped event
         formatter = new Formatter() { Format = "{0}{1}", Wrap = true, Width = 3 };
         Assert.AreEqual(formatter.FormatEventString(evt), "tes\r\nt1\r\n");
      }

      [TestMethod]
      public void TestFormatEventList ()
      {
         Formatter formatter;
         Event[] events = new[]
         {
            new Event(
               new[]
               {
                  new KeyValuePair<String, Object>("Event.Message", "test"),
                  new KeyValuePair<String, Object>("Event.Time", "1")
               }
            ),
            new Event(
               new[]
               {
                  new KeyValuePair<String, Object>("Event.Message", "test"),
                  new KeyValuePair<String, Object>("Event.Time", "2")
               }
            )
         };
         // invalid formatter
         formatter = new Formatter() { Wrap = true };
         AssertException(() => formatter.FormatEventListString(events));
         formatter = new Formatter() { Wrap = true, Indent = 3, Hang = 3, Width = 5 };
         AssertException(() => formatter.FormatEventListString(events));
         // default format string
         formatter = new Formatter();
         Assert.IsNull(formatter.FormatEventListString(null));
         Assert.AreEqual(formatter.FormatEventListString(new Event[0]), "");
         Assert.IsNotNull(formatter.FormatEventListString(events));
         // custom format string
         formatter = new Formatter() { Format = "{0}{1}" };
         Assert.IsNull(formatter.FormatEventListString(null));
         Assert.AreEqual(formatter.FormatEventListString(new Event[0]), "");
         Assert.AreEqual(formatter.FormatEventListString(events), "test1\r\ntest2\r\n");
         // indented event
         formatter = new Formatter() { Format = "{0}\r\n{1}", Indent = 2, Hang = 1 };
         Assert.IsNull(formatter.FormatEventListString(null));
         Assert.AreEqual(formatter.FormatEventListString(new Event[0]), "");
         Assert.AreEqual(formatter.FormatEventListString(events), "  test\r\n   1\r\n  test\r\n   2\r\n");
         // wrapped event
         formatter = new Formatter() { Format = "{0}{1}", Wrap = true, Width = 3 };
         Assert.IsNull(formatter.FormatEventListString(null));
         Assert.AreEqual(formatter.FormatEventListString(new Event[0]), "");
         Assert.AreEqual(formatter.FormatEventListString(events), "tes\r\nt1\r\ntes\r\nt2\r\n");
      }

      private void AssertException (Action action)
      {
         try
         {
            action();
            Assert.Fail();
         }
         catch (AssertFailedException) { throw; }
         catch { }
      }
   }
}
