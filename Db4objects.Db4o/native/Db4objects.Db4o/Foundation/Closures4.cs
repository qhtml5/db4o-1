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

namespace Db4objects.Db4o.Foundation
{
    internal class Closures4
    {
        public delegate object Closure();

        public static IClosure4 ForDelegate(Closure @delegate)
        {
            return new Closure4OverDelegate(@delegate);
        }

        internal class Closure4OverDelegate : IClosure4
        {
            private readonly Closure _delegate;

            public Closure4OverDelegate(Closure @delegate)
            {
                _delegate = @delegate;
            }

            public object Run()
            {
                return _delegate();
            }
        }
    }
}