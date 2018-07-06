﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.Extensions.TrxLogger.UnitTests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Microsoft.TestPlatform.Extensions.TrxLogger.Utility;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ObjectModel;
    using ObjectModel = Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using TestOutcome = VisualStudio.TestPlatform.ObjectModel.TestOutcome;
    using TrxLoggerConstants = Microsoft.TestPlatform.Extensions.TrxLogger.Utility.Constants;
    using TrxLoggerOutcome = Microsoft.TestPlatform.Extensions.TrxLogger.ObjectModel.TestOutcome;
    using UriDataAttachment = VisualStudio.TestPlatform.ObjectModel.UriDataAttachment;

    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void ToOutcomeShouldMapFailedToFailed()
        {
            Assert.AreEqual(TrxLoggerOutcome.Failed, Converter.ToOutcome(TestOutcome.Failed));
        }

        [TestMethod]
        public void ToOutcomeShouldMapPassedToPassed()
        {
            Assert.AreEqual(TrxLoggerOutcome.Passed, Converter.ToOutcome(TestOutcome.Passed));
        }

        [TestMethod]
        public void ToOutcomeShouldMapSkippedToNotExecuted()
        {
            Assert.AreEqual(TrxLoggerOutcome.NotExecuted, Converter.ToOutcome(TestOutcome.Skipped));
        }

        [TestMethod]
        public void ToOutcomeShouldMapNoneToNotExecuted()
        {
            Assert.AreEqual(TrxLoggerOutcome.NotExecuted, Converter.ToOutcome(TestOutcome.None));
        }

        [TestMethod]
        public void ToOutcomeShouldMapNotFoundToNotExecuted()
        {
            Assert.AreEqual(TrxLoggerOutcome.NotExecuted, Converter.ToOutcome(TestOutcome.NotFound));
        }

        [TestMethod]
        public void ToCollectionEntriesShouldRenameAttachmentUriIfTheAttachmentNameIsSame()
        {
            ConverterTests.SetupForToCollectionEntries(out var tempDir, out var attachmentSets, out var testRun, out var testResultsDirectory);

            List<CollectorDataEntry> collectorDataEntries = Converter.ToCollectionEntries(attachmentSets, testRun, testResultsDirectory);

            Assert.AreEqual($@"{Environment.MachineName}\123.coverage", ((ObjectModel.UriDataAttachment) collectorDataEntries[0].Attachments[0]).Uri.OriginalString);
            Assert.AreEqual($@"{Environment.MachineName}\123[1].coverage", ((ObjectModel.UriDataAttachment)collectorDataEntries[0].Attachments[1]).Uri.OriginalString);

            Directory.Delete(tempDir, true);
        }

        /// <summary>
        /// Unit test for assigning or populating test categories read to the unit test element.
        /// </summary>
        [TestMethod]
        public void ToTestElementShouldAssignTestCategoryOfUnitTestElement()
        {
            ObjectModel.TestCase testCase = CreateTestCase("TestCase1");
            ObjectModel.TestResult result = new ObjectModel.TestResult(testCase);
            TestProperty testProperty = TestProperty.Register("MSTestDiscoverer.TestCategory", "String array property", string.Empty, string.Empty, typeof(string[]), null, TestPropertyAttributes.Hidden, typeof(TestObject));

            testCase.SetPropertyValue(testProperty, new[] { "AsmLevel", "ClassLevel", "MethodLevel" });

            var unitTestElement = Converter.ToTestElement(testCase.Id, Guid.Empty, Guid.Empty, testCase.DisplayName, TrxLoggerConstants.UnitTestType, testCase);

            object[] expected = new[] { "MethodLevel", "ClassLevel", "AsmLevel" };

            CollectionAssert.AreEqual(expected, unitTestElement.TestCategories.ToArray().OrderByDescending(x => x.ToString()).ToArray());
        }

        /// <summary>
        /// Unit test for regression when there's no test categories.
        /// </summary>
        [TestMethod]
        public void ToTestElementShouldNotFailWhenThereIsNoTestCategoreis()
        {
            ObjectModel.TestCase testCase = CreateTestCase("TestCase1");
            ObjectModel.TestResult result = new ObjectModel.TestResult(testCase);

            var unitTestElement = Converter.ToTestElement(testCase.Id, Guid.Empty, Guid.Empty, testCase.DisplayName, TrxLoggerConstants.UnitTestType, testCase);

            object[] expected = Enumerable.Empty<Object>().ToArray();

            CollectionAssert.AreEqual(expected, unitTestElement.TestCategories.ToArray());
        }

        [TestMethod]
        public void ToTestElementShouldContainExpectedTestMethodPropertiesIfFqnEndsWithTestName()
        {
            var expectedClassName = "TestProject1.Class1";
            var fullyQualifiedName = expectedClassName + "." + "TestMethod1(2, 3, 4.0d)";
            var testName = "TestMethod1(2, 3, 4.0d)";

            ValidateTestMethodProperties(testName, fullyQualifiedName, expectedClassName);
        }

        [TestMethod]
        public void ToTestElementShouldContainExpectedTestMethodPropertiesIfFqnDoesNotEndsWithTestName()
        {
            var expectedClassName = "TestProject1.Class1.TestMethod1(2, 3, 4";
            var fullyQualifiedName = "TestProject1.Class1.TestMethod1(2, 3, 4.0d)";
            var testName = "TestMethod1";

            ValidateTestMethodProperties(testName, fullyQualifiedName, expectedClassName);
        }

        private void ValidateTestMethodProperties(string testName, string fullyQualifiedName, string expectedClassName)
        {
            ObjectModel.TestCase testCase = CreateTestCase(fullyQualifiedName);
            ObjectModel.TestResult result = new ObjectModel.TestResult(testCase);

            var unitTestElement = Converter.ToTestElement(testCase.Id, Guid.Empty, Guid.Empty, testName, TrxLoggerConstants.UnitTestType, testCase) as UnitTestElement;

            Assert.AreEqual(expectedClassName, unitTestElement.TestMethod.ClassName);
            Assert.AreEqual(testName, unitTestElement.TestMethod.Name);
        }

        private static TestCase CreateTestCase(string fullyQualifiedName)
        {
            return new ObjectModel.TestCase(fullyQualifiedName, new Uri("some://uri"), "DummySourceFileName");
        }

        private static void SetupForToCollectionEntries(out string tempDir, out List<AttachmentSet> attachmentSets, out TestRun testRun,
            out string testResultsDirectory)
        {
            ConverterTests.CreateTempCoverageFiles(out tempDir, out var coverageFilePath1, out var coverageFilePath2);

            UriDataAttachment uriDataAttachment1 =
                new UriDataAttachment(new Uri($"file:///{coverageFilePath1}"), "Description 1");
            UriDataAttachment uriDataAttachment2 =
                new UriDataAttachment(new Uri($"file:///{coverageFilePath2}"), "Description 2");
            attachmentSets = new List<AttachmentSet>
            {
                new AttachmentSet(new Uri("datacollector://microsoft/CodeCoverage/2.0"), "Code Coverage")
            };

            testRun = new TestRun(Guid.NewGuid());
            testRun.RunConfiguration = new TestRunConfiguration("Testrun 1");
            attachmentSets[0].Attachments.Add(uriDataAttachment1);
            attachmentSets[0].Attachments.Add(uriDataAttachment2);
            testResultsDirectory = Path.Combine(tempDir, "TestResults");
        }

        private static void CreateTempCoverageFiles(out string tempDir, out string coverageFilePath1,
            out string coverageFilePath2)
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var covDir1 = Path.Combine(tempDir, Guid.NewGuid().ToString());
            var covDir2 = Path.Combine(tempDir, Guid.NewGuid().ToString());

            Directory.CreateDirectory(covDir1);
            Directory.CreateDirectory(covDir2);

            coverageFilePath1 = Path.Combine(covDir1, "123.coverage");
            coverageFilePath2 = Path.Combine(covDir2, "123.coverage");

            File.WriteAllText(coverageFilePath1, string.Empty);
            File.WriteAllText(coverageFilePath2, string.Empty);
        }
    }
}
