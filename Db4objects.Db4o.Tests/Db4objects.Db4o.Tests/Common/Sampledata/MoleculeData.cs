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

namespace Db4objects.Db4o.Tests.Common.Sampledata
{
    public class MoleculeData : AtomData
    {
        public MoleculeData()
        {
        }

        public MoleculeData(AtomData child) : base(child)
        {
        }

        public MoleculeData(string name) : base(name)
        {
        }

        public MoleculeData(AtomData child, string name) : base(child, name)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is MoleculeData)
            {
                return base.Equals(obj);
            }
            return false;
        }

        public override string ToString()
        {
            var str = "Molecule(" + name + ")";
            if (child != null)
            {
                return str + "." + child;
            }
            return str;
        }
    }
}