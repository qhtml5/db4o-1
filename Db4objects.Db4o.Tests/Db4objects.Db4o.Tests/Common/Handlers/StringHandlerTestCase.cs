/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Slots;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public class StringHandlerTestCase : TypeHandlerTestCaseBase
    {
        public static void Main(string[] arguments)
        {
            new StringHandlerTestCase().RunSolo();
        }

        public virtual void TestIndexMarshalling()
        {
            var reader = new ByteArrayBuffer(2*Const4.IntLength);
            var original = new Slot(unchecked(0xdb), unchecked(0x40));
            StringHandler().WriteIndexEntry(Context(), reader, original);
            reader._offset = 0;
            var retrieved = (Slot) StringHandler().ReadIndexEntry(Context(), reader);
            Assert.AreEqual(original.Address(), retrieved.Address());
            Assert.AreEqual(original.Length(), retrieved.Length());
        }

        private StringHandler StringHandler()
        {
            return new StringHandler();
        }

        public virtual void TestReadWrite()
        {
            var writeContext = new MockWriteContext(Db());
            StringHandler().Write(writeContext, "one");
            var readContext = new MockReadContext(writeContext);
            var str = (string) StringHandler().Read(readContext);
            Assert.AreEqual("one", str);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestStoreObject()
        {
            DoTestStoreObject(new Item("one"));
        }

        public class Item
        {
            public string _string;

            public Item(string s)
            {
                _string = s;
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                {
                    return true;
                }
                if (!(obj is Item))
                {
                    return false;
                }
                var other = (Item) obj;
                return _string.Equals(other._string);
            }

            public override int GetHashCode()
            {
                var hash = 7;
                hash = 31*hash + (null == _string ? 0 : _string.GetHashCode());
                return hash;
            }

            public override string ToString()
            {
                return "[" + _string + "]";
            }
        }
    }
}