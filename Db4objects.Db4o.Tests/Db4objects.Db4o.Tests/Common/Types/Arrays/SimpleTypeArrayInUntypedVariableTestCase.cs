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

using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Types.Arrays
{
    public class SimpleTypeArrayInUntypedVariableTestCase : AbstractDb4oTestCase
    {
        private static readonly int[] Array = {1, 2, 3};

        protected override void Store()
        {
            Db().Store(new Data(Array));
        }

        public virtual void TestRetrieval()
        {
            var data = (Data) RetrieveOnlyInstance(typeof (Data
                ));
            Assert.IsTrue(data._arr is int[]);
            var arri = (int[]) data._arr;
            ArrayAssert.AreEqual(Array, arri);
        }

        public class Data
        {
            public object _arr;

            public Data(object arr)
            {
                _arr = arr;
            }
        }
    }
}