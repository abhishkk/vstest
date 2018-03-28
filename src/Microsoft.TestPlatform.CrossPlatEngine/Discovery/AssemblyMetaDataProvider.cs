// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    using System.Runtime.Versioning;
    using System.Text;
    using ObjectModel;

    internal class AssemblyMetadataProvider : IAssemblyMetadataProvider
    {
        // TODO: make class singleton, we are still able to create its object from outside like from DiscovererEnumerator.
        private static AssemblyMetadataProvider instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        internal static AssemblyMetadataProvider Instance => instance ?? (instance = new AssemblyMetadataProvider());

        /// <inheritdoc />
        public AssemblyType GetAssemblyType(string filePath)
        {
            var assemblyType = AssemblyType.Managed;

            try
            {
                using (var assemblyStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    // TODO: For exe also verify if its working or not. For dll its verified.
                    assemblyType = GetAssemblyTypeFromAssemblyMetadata(assemblyStream);
                }
            }
            catch (Exception ex)
            {
                // TODO: In case of error, we should either error out OR we should set assembly type to UnKnown. But never let it be managed.
                EqtTrace.Warning("GetAssemblyTypeFromAssemblyMetadata: failed to determine assembly type: {0} for assembly: {1}", ex, filePath);
            }

            if (EqtTrace.IsInfoEnabled)
            {
                EqtTrace.Info("AssemblyMetadataProvider.GetAssemblyType: Determined assemblyType:'{0}' for source: '{1}'", assemblyType, filePath);
            }

            return assemblyType;
        }

        private AssemblyType GetAssemblyTypeFromAssemblyMetadata(FileStream assemblyStream)
        {
            // TODO: why we are opening and reading the dll again and again. We should read the dll in one go in contructor. After calling all methods destroy this object.

            var assemblyType = AssemblyType.Managed;

            using (var peReader = new PEReader(assemblyStream))
            {
                var peHeaders = peReader.PEHeaders;
                var corHeader = peHeaders.CorHeader;
                var corHeaderStartOffset = peHeaders.CorHeaderStartOffset;

                if (corHeader == null && corHeaderStartOffset < 0)
                {
                    assemblyType = AssemblyType.Native;
                }
            }

            return assemblyType;
        }
    }
}
