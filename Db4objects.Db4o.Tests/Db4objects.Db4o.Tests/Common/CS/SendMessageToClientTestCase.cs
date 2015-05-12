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
using Db4objects.Db4o.CS.Internal.Messages;
using Db4oUnit;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class SendMessageToClientTestCase : ClientServerTestCaseBase
    {
        public static void Main(string[] args)
        {
            new SendMessageToClientTestCase().RunNetworking();
        }

        public virtual void Test()
        {
            if (IsEmbedded())
            {
                // No sending messages back and forth on MTOC.
                return;
            }
            ServerDispatcher().Write(Msg.Ok);
            var msg = Client().GetResponse();
            Assert.AreEqual(Msg.Ok, msg);
        }
    }
}

#endif // !SILVERLIGHT