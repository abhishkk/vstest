// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.ObjectModel
{
    using System;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Resources;

    // TODO: we should not use this class at all. Instead assembly type should be part of FileExtensionsAttribute. Create something lie DllFileExtensionAttribute.

    /// <summary>
    /// This attribute is applied on the discoverers to inform the framework about their assembly type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AssemblyTypeAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Initializes with the assembly type that the test discoverer can process tests from. 
        /// </summary>
        /// <param name="assemblyType">The assembly type that the test discoverer can process tests from.</param>
        public AssemblyTypeAttribute(string assemblyType)
        {
            if (string.IsNullOrWhiteSpace(assemblyType))
            {
                throw new ArgumentException(CommonResources.CannotBeNullOrEmpty, "assemblyType");
            }

            AssemblyType = assemblyType;
        }

        #endregion

        #region Properties

        // TODO: If we are picking assembly type as string instead of enum, then we should document it clearly that only native and managed will be respected. Others will not. Ideally we should use enum.

        /// <summary>
        /// The assembly type that the test discoverer can process tests from.
        /// </summary>
        public string AssemblyType { get; private set; }

        #endregion

    }
}
