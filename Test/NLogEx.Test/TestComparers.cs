//===========================================================================
// MODULE:  TestEvent.cs
// PURPOSE: log filter comparer unit test driver
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

namespace NLogEx.Test
{
   [TestClass]
   public class TestComparers
   {
      [TestMethod]
      public void TestDefault ()
      {
         // matching
         Assert.IsTrue(new Comparers.Default().Equals(42, 42));
         Assert.IsTrue(new Comparers.Default().Equals(42, "42"));
         Assert.IsTrue(new Comparers.Default().Equals("42", 42));
         Assert.IsTrue(new Comparers.Default().Equals("", ""));
         Assert.IsTrue(new Comparers.Default().Equals("abc", "abc"));
         Assert.IsTrue(new Comparers.Default().Equals(TestEnum.One, TestEnum.One));
         Assert.IsTrue(new Comparers.Default().Equals(TestEnum.One, "One"));
         Assert.IsTrue(new Comparers.Default().Equals(TestEnum.One, "one"));
         Assert.IsTrue(new Comparers.Default().Equals(TestEnum.One, "1"));
         // non-matching
         Assert.IsFalse(new Comparers.Default().Equals(42, 43));
         Assert.IsFalse(new Comparers.Default().Equals(42, "43"));
         Assert.IsFalse(new Comparers.Default().Equals("42", 43));
         Assert.IsFalse(new Comparers.Default().Equals("", " "));
         Assert.IsFalse(new Comparers.Default().Equals("abc", "123"));
         Assert.IsFalse(new Comparers.Default().Equals("abc", "ABC"));
         Assert.IsFalse(new Comparers.Default().Equals(TestEnum.One, TestEnum.Two));
         Assert.IsFalse(new Comparers.Default().Equals(TestEnum.One, "Two"));
         Assert.IsFalse(new Comparers.Default().Equals(TestEnum.One, "2"));
      }

      [TestMethod]
      public void TestHasAttribute ()
      {
         // matching
         Assert.IsTrue(new Comparers.HasAttribute().Equals(TestEnum.One, typeof(FlagsAttribute)));
         Assert.IsTrue(new Comparers.HasAttribute().Equals(TestEnum.One, "System.FlagsAttribute"));
         Assert.IsTrue(new Comparers.HasAttribute().Equals(typeof(TestEnum), typeof(FlagsAttribute)));
         Assert.IsTrue(new Comparers.HasAttribute().Equals(typeof(TestEnum), "System.FlagsAttribute"));
         // non-matching
         Assert.IsFalse(new Comparers.HasAttribute().Equals(new Object(), typeof(FlagsAttribute)));
         Assert.IsFalse(new Comparers.HasAttribute().Equals(TestEnum.One, typeof(AttributeUsageAttribute)));
         Assert.IsFalse(new Comparers.HasAttribute().Equals(TestEnum.One, "System.AttributeUsageAttribute"));
         Assert.IsFalse(new Comparers.HasAttribute().Equals(typeof(Object), typeof(FlagsAttribute)));
         Assert.IsFalse(new Comparers.HasAttribute().Equals(typeof(TestEnum), typeof(AttributeUsageAttribute)));
         Assert.IsFalse(new Comparers.HasAttribute().Equals(typeof(TestEnum), "System.AttributeUsageAttribute"));
      }

