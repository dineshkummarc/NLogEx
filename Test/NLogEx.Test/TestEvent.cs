//===========================================================================
// MODULE:  TestEvent.cs
// PURPOSE: log event unit test driver
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace NLogEx
{
   [TestClass]
   public class TestEvent
   {
      [TestMethod]
      public void TestConstruction ()
      {
         Event evt;
         BinaryFormatter formatter = new BinaryFormatter();
         MemoryStream stream;
         // default construction
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.IsFalse(evt.Properties.Any());
         // invalid construction
         AssertException(
            () => new Event(new[]
               {
                  new KeyValuePair<String, Object>("test1", "value1"),
                  new KeyValuePair<String, Object>("test1", "value2")
               }
            )
         );
         // valid construction
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.IsTrue(evt.Properties.Any());
         // default serialized event
         stream = new MemoryStream();
         evt = new Event(new KeyValuePair<String, Object>[0]);
         formatter.Serialize(stream, evt);
         stream.Position = 0;
         evt = (Event)formatter.Deserialize(stream);
         Assert.IsFalse(evt.Properties.Any());
         // valid serialized event
         stream = new MemoryStream();
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         formatter.Serialize(stream, evt);
         stream.Position = 0;
         evt = (Event)formatter.Deserialize(stream);
         Assert.AreEqual(evt.Properties.Count, 2);
         Assert.AreEqual(evt.Properties[0].Key, "test1");
         Assert.AreEqual(evt.Properties[0].Value, "value1");
         Assert.AreEqual(evt.Properties[1].Key, "test2");
         Assert.AreEqual(evt.Properties[1].Value, "value2");
      }

      [TestMethod]
      public void TestProperties ()
      {
         Event evt;
         // property map
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.AreEqual(evt[null], null);
         Assert.AreEqual(evt[""], null);
         Assert.AreEqual(evt[" "], null);
         Assert.AreEqual(evt["test"], null);
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.AreEqual(evt[null], null);
         Assert.AreEqual(evt[""], null);
         Assert.AreEqual(evt[" "], null);
         Assert.AreEqual(evt["test"], null);
         Assert.AreEqual(evt["test1"], "value1");
         Assert.AreEqual(evt["test2"], "value2");
         // property list
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.IsFalse(evt.Properties.Any());
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.AreEqual(evt.Properties.Count, 2);
         Assert.AreEqual(evt.Properties[0].Key, "test1");
         Assert.AreEqual(evt.Properties[0].Value, "value1");
         Assert.AreEqual(evt.Properties[1].Key, "test2");
         Assert.AreEqual(evt.Properties[1].Value, "value2");
         // property names
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.IsFalse(evt.Names.Any());
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.AreEqual(evt.Names.Count(), 2);
         Assert.AreEqual(evt.Names.First(), "test1");
         Assert.AreEqual(evt.Names.Skip(1).Single(), "test2");
         // property values
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.IsFalse(evt.Values.Any());
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.AreEqual(evt.Values.Count(), 2);
         Assert.AreEqual(evt.Values.First(), "value1");
         Assert.AreEqual(evt.Values.Skip(1).Single(), "value2");
      }

      [TestMethod]
      public void TestComparison ()
      {
         Event evt1, evt2, evt3;
         // empty comparison
         evt1 = new Event(new KeyValuePair<String, Object>[0]);
         evt2 = new Event(new KeyValuePair<String, Object>[0]);
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.IsFalse(evt1.Equals(null));
         Assert.IsFalse(evt1.Equals(""));
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
         // value comparison
         evt1 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt2 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value2")
            }
         );
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
         // count comparison
         evt1 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt2 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
         // list comparison
         evt1 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         evt2 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value2"),
               new KeyValuePair<String, Object>("test2", "value1")
            }
         );
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
         // ordering comparison
         evt1 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test2", "value2"),
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt2 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test2", "value2"),
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         evt3 = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value2"),
               new KeyValuePair<String, Object>("test2", "value1")
            }
         );
         Assert.IsTrue(evt1.Equals(evt1));
         Assert.AreEqual(evt1.GetHashCode(), evt2.GetHashCode());
         Assert.IsTrue(evt1.Equals(evt2));
         Assert.IsFalse(evt1.Equals(evt3));
      }

      [TestMethod]
      public void TestOperations ()
      {
         Event evt;
         // string conversion
         evt = new Event(new KeyValuePair<String, Object>[0]);
         Assert.AreEqual(evt.ToString(), "");
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1")
            }
         );
         Assert.AreEqual(evt.ToString(), "test1: value1\r\n");
         evt = new Event(new[]
            {
               new KeyValuePair<String, Object>("test1", "value1"),
               new KeyValuePair<String, Object>("test2", "value2")
            }
         );
         Assert.AreEqual(evt.ToString(), "test1: value1\r\ntest2: value2\r\n");
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
