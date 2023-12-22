// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.StudentReviewQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetReviewStudentsByStudentQuery : IRequest<MethodResult<IList<StudentReviewInfoModel>>>
    {
    }

    public class GetReviewStudentsByStudentQueryHandler : IRequestHandler<GetReviewStudentsByStudentQuery, MethodResult<IList<StudentReviewInfoModel>>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ICourseService _courseService;

        public GetReviewStudentsByStudentQueryHandler(IStudentReviewRepository studentReviewRepository
            , AuthContext authContext
            , IMapper mapper
            , ICourseService courseService)
        {
            _studentReviewRepository = studentReviewRepository;
            _authContext = authContext;
            _mapper = mapper;
            _courseService = courseService;
        }

        public async Task<MethodResult<IList<StudentReviewInfoModel>>> Handle(GetReviewStudentsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentReviewInfoModel>>();
            var courseResult = await _courseService.GetCourseStudying();
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            methodResult.Result = await GetStudentReview(_authContext.CurrentUserId, course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<StudentReviewInfoModel>> GetStudentReview(Guid userId, CourseModel course)
        {
            var reviewTypes = ConvertHelper.EnumToList<EnumReviewType>();
            var studentReviews = await _studentReviewRepository.Queryable
                .Include(x => x.StudentReviewDetails.OrderBy(x => x.CreatedDate))
                .Where(x => x.CreatedUserId == userId && reviewTypes.Contains(x.ReviewType) && (x.ReviewType != EnumReviewType.Course || x.CourseId == course.Id))
                .AsNoTracking()
                .ToListAsync();
            return studentReviews.Select(x => GetStudentReviewInfo(x, course)).ToList();
        }

        private StudentReviewInfoModel GetStudentReviewInfo(StudentReview studentReview, CourseModel course)
        {
            var studentReviewInfo = _mapper.Map<StudentReviewInfoModel>(studentReview);
            studentReviewInfo.CourseName = studentReview.CourseId.HasValue ? course.Name : null;
            studentReviewInfo.VoteStars = NumberHelper.ConvertRound(studentReview.StudentReviewDetails.Average(x => x.VoteStars));
            return studentReviewInfo;
        }
    }
}
