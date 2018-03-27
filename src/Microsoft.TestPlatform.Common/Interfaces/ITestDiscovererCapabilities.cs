// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Common.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Metadata that is available from Test Discoverers.
    /// </summary>
    public interface ITestDiscovererCapabilities
    {
        /// <summary>
        /// List of file extensions that the test discoverer can process tests from.
        /// </summary>
        IEnumerable<string> FileExtension { get; }

        /// <summary>
        /// Default executor Uri for this discoverer
        /// </summary>
        Uri DefaultExecutorUri { get; }

        // TODO: Even though we let user pass string in AssemblyTypeAttribute, here we should store enum as the value should be stored here after validation.

        /// <summary>
        /// Assembly type supported by this discoverer.
        /// </summary>
        string AssemblyType { get; }
    }
}
