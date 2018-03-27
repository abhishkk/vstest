// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TestPlatform.Common.UnitTests.ExtensionFramework.Utilities
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    
    [TestClass]
    public class TestDiscovererPluginInformationTests
    {
        private TestDiscovererPluginInformation testPluginInformation;
        
        [TestMethod]
        public void AssemblyQualifiedNameShouldReturnTestExtensionTypesName()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithNoFileExtensions));
            Assert.AreEqual(typeof(DummyTestDiscovererWithNoFileExtensions).AssemblyQualifiedName, this.testPluginInformation.AssemblyQualifiedName);
        }

        [TestMethod]
        public void IdentifierDataShouldReturnTestExtensionTypesName()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithNoFileExtensions));
            Assert.AreEqual(typeof(DummyTestDiscovererWithNoFileExtensions).AssemblyQualifiedName, this.testPluginInformation.IdentifierData);
        }

        [TestMethod]
        public void FileExtensionsShouldReturnEmptyListIfADiscovererSupportsNoFileExtensions()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithNoFileExtensions));
            Assert.IsNotNull(this.testPluginInformation.FileExtensions);
            Assert.AreEqual(0, this.testPluginInformation.FileExtensions.Count);
        }

        [TestMethod]
        public void FileExtensionsShouldReturnAFileExtensionForADiscoverer()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithOneFileExtensions));
            CollectionAssert.AreEqual(new List<string> { "csv"}, this.testPluginInformation.FileExtensions);
        }

        [TestMethod]
        public void FileExtensionsShouldReturnSupportedFileExtensionsForADiscoverer()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithTwoFileExtensions));
            CollectionAssert.AreEqual(new List<string> {"csv", "docx"}, this.testPluginInformation.FileExtensions);
        }

        // TODO: Unit tests for assembly type as well.

        [TestMethod]
        public void DefaultExecutorUriShouldReturnEmptyListIfADiscovererDoesNotHaveOne()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithNoFileExtensions));
            Assert.IsNotNull(this.testPluginInformation.DefaultExecutorUri);
            Assert.AreEqual(string.Empty, this.testPluginInformation.DefaultExecutorUri);
        }

        [TestMethod]
        public void DefaultExecutorUriShouldReturnDefaultExecutorUriOfADiscoverer()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithOneFileExtensions));
            Assert.AreEqual("csvexecutor", this.testPluginInformation.DefaultExecutorUri);
        }

        [TestMethod]
        public void MetadataShouldReturnFileExtensionsAndDefaultExecutorUri()
        {
            this.testPluginInformation = new TestDiscovererPluginInformation(typeof(DummyTestDiscovererWithTwoFileExtensions));

            var expectedFileExtensions = new List<string> { "csv", "docx" };
            var testPluginMetada = this.testPluginInformation.Metadata.ToArray();

            CollectionAssert.AreEqual(expectedFileExtensions, (testPluginMetada[0] as List<string>).ToArray());
            Assert.AreEqual("csvexecutor", testPluginMetada[1] as string);
        }
    }

    #region Implementation
    
    public class DummyTestDiscovererWithNoFileExtensions
    {
    }

    [FileExtension("csv")]
    [DefaultExecutorUri("csvexecutor")]
    public class DummyTestDiscovererWithOneFileExtensions
    {
    }

    [FileExtension("csv")]
    [FileExtension("docx")]
    [DefaultExecutorUri("csvexecutor")]
    public class DummyTestDiscovererWithTwoFileExtensions
    {
    }

    #endregion
}