      [TestMethod]
      public void TestHasFlag ()
      {
         // matching
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, TestEnum.Three));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, TestEnum.One));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, TestEnum.Two));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, "Three"));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, "three"));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, "One"));
         Assert.IsTrue(new Comparers.HasFlag().Equals(TestEnum.Three, "Two"));
         // non-matching
         Assert.IsFalse(new Comparers.HasFlag().Equals(TestEnum.One, TestEnum.Three));
         Assert.IsFalse(new Comparers.HasFlag().Equals(TestEnum.One, TestEnum.Two));
         Assert.IsFalse(new Comparers.HasFlag().Equals(TestEnum.One, "three"));
         Assert.IsFalse(new Comparers.HasFlag().Equals(TestEnum.One, "Three"));
         Assert.IsFalse(new Comparers.HasFlag().Equals(TestEnum.One, "Two"));
      }

      [TestMethod]
      public void TestHasSubstring ()
      {
         // matching
         Assert.IsTrue(new Comparers.HasSubstring().Equals("test", ""));
         Assert.IsTrue(new Comparers.HasSubstring().Equals("test", "es"));
         Assert.IsTrue(new Comparers.HasSubstring().Equals("test", "test"));
         Assert.IsTrue(new Comparers.HasSubstring().Equals("testtest", "es"));
         Assert.IsTrue(new Comparers.HasSubstring().Equals(1, "1"));
         // non-matching
         Assert.IsFalse(new Comparers.HasSubstring().Equals("", "test"));
         Assert.IsFalse(new Comparers.HasSubstring().Equals("test", "tset"));
         Assert.IsFalse(new Comparers.HasSubstring().Equals("test", "testtest"));
         Assert.IsFalse(new Comparers.HasSubstring().Equals("test", "Test"));
      }

      [TestMethod]
      public void TestIsGreaterThan ()
      {
         // matching
         Assert.IsTrue(new Comparers.IsGreaterThan().Equals("acd", "abc"));
         Assert.IsTrue(new Comparers.IsGreaterThan().Equals("bcd", "abc"));
         Assert.IsTrue(new Comparers.IsGreaterThan().Equals(2, 1));
         Assert.IsTrue(new Comparers.IsGreaterThan().Equals(2, "1"));
         Assert.IsTrue(new Comparers.IsGreaterThan().Equals("2", 1));
         // non-matching
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals("abc", "abc"));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals("abc", "acd"));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals("abc", "bcd"));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals(1, 1));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals(1, 2));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals(1, "2"));
         Assert.IsFalse(new Comparers.IsGreaterThan().Equals("1", 2));
      }

      [TestMethod]
      public void TestIsLessThan ()
      {
         // matching
         Assert.IsTrue(new Comparers.IsLessThan().Equals("abc", "acd"));
         Assert.IsTrue(new Comparers.IsLessThan().Equals("abc", "bcd"));
         Assert.IsTrue(new Comparers.IsLessThan().Equals(1, 2));
         Assert.IsTrue(new Comparers.IsLessThan().Equals(1, "2"));
         Assert.IsTrue(new Comparers.IsLessThan().Equals("1", 2));
         // non-matching
         Assert.IsFalse(new Comparers.IsLessThan().Equals("abc", "abc"));
         Assert.IsFalse(new Comparers.IsLessThan().Equals("acd", "abc"));
         Assert.IsFalse(new Comparers.IsLessThan().Equals("bcd", "abc"));
         Assert.IsFalse(new Comparers.IsLessThan().Equals(1, 1));
         Assert.IsFalse(new Comparers.IsLessThan().Equals(2, 1));
         Assert.IsFalse(new Comparers.IsLessThan().Equals(2, "1"));
         Assert.IsFalse(new Comparers.IsLessThan().Equals("2", 1));
      }

      [TestMethod]
      public void TestIsSubclassOf ()
      {
         // matching
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Base)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Object)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Derived)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Base)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Object)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Descended)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Derived)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Base)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Object)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Base).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Object).FullName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Derived).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Base).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Object).FullName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Descended).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Derived).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Base).AssemblyQualifiedName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(new Descended(), typeof(Object).FullName));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(typeof(Base), typeof(Base)));
         Assert.IsTrue(new Comparers.IsSubclassOf().Equals(typeof(Derived), typeof(Base)));
         // non-matching
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Base)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Derived)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Descended)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Base)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Derived)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Descended)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Base)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Derived)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Descended)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Descended)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Descended)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Derived)));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Base).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Derived).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Descended).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Base).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Derived).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(42, typeof(Descended).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Base).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Derived).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Object(), typeof(Descended).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Derived(), typeof(Descended).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Descended).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(new Base(), typeof(Derived).AssemblyQualifiedName));
         Assert.IsFalse(new Comparers.IsSubclassOf().Equals(typeof(Int32), typeof(Base)));
      }

      [TestMethod]
      public void TestMatchesRegEx ()
      {
         // matching
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("", ""));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("", ".*"));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("a", "a"));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals(42, "42"));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("aaa", "a*"));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("abcdefghi", "ghi"));
         Assert.IsTrue(new Comparers.MatchesRegEx().Equals("abcabcabc", "(abc){3}"));
         // non-matching
         Assert.IsFalse(new Comparers.MatchesRegEx().Equals("", ".+"));
         Assert.IsFalse(new Comparers.MatchesRegEx().Equals("abc", "d"));
         Assert.IsFalse(new Comparers.MatchesRegEx().Equals("abc", "def"));
         Assert.IsFalse(new Comparers.MatchesRegEx().Equals(42, "a"));
      }

      [Flags]
      private enum TestEnum
      {
         One   = 0x00000001,
         Two   = 0x00000002,
         Three = One | Two
      }

      private class Base
      {
      }

      private class Derived : Base
      {
      }

      private class Descended : Derived
      {
      }
   }
}
