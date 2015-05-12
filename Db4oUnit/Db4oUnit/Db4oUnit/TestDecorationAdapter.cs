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

using Db4objects.Db4o.Foundation;

namespace Db4oUnit
{
    public class TestDecorationAdapter : ITest
    {
        private readonly ITest _test;

        public TestDecorationAdapter(ITest test)
        {
            _test = test;
        }

        public virtual string Label()
        {
            return _test.Label();
        }

        public virtual void Run()
        {
            _test.Run();
        }

        public virtual bool IsLeafTest()
        {
            return _test.IsLeafTest();
        }

        public virtual ITest Transmogrify(IFunction4 fun)
        {
            return ((ITest) fun.Apply(this));
        }
    }
}