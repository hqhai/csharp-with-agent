// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Bases.V1i1
{
    using Asp.Versioning;
    using Fsel.Shared.Constants;

    [ApiVersion(ApiSettings.APIVersion1i1)]
    [ApiVersion(ApiSettings.APIVersion1i2)]
    public class BaseController : Core.Base.BaseController
    {
    }
}
