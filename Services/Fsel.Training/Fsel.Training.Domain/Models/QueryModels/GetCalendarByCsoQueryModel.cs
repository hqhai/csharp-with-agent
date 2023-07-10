// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using System;

    public class GetCalendarByCsoQueryModel : GetCalendarByTeacherQueryModel
    {
        public Guid? TeacherId { get; set; }
    }
}
