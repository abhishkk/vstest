// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery
{
    using System.Runtime.Versioning;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    /// <summary>
    /// Defines interface for assembly properties.
    /// </summary>
    internal interface IAssemblyMetadataProvider
    {
        // TODO: this interfacce is already present in command line. Move that to common and delete this interface.

        /// <summary>
        /// Determines AssemblyType from filePath.
        /// </summary>
        /// <param name="filePath"></param>
        AssemblyType GetAssemblyType(string filePath);
    }
}
