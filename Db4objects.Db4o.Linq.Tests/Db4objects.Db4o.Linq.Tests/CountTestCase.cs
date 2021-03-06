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

namespace Db4objects.Db4o.Linq.Tests
{
    public class CountTestCase : AbstractDb4oLinqTestCase
    {
        protected override void Store()
        {
            Store(new Person {Name = "Malkovitch", Age = 24});
            Store(new Person {Name = "Malkovitch", Age = 20});
            Store(new Person {Name = "Malkovitch", Age = 25});
            Store(new Person {Name = "Malkovitch", Age = 32});
            Store(new Person {Name = "Malkovitch", Age = 7});
        }

        public void TestOptimizedCount()
        {
            AssertQuery("(Person)",
                delegate
                {
                    var johns = from Person p in Db()
                        select p;

                    Assert.AreEqual(5, johns.Count());
                });
        }

        public class Person
        {
            public int Age;
            public string Name;
        }
    }
}