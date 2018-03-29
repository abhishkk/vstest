// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ObjectModel;

    /// <summary>
    /// The test discoverer plugin information.
    /// </summary>
    internal class TestDiscovererPluginInformation : TestPluginInformation
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="testDiscovererType">Data type of the test discoverer</param>
        public TestDiscovererPluginInformation(Type testDiscovererType)
            : base(testDiscovererType)
        {
            if (testDiscovererType != null)
            {
                this.FileExtensions = GetFileExtensions(testDiscovererType);
                this.DefaultExecutorUri = GetDefaultExecutorUri(testDiscovererType);
                // TODO: Ideally we should take this param along with FileExtensions as second param of the constructor.

                if (this.AssemblyType == AssemblyType.Unknown)
                {
                    this.AssemblyType = GetAssemblyType(testDiscovererType); // TODO: remove this if we are not supporting this.
                }
            }
        }

        /// <summary>
        /// Metadata for the test plugin
        /// </summary>
        public override ICollection<Object> Metadata
        {
            get
            {
                return new object[] { this.FileExtensions, this.DefaultExecutorUri, this.AssemblyType };
            }
        }

        /// <summary>
        /// Gets collection of file extensions supported by discoverer plugin.
        /// </summary>
        public List<string> FileExtensions
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Uri identifying the executor
        /// </summary>
        public string DefaultExecutorUri
        {
            get;
            private set;
        }

        // TODO: AssemblyType should come from some enum which has only two values, native and managed.
        public AssemblyType AssemblyType
        {
            get;
            private set;
        }

        /// <summary>
        /// Helper to get file extensions from the FileExtensionAttribute on the discover plugin.
        /// </summary>
        /// <param name="testDicovererType">Data type of the test discoverer</param>
        /// <returns>List of file extensions</returns>
        private List<string> GetFileExtensions(Type testDicovererType)
        {
            // ***TODO***: IMPORTANT: In run tests directly, discovery happens but code is not coming here. Check where is the code going.
            var fileExtensions = new List<string>();
            
            var attributes = testDicovererType.GetTypeInfo().GetCustomAttributes(typeof(FileExtensionAttribute), false).ToArray();
            if (attributes != null && attributes.Length > 0)
            {
                foreach (var attribute in attributes)
                {
                    var fileExtAttribute = (FileExtensionAttribute)attribute;
                    if (!string.IsNullOrEmpty(fileExtAttribute.FileExtension))
                    {
                        if (!SetAssemblyTypeIfExtensionMatches(fileExtAttribute.FileExtension))
                        {
                            fileExtensions.Add(fileExtAttribute.FileExtension);
                        }
                    }
                }
            }

            return fileExtensions;
        }

        // TODO: As we are doing hack here. Write comment properly.
        private bool SetAssemblyTypeIfExtensionMatches(string fileExtension)
        {
            // TODO: write this section properly and move string to const.
            if ("_native_".Equals(fileExtension))
            {
                if (this.AssemblyType == AssemblyType.Unknown)
                {
                    this.AssemblyType = AssemblyType.Native;
                }
                return true;
            }
            else if ("_managed_".Equals(fileExtension))
            {
                if (this.AssemblyType == AssemblyType.Unknown)
                {
                    this.AssemblyType = AssemblyType.Managed;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the value of default executor Uri on this type. 'Null' if not present.
        /// </summary>
        /// <param name="testDiscovererType"> The test discoverer Type. </param>
        /// <returns> The default executor URI. </returns>
        private string GetDefaultExecutorUri(Type testDiscovererType)
        {
            string result = string.Empty;
            
            object[] attributes = testDiscovererType.GetTypeInfo().GetCustomAttributes(typeof(DefaultExecutorUriAttribute), false).ToArray();
            if (attributes != null && attributes.Length > 0)
            {
                DefaultExecutorUriAttribute executorUriAttribute = (DefaultExecutorUriAttribute)attributes[0];

                if (!string.IsNullOrEmpty(executorUriAttribute.ExecutorUri))
                {
                    result = executorUriAttribute.ExecutorUri;
                }
            }

            return result;
        }

        private AssemblyType GetAssemblyType(Type testDiscovererType)
        {
            var assemblyType = default(AssemblyType);

            // TODO: Instead of string, if we can take enum as attribute, prefer that.
            string result = string.Empty;

            object[] attributes = testDiscovererType.GetTypeInfo().GetCustomAttributes(typeof(AssemblyTypeAttribute), false).ToArray();
            if (attributes != null && attributes.Length > 0)
            {
                AssemblyTypeAttribute assemblyTypeAttribute = (AssemblyTypeAttribute)attributes[0];

                if (!string.IsNullOrEmpty(assemblyTypeAttribute.AssemblyType))
                {
                    result = assemblyTypeAttribute.AssemblyType;
                }
            }

            if ("native".Equals(result))
            {
                assemblyType = AssemblyType.Native;
            }
            else if ("managed".Equals(result))
            {
                assemblyType = AssemblyType.Managed;
            }

            return assemblyType;
        }
    }
}
