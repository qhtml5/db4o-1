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
using Db4oUnit;
using Db4oUnit.Extensions;

namespace Db4objects.Db4o.Tests.Common.Exceptions
{
    public class ObjectCanNewExceptionTestCase : AbstractDb4oTestCase
    {
        public static void Main(string[] args)
        {
            new ObjectCanNewExceptionTestCase().RunSoloAndClientServer();
        }

        public virtual void Test()
        {
            Assert.Expect(typeof (ReflectException), typeof (ItemException), new _ICodeBlock_24
                (this));
        }

        public class Item
        {
            public virtual bool ObjectCanNew(IObjectContainer container)
            {
                throw new ItemException();
            }
        }

        private sealed class _ICodeBlock_24 : ICodeBlock
        {
            private readonly ObjectCanNewExceptionTestCase _enclosing;

            public _ICodeBlock_24(ObjectCanNewExceptionTestCase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            /// <exception cref="System.Exception"></exception>
            public void Run()
            {
                _enclosing.Store(new Item());
            }
        }
    }
}