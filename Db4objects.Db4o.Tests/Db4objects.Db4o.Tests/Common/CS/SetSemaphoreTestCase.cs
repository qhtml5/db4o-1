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
using Db4objects.Db4o.Config;
using Db4objects.Db4o.CS.Internal;
using Db4objects.Db4o.Events;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.IO;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen.Lang;

namespace Db4objects.Db4o.Tests.Common.CS
{
    public class SetSemaphoreTestCase : Db4oClientServerTestCase, IOptOutSolo
    {
        private static readonly string SemaphoreName = "hi";

        public static void Main(string[] args)
        {
            new SetSemaphoreTestCase().RunAll();
        }

        /// <exception cref="System.Exception"></exception>
        protected override void Configure(IConfiguration config)
        {
            config.Storage = new MemoryStorage();
        }

        public virtual void TestSemaphoreReentrancy()
        {
            var container = Db();
            Assert.IsTrue(container.SetSemaphore(SemaphoreName, 0));
            Assert.IsTrue(container.SetSemaphore(SemaphoreName, 0));
            container.ReleaseSemaphore(SemaphoreName);
        }

        public virtual void TestOwnedSemaphoreCannotBeTaken()
        {
            var client1 = OpenNewSession();
            try
            {
                Assert.IsTrue(Db().SetSemaphore(SemaphoreName, 0));
                Assert.IsFalse(client1.SetSemaphore(SemaphoreName, 0));
            }
            finally
            {
                client1.Close();
            }
        }

        public virtual void TestPreviouslyOwnedSemaphoreCannotBeTaken()
        {
            var client1 = OpenNewSession();
            try
            {
                Assert.IsTrue(Db().SetSemaphore(SemaphoreName, 0));
                Assert.IsFalse(client1.SetSemaphore(SemaphoreName, 0));
                Db().ReleaseSemaphore(SemaphoreName);
                EnsureMessageProcessed(Db());
                Assert.IsTrue(client1.SetSemaphore(SemaphoreName, 0));
                Assert.IsFalse(Db().SetSemaphore(SemaphoreName, 0));
            }
            finally
            {
                client1.Close();
            }
        }

        public virtual void TestClosingClientReleasesSemaphores()
        {
            var client1 = OpenNewSession();
            Assert.IsTrue(client1.SetSemaphore(SemaphoreName, 0));
            Assert.IsFalse(Db().SetSemaphore(SemaphoreName, 0));
            if (IsNetworking())
            {
                CloseConnectionInNetworkingCS(client1);
            }
            else
            {
                client1.Close();
            }
            Assert.IsTrue(Db().SetSemaphore(SemaphoreName, 0));
        }

        private void CloseConnectionInNetworkingCS(IExtObjectContainer client)
        {
            var eventWasRaised = new BooleanByRef();
            var clientDisconnectedLock = new Lock4();
            var serverEvents = (IObjectServerEvents) ClientServerFixture().Server
                ();
            serverEvents.ClientDisconnected += new _IEventListener4_85(clientDisconnectedLock, eventWasRaised).OnEvent;
            clientDisconnectedLock.Run(new _IClosure4_96(client, clientDisconnectedLock));
            Assert.IsTrue(eventWasRaised.value, "ClientDisconnected event was not raised.");
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TestMultipleThreads()
        {
            var clients = new IExtObjectContainer[5];
            clients[0] = Db();
            for (var i = 1; i < clients.Length; i++)
            {
                clients[i] = OpenNewSession();
            }
            Assert.IsTrue(clients[1].SetSemaphore(SemaphoreName, 50));
            var threads = new Thread[clients.Length];
            for (var i = 0; i < clients.Length; i++)
            {
                threads[i] = StartGetAndReleaseThread(clients[i]);
            }
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }
            EnsureMessageProcessed(clients[0]);
            Assert.IsTrue(clients[0].SetSemaphore(SemaphoreName, 0));
            clients[0].Close();
            threads[2] = StartGetAndReleaseThread(clients[2]);
            threads[1] = StartGetAndReleaseThread(clients[1]);
            threads[1].Join();
            threads[2].Join();
            for (var i = 1; i < clients.Length - 1; i++)
            {
                clients[i].Close();
            }
            clients[4].SetSemaphore(SemaphoreName, 1000);
            clients[4].Close();
        }

        private Thread StartGetAndReleaseThread(IExtObjectContainer client)
        {
            var t = new Thread(new GetAndRelease(client), "SetSemaphoreTestCase.startGetAndReleaseThread"
                );
            t.Start();
            return t;
        }

        private static void EnsureMessageProcessed(IExtObjectContainer client)
        {
            client.Commit();
        }

        private sealed class _IEventListener4_85
        {
            private readonly Lock4 clientDisconnectedLock;
            private readonly BooleanByRef eventWasRaised;

            public _IEventListener4_85(Lock4 clientDisconnectedLock, BooleanByRef eventWasRaised
                )
            {
                this.clientDisconnectedLock = clientDisconnectedLock;
                this.eventWasRaised = eventWasRaised;
            }

            public void OnEvent(object sender, StringEventArgs args)
            {
                clientDisconnectedLock.Run(new _IClosure4_87(eventWasRaised, clientDisconnectedLock
                    ));
            }

            private sealed class _IClosure4_87 : IClosure4
            {
                private readonly Lock4 clientDisconnectedLock;
                private readonly BooleanByRef eventWasRaised;

                public _IClosure4_87(BooleanByRef eventWasRaised, Lock4 clientDisconnectedLock)
                {
                    this.eventWasRaised = eventWasRaised;
                    this.clientDisconnectedLock = clientDisconnectedLock;
                }

                public object Run()
                {
                    eventWasRaised.value = true;
                    clientDisconnectedLock.Awake();
                    return null;
                }
            }
        }

        private sealed class _IClosure4_96 : IClosure4
        {
            private readonly IExtObjectContainer client;
            private readonly Lock4 clientDisconnectedLock;

            public _IClosure4_96(IExtObjectContainer client, Lock4 clientDisconnectedLock)
            {
                this.client = client;
                this.clientDisconnectedLock = clientDisconnectedLock;
            }

            public object Run()
            {
                client.Close();
                clientDisconnectedLock.Snooze(30000);
                return null;
            }
        }

        internal class GetAndRelease : IRunnable
        {
            private readonly IExtObjectContainer _client;

            public GetAndRelease(IExtObjectContainer client)
            {
                _client = client;
            }

            public virtual void Run()
            {
                Assert.IsTrue(_client.SetSemaphore(SemaphoreName, 50000));
                EnsureMessageProcessed(_client);
                _client.ReleaseSemaphore(SemaphoreName);
            }
        }
    }
}

#endif // !SILVERLIGHT