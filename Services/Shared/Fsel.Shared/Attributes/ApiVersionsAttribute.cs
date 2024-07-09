// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Attributes
{
    using System;
    using System.Globalization;
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed partial class ApiVersionsAttribute : ApiVersionsBaseAttribute, IApiVersionProvider
    {
        private ApiVersionProviderOptions _options = ApiVersionProviderOptions.None;

        public string Version { get; }
        ApiVersionProviderOptions IApiVersionProvider.Options => _options;

        public ApiVersionsAttribute(string version) : base(version, GetVersions(version))
        {
            Version = version;
        }

        private static string[] GetVersions(string? currentVersion)
        {
            // Logic để sinh ra các phiên bản API
            if (currentVersion == null)
            {
                return Array.Empty<string>();
            }
            return ApiSettings.APIVersions.Where(x => double.Parse(currentVersion, CultureInfo.InvariantCulture) < double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
