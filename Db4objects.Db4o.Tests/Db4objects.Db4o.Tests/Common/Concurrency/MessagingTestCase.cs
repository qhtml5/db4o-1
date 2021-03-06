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
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Messaging;
using Db4objects.Db4o.Tests.Common.CS;
using Db4oUnit;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.Concurrency
{
    public class MessagingTestCase : ClientServerTestCaseBase
    {
        public static readonly object Lock = new object();
        public TestMessageRecipient _recipient;

        public MessagingTestCase()
        {
            _recipient = new TestMessageRecipient(ThreadCount());
        }

        public static void Main(string[] args)
        {
            new MessagingTestCase().RunConcurrency();
        }

        public virtual void Conc(IExtObjectContainer oc, int seq)
        {
            IMessageSender sender = null;
            // Configuration is not threadsafe.
            lock (Lock)
            {
                Server().Ext().Configure().ClientServer().SetMessageRecipient(_recipient);
                sender = oc.Configure().ClientServer().GetMessageSender();
            }
            sender.Send(new Data(seq));
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void Check(IExtObjectContainer oc)
        {
            Thread.Sleep(1000);
            _recipient.Check();
        }

        public class TestMessageRecipient : IMessageRecipient
        {
            public bool[] processed;
            public int seq;

            public TestMessageRecipient(int threadCount)
            {
                processed = new bool[threadCount];
            }

            public virtual void ProcessMessage(IMessageContext con, object message)
            {
                Assert.IsTrue(message is Data);
                var value = ((Data) message).value;
                processed[value] = true;
            }

            public virtual void Check()
            {
                for (var i = 0; i < processed.Length; ++i)
                {
                    Assert.IsTrue(processed[i]);
                }
            }
        }

        public class Data
        {
            public int value;

            public Data(int seq)
            {
                value = seq;
            }
        }
    }
}

#endif // !SILVERLIGHT