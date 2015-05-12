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
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.Foundation
{
    public class BitMap4TestCase : ITestCase
    {
        public virtual void Test()
        {
            var buffer = new byte[100];
            for (var i = 0; i < 17; i++)
            {
                var map = new BitMap4(i);
                map.WriteTo(buffer, 11);
                var reReadMap = new BitMap4(buffer, 11, i);
                for (var j = 0; j < i; j++)
                {
                    TBit(map, j);
                    TBit(reReadMap, j);
                }
            }
        }

        private void TBit(BitMap4 map, int bit)
        {
            map.SetTrue(bit);
            Assert.IsTrue(map.IsTrue(bit));
            map.SetFalse(bit);
            Assert.IsFalse(map.IsTrue(bit));
            map.SetTrue(bit);
            Assert.IsTrue(map.IsTrue(bit));
        }
    }
}