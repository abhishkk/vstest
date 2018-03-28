// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    internal class InferHelper
    {
        private IAssemblyMetadataProvider assemblyMetadataProvider;

        internal InferHelper(IAssemblyMetadataProvider assemblyMetadataProvider)
        {
            this.assemblyMetadataProvider = assemblyMetadataProvider;
        }

        // TODO: Instead of taking empty sourceAssemblyTypes map, create it in method and return it.
        public void AutoDetectAssemblyType(IEnumerable<string> sources, IDictionary<string, AssemblyType> sourceAssemblyTypes)
        {
            try
            {
                if (sources != null && sources.Any())
                {
                    DetermineAssemblyTypes(sources, sourceAssemblyTypes);
                }
            }
            catch (Exception ex)
            {
                EqtTrace.Error("Failed to determine assemblyTypes:{0}", ex);
            }
        }

        private bool IsDotNETAssembly(string filePath)
        {
            var extType = Path.GetExtension(filePath);
            return extType != null && (extType.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
                                       extType.Equals(".exe", StringComparison.OrdinalIgnoreCase));
        }

        private void DetermineAssemblyTypes(IEnumerable<string> sources, IDictionary<string, AssemblyType> sourceAssemblyTypes)
        {
            foreach (string source in sources)
            {
                if (IsDotNETAssembly(source))
                {
                    sourceAssemblyTypes[source] = assemblyMetadataProvider.GetAssemblyType(source);
                }
            }
        }
    }
}
