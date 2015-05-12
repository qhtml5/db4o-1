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

#if !SILVERLIGHT
using System;
using Db4objects.Db4o.Query;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class ReferenceSystemIsolationTestCase : EmbeddedAndNetworkingClientTestCaseBase
    {
        public virtual void Test()
        {
            var item = new Item
                ();
            NetworkingClient().Store(item);
            var id = (int) NetworkingClient().GetID(item);
            var query = NetworkingClient().Query();
            query.Constrain(typeof (Item));
            query.Constrain(new IncludeAllEvaluation());
            query.Execute();
            Assert.IsNull(EmbeddedClient().Transaction.ReferenceForId(id));
        }

        [Serializable]
        public sealed class IncludeAllEvaluation : IEvaluation
        {
            public void Evaluate(ICandidate candidate)
            {
                candidate.Include(true);
            }
        }

        public class Item
        {
        }
    }
}

#endif // !SILVERLIGHT