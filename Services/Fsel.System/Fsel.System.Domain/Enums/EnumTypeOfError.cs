// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumTypeOfError
    {
        [Description("Lỗi học thuật")]
        AcademicError,

        [Description("Lỗi phần mềm")]
        SoftwareError,

        [Description("Lỗi từ phía người dùng")]
        UserInputError,

        [Description("Lỗi từ thiết bị người dùng")]
        UserDeviceError
    }
}
