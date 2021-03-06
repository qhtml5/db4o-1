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

using System;
using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Foundation.IO;
using Db4objects.Db4o.Tests.Util;
using Db4oUnit;
using Db4oUnit.Extensions;
using Db4oUnit.Extensions.Fixtures;
using Sharpen;

namespace Db4objects.Db4o.Tests.Common.Handlers
{
    public abstract partial class FormatMigrationTestCaseBase : ITestLifeCycle, IOptOutNoFileSystemData
        , IOptOutMultiSession, IOptOutWorkspaceIssue
    {
        private static readonly string Username = "db4o";
        private static readonly string Password = Username;
        private byte _db4oHeaderVersion;
        private string _db4oVersion;

        private string DatabasePath
        {
            get { return Path.Combine(GetTempPath(), "test/db4oVersions"); }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void SetUp()
        {
            Configure();
            CreateDatabase();
        }

        /// <exception cref="System.Exception"></exception>
        public virtual void TearDown()
        {
            Deconfigure();
        }

        public virtual void Configure()
        {
            var config = Db4oFactory.Configure();
            config.AllowVersionUpdates(true);
            ConfigureForTest(config);
        }

        private void Deconfigure()
        {
            var config = Db4oFactory.Configure();
            config.AllowVersionUpdates(false);
            DeconfigureForTest(config);
        }

        public virtual void CreateDatabase()
        {
            CreateDatabase(FileName());
        }

        public virtual void CreateDatabaseFor(string versionName)
        {
            _db4oVersion = versionName;
            var config = Db4oFactory.Configure();
            try
            {
                ConfigureForStore(config);
            }
            catch
            {
            }
            // Some old database engines may throw NoSuchMethodError
            // for configuration methods they don't know yet. Ignore,
            // but tell the implementor:
            // System.out.println("Exception in configureForStore for " + versionName + " in " + getClass().getName());
            try
            {
                CreateDatabase(FileName(versionName));
            }
            finally
            {
                DeconfigureForStore(config);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Test()
        {
            for (var i = 0; i < VersionNames().Length; i++)
            {
                var versionName = VersionNames()[i];
                Test(versionName);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Test(string versionName)
        {
            _db4oVersion = versionName;
            if (!IsApplicableForDb4oVersion())
            {
                return;
            }
            var testFileName = FileName(versionName);
            if (!File.Exists(testFileName))
            {
                Runtime.Out.WriteLine("Version upgrade check failed. File not found:" + testFileName
                    );
                // FIXME: The following fails the CC build since not all files are there on .NET.
                //        Change back when we have all files.
                // Assert.fail("Version upgrade check failed. File not found:" + testFileName);
                return;
            }
            //      System.out.println("Checking database file: " + testFileName);
            InvestigateFileHeaderVersion(testFileName);
            PrepareClientServerTest(testFileName);
            try
            {
                RunDeletionTests(testFileName);
                DefragmentSoloAndCS(testFileName);
                CheckDatabaseFile(testFileName);
                // Twice, to ensure everything is fine after opening, converting and closing.
                CheckDatabaseFile(testFileName);
                UpdateDatabaseFile(testFileName);
                CheckUpdatedDatabaseFile(testFileName);
                DefragmentSoloAndCS(testFileName);
                CheckUpdatedDatabaseFile(testFileName);
            }
            finally
            {
                TearDownClientServer(testFileName);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void DefragmentSoloAndCS(string fileName)
        {
            RunDefrag(fileName);
            RunDefrag(ClientServerFileName(fileName));
        }

        private void TearDownClientServer(string testFileName)
        {
            File4.Delete(ClientServerFileName(testFileName));
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void PrepareClientServerTest(string fileName)
        {
            File4.Copy(fileName, ClientServerFileName(fileName));
        }

        private string ClientServerFileName(string fileName)
        {
            return fileName + ".CS";
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void RunDeletionTests(string testFileName)
        {
            WithDatabase(testFileName, new _IFunction4_152(this));
            CheckDatabaseFile(testFileName);
        }

        /// <summary>Override to provide tests for deletion.</summary>
        /// <remarks>Override to provide tests for deletion.</remarks>
        protected virtual void AssertObjectDeletion(IExtObjectContainer objectContainer)
        {
        }

        /// <summary>Can be overridden to disable the test for specific db4o versions.</summary>
        /// <remarks>Can be overridden to disable the test for specific db4o versions.</remarks>
        protected virtual bool IsApplicableForDb4oVersion()
        {
            return true;
        }

        private void CheckDatabaseFile(string testFile)
        {
            WithDatabase(testFile, new _IFunction4_174(this));
        }

        private void CheckUpdatedDatabaseFile(string testFile)
        {
            WithDatabase(testFile, new _IFunction4_183(this));
        }

        private void CreateDatabase(string file)
        {
            if (!IsApplicableForDb4oVersion())
            {
                return;
            }
            Directory.CreateDirectory(DatabasePath);
            if (File.Exists(file))
            {
                File4.Delete(file);
            }
            var objectContainer = Db4oFactory.OpenFile(file).Ext();
            var adapter = ObjectContainerAdapterFactory.ForVersion(Db4oMajorVersion
                (), Db4oMinorVersion()).ForContainer(objectContainer);
            try
            {
                Store(adapter);
            }
            finally
            {
                objectContainer.Close();
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void InvestigateFileHeaderVersion(string testFile)
        {
            _db4oHeaderVersion = VersionServices.FileHeaderVersion(testFile);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void RunDefrag(string testFileName)
        {
            var config = Db4oFactory.NewConfiguration();
            config.AllowVersionUpdates(true);
            ConfigureForTest(config);
            var oc = Db4oFactory.OpenFile(config, testFileName);
            oc.Close();
            var backupFileName = Path.GetTempFileName();
            try
            {
                var defragConfig = new DefragmentConfig(testFileName, backupFileName
                    );
                defragConfig.ForceBackupDelete(true);
                ConfigureForTest(defragConfig.Db4oConfig());
                defragConfig.ReadOnly(!DefragmentInReadWriteMode());
                Db4o.Defragment.Defragment.Defrag(defragConfig);
            }
            finally
            {
                File4.Delete(backupFileName);
            }
        }

        private void UpdateDatabaseFile(string testFile)
        {
            WithDatabase(testFile, new _IFunction4_247(this));
        }

        private void WithDatabase(string file, IFunction4 function)
        {
            Configure();
            var objectContainer = Db4oFactory.OpenFile(file).Ext();
            try
            {
                function.Apply(objectContainer);
            }
            finally
            {
                objectContainer.Close();
            }
            var server = Db4oFactory.OpenServer(ClientServerFileName(file), -1);
            server.GrantAccess(Username, Password);
            objectContainer = Db4oFactory.OpenClient("localhost", server.Ext().Port(), Username
                , Password).Ext();
            try
            {
                function.Apply(objectContainer);
            }
            finally
            {
                objectContainer.Close();
                server.Close();
            }
        }

        protected abstract void AssertObjectsAreReadable(IExtObjectContainer objectContainer
            );

        protected virtual void AssertObjectsAreUpdated(IExtObjectContainer objectContainer
            )
        {
        }

        // Override to check updates also
        protected virtual void ConfigureForStore(IConfiguration config)
        {
        }

        // Override for special storage configuration.
        protected virtual void ConfigureForTest(IConfiguration config)
        {
        }

        // Override for special testing configuration.
        protected virtual byte Db4oHeaderVersion()
        {
            return _db4oHeaderVersion;
        }

        protected virtual int Db4oMajorVersion()
        {
            if (_db4oVersion != null)
            {
                return Convert.ToInt32(Runtime.Substring(_db4oVersion, 0, 1));
            }
            return Convert.ToInt32(Runtime.Substring(Db4oFactory.Version(), 5,
                6));
        }

        protected virtual int Db4oMinorVersion()
        {
            if (_db4oVersion != null)
            {
                return Convert.ToInt32(Runtime.Substring(_db4oVersion, 2, 3));
            }
            return Convert.ToInt32(Runtime.Substring(Db4oFactory.Version(), 7,
                8));
        }

        /// <summary>
        ///     override and return true for database updates that produce changed class metadata
        /// </summary>
        protected virtual bool DefragmentInReadWriteMode()
        {
            return false;
        }

        protected virtual string FileName()
        {
            _db4oVersion = Db4oVersion.Name;
            return FileName(_db4oVersion);
        }

        protected virtual string FileName(string versionName)
        {
            return OldVersionFileName(versionName) + ".db4o";
        }

        protected virtual void DeconfigureForStore(IConfiguration config)
        {
        }

        // Override for special storage deconfiguration.
        protected virtual void DeconfigureForTest(IConfiguration config)
        {
        }

        // Override for special storage deconfiguration.
        protected abstract string FileNamePrefix();

        protected virtual string OldVersionFileName(string versionName)
        {
            return Path.Combine(DatabasePath, FileNamePrefix() + versionName.Replace(' ', '_'
                ));
        }

        protected abstract void Store(IObjectContainerAdapter objectContainer);

        protected virtual void Update(IExtObjectContainer objectContainer)
        {
        }

        // Override to do updates also
        protected virtual string[] VersionNames()
        {
            return new[] {Runtime.Substring(Db4oFactory.Version(), 5)};
        }

        private sealed class _IFunction4_152 : IFunction4
        {
            private readonly FormatMigrationTestCaseBase _enclosing;

            public _IFunction4_152(FormatMigrationTestCaseBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object db)
            {
                _enclosing.AssertObjectDeletion(((IObjectContainer) db).Ext());
                return null;
            }
        }

        private sealed class _IFunction4_174 : IFunction4
        {
            private readonly FormatMigrationTestCaseBase _enclosing;

            public _IFunction4_174(FormatMigrationTestCaseBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object objectContainer)
            {
                _enclosing.AssertObjectsAreReadable((IExtObjectContainer) objectContainer);
                return null;
            }
        }

        private sealed class _IFunction4_183 : IFunction4
        {
            private readonly FormatMigrationTestCaseBase _enclosing;

            public _IFunction4_183(FormatMigrationTestCaseBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object objectContainer)
            {
                _enclosing.AssertObjectsAreUpdated((IExtObjectContainer) objectContainer);
                return null;
            }
        }

        private sealed class _IFunction4_247 : IFunction4
        {
            private readonly FormatMigrationTestCaseBase _enclosing;

            public _IFunction4_247(FormatMigrationTestCaseBase _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public object Apply(object objectContainer)
            {
                _enclosing.Update((IExtObjectContainer) objectContainer);
                return null;
            }
        }
    }
}