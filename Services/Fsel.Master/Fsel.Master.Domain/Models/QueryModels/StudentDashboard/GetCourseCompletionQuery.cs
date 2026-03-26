// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.QueryModels.StudentDashboard
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using MediatR;

    /// <summary>
    /// Query model cho màn hình Tỉ lệ hoàn thành khóa học
    /// </summary>
    public class GetCourseCompletionQuery : BaseQueryModel, IRequest<MethodResult<CourseCompletionResponseModel>>
    {
        /// <summary>
        /// Filter theo khóa học
        /// </summary>
        public IList<Guid>? CourseIds { get; set; }

        /// <summary>
        /// Filter theo tỉnh/thành
        /// </summary>
        public IList<Guid>? ProvinceIds { get; set; }

        /// <summary>
        /// Filter theo quận/huyện
        /// </summary>
        public IList<Guid>? DistrictIds { get; set; }

        /// <summary>
        /// Filter theo trường học
        /// </summary>
        public IList<Guid>? SchoolIds { get; set; }

        /// <summary>
        /// Filter từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
