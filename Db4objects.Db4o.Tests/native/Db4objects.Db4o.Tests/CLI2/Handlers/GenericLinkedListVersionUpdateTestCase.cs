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

using System.Collections;
using System.Collections.Generic;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Tests.Common.Handlers;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.CLI2.Handlers
{
    internal class GenericLinkedListVersionUpdateTestCase : HandlerUpdateTestCaseBase
    {
        protected override string TypeName()
        {
            return "Generic Linked List version update";
        }

        protected override object[] CreateValues()
        {
            return new object[]
            {
                new Item<object>("baz", null, null, null, null),
                new Item<int>("foo", NewIntList(1), NewIntList(1), NewIntList(2), NewIntList(2)),
                new Item<int?>("bar", NewNullableIntList(1), NewNullableIntList(1), NewNullableIntList(2),
                    NewNullableIntList(2)),
                new Item<SimpleSubject>("foobar", NewSimpleSubjectList(1), NewSimpleSubjectList(1),
                    NewSimpleSubjectList(2), NewSimpleSubjectList(2))
            };
        }

        protected override object CreateArrays()
        {
            LinkedList<int>[] typed = {NewIntList(1), NewIntList(2)};
            ICollection<SimpleSubject>[] typedInterface = {NewSimpleSubjectList(1), NewSimpleSubjectList(2)};
            IEnumerable[] untypedEnumerable = {NewIntList(1), NewIntList(2)};
            object[] untyped = {NewSimpleSubjectList(1), NewSimpleSubjectList(2)};

            return new ItemArray(typed, typedInterface, untypedEnumerable, untyped);
        }

        protected override void AssertValues(IExtObjectContainer objectContainer, object[] values)
        {
            var expected = CreateValues();
            Assert.AreEqual(expected.Length, values.Length);

            var newObj = new SimpleSubject("new obj");
            AssertItem<object>(objectContainer, expected[0], values[0], newObj);
            AssertItem(objectContainer, expected[1], values[1], 0xCACA);
            AssertItem<int?>(objectContainer, expected[2], values[2], 0xCACA);
            AssertItem(objectContainer, expected[3], values[3], newObj);
        }

        protected override void AssertArrays(IExtObjectContainer objectContainer, object obj)
        {
            var actual = (ItemArray) obj;
            var expected = (ItemArray) CreateArrays();

            AssertItemArray(expected.typed, actual.typed);
            AssertItemArray(expected.typedInterface, actual.typedInterface);
            AssertItemArray((LinkedList<SimpleSubject>[]) expected.untyped, (LinkedList<SimpleSubject>[]) actual.untyped);
            AssertItemArray(expected.untypedEnumerable, actual.untypedEnumerable);
        }

        private static void AssertItemArray(IEnumerable[] expected, IEnumerable[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Iterator4Assert.AreEqual(expected[i].GetEnumerator(), actual[i].GetEnumerator());
            }
        }

        private void AssertItem<T>(IObjectContainer container, object objExpected, object objActual, T updateValue)
        {
            var expected = (Item<T>) objExpected;
            var actual = (Item<T>) objActual;

            Assert.AreEqual(expected.name, actual.name);

            Iterator4Assert.AreEqual(
                EnumeratorFor(expected.typed),
                EnumeratorFor(actual.typed));

            Iterator4Assert.AreEqual(
                EnumeratorFor(expected.typedInterface),
                EnumeratorFor(actual.typedInterface));

            Iterator4Assert.AreEqual(
                EnumeratorFor(expected.untyped),
                EnumeratorFor(actual.untyped));

            Iterator4Assert.AreEqual(
                EnumeratorFor(expected.untypedEnumerable),
                EnumeratorFor(actual.untypedEnumerable));


            AssertQuery(container, actual);
        }

        private bool OldTypehandler()
        {
            return Db4oHandlerVersion() < 4;
        }

        private void AssertQuery<T>(IObjectContainer container, Item<T> expected)
        {
            if (expected.typedInterface == null) return;
            if (OldTypehandler()) return;

            var query = container.Query();
            query.Constrain(typeof (Item<T>));
            query.Descend("typedInterface").Constrain(FirstElementOf(expected.typedInterface));

            var objectSet = query.Execute();
            Assert.AreEqual(1, objectSet.Count);

            var actual = (Item<T>) objectSet[0];
            Assert.AreSame(expected, actual);
        }

        private static object FirstElementOf(IEnumerable coll)
        {
            var enumerator = coll.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());

            return enumerator.Current;
        }

        private static IEnumerator EnumeratorFor(object untyped)
        {
            if (untyped == null) return null;
            return ((IEnumerable) untyped).GetEnumerator();
        }

        private static LinkedList<SimpleSubject> NewSimpleSubjectList(int n)
        {
            return n == 1
                ? new LinkedList<SimpleSubject>(new[]
                {new SimpleSubject("James P. Sullivan"), new SimpleSubject("Mike Wazowski"), new SimpleSubject("Boo")})
                : new LinkedList<SimpleSubject>(new[]
                {
                    new SimpleSubject("Randall Boggs"), new SimpleSubject("Henry J. Waternoose"),
                    new SimpleSubject("The Abominable Snowman")
                });
        }

        private static LinkedList<int?> NewNullableIntList(int n)
        {
            return n == 1
                ? new LinkedList<int?>(new int?[] {0xDB40, 0, 42})
                : new LinkedList<int?>(new int?[] {42, 0, -42});
        }

        private static LinkedList<int> NewIntList(int n)
        {
            return n == 1
                ? new LinkedList<int>(new[] {0xDB40, 0, 42})
                : new LinkedList<int>(new[] {42, 0, -42});
        }

        private class Item<T>
        {
            public readonly string name;
            public readonly LinkedList<T> typed;
            public readonly ICollection<T> typedInterface;
            public readonly object untyped;
            public readonly IEnumerable untypedEnumerable;

            public Item(string _name, LinkedList<T> _typed, ICollection<T> _typedInterface,
                IEnumerable _untypedEnumerable, object _untyped)
            {
                name = _name;
                typed = _typed;
                untyped = _untyped;
                typedInterface = _typedInterface;
                untypedEnumerable = _untypedEnumerable;
            }
        }

        private class ItemArray
        {
            public readonly LinkedList<int>[] typed;
            public readonly ICollection<SimpleSubject>[] typedInterface;
            public readonly object[] untyped;
            public readonly IEnumerable[] untypedEnumerable;

            public ItemArray(
                LinkedList<int>[] typed_,
                ICollection<SimpleSubject>[] typedInterface_,
                IEnumerable[] untypedEnumerable_,
                object[] untyped_)
            {
                typed = typed_;
                typedInterface = typedInterface_;
                untypedEnumerable = untypedEnumerable_;
                untyped = untyped_;
            }
        }

        private class SimpleSubject
        {
            public readonly string _name;

            public SimpleSubject(string name)
            {
                _name = name;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetType() != GetType()) return false;

                var rhs = (SimpleSubject) obj;

                return rhs._name == _name;
            }
        }
    }
}