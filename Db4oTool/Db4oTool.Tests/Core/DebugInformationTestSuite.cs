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
using Db4oTool.Core;
using Db4oUnit;
using Db4oUnit.Fixtures;
using Mono.Cecil.Cil;

namespace Db4oTool.Tests.Core
{
    internal class DebugInformationTestSuite : FixtureBasedTestSuite
    {
        public override Type[] TestUnits()
        {
            return new[] {typeof (DebugInformationTestUnit)};
        }

        public override IFixtureProvider[] FixtureProviders()
        {
            return new[]
            {
                DebugInformationTestVariables.SourceFixtureProvider,
                DebugInformationTestVariables.DebugSymbolsFixtureProvider
            };
        }
    }

    internal class DebugInformationTestUnit : ITestCase
    {
        private const string ResourceName = "DebugInformationSubject";

        private static Action<string> SourceHandler
        {
            get
            {
                Action<string> deleteFile = delegate(string path) { File.Delete(path); };
                Action<string> doNothing = delegate { };

                return DebugInformationTestVariables.TestWithSourceAvailable()
                    ? doNothing
                    : deleteFile;
            }
        }

        public void TestSimpleSourceLine()
        {
            AssertLineInformation("SimpleSourceLine", "o.MethodCall(10)");
        }

        public void TestSimpleIfBoby()
        {
            AssertLineInformation("SimpleIfBody", "o.MethodCall(10)");
        }

        public void TestIfAndElseBranch()
        {
            AssertLineInformation("IfAndElseBranch", "o.MethodCall(10)");
        }

        public void TestElseBranch()
        {
            AssertLineInformation("ElseBranch", "o.MethodCall(10)");
        }

        public void TestAssignmentExpressionAndComparison()
        {
            AssertLineInformation("AssignmentExpressionAndComparison", "if ( (v = o.MethodCall(10)) > 1 ");
        }

        public void TestTryBody()
        {
            AssertLineInformation("TryBody", "o.MethodCall(10)");
        }

        public void TestCatchBody()
        {
            AssertLineInformation("CatchBody", "o.MethodCall(10)");
        }

        private static void AssertLineInformation(string methodName, string expected)
        {
            var assembly = Db4oToolTestServices.AssemblyFromResource(ResourceName, typeof (DebugInformationTestSuite),
                DebugInformationTestVariables.TestWithDebugSymbolsAvailable(), SourceHandler);

            var method = ReflectionServices.FindMethod(assembly, ResourceName, methodName);
            var instruction = ReflectionServices.FindInstruction(method, OpCodes.Callvirt);

            var actual = DebugInformation.InstructionInformationFor(instruction, method.Body.Instructions);

            var fixedExpectation = FixExpectation(expected);
            Assert.IsTrue(actual.Contains(fixedExpectation),
                string.Format("Expected: {0}, Actual: {1}", fixedExpectation, actual));
        }

        private static string FixExpectation(string expectation)
        {
            if (!DebugInformationTestVariables.TestWithDebugSymbolsAvailable())
                return string.Format("{0}", OpCodes.Callvirt);
            return DebugInformationTestVariables.TestWithSourceAvailable()
                ? expectation
                : string.Format("{0}.cs", ResourceName);
        }
    }
}