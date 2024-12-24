// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseAndUnitByUserIdQuery : IRequest<MethodResult<StudentCourseUnitModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetCourseAndUnitByUserIdQueryHandler : IRequestHandler<GetCourseAndUnitByUserIdQuery, MethodResult<StudentCourseUnitModel>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetCourseAndUnitByUserIdQueryHandler(IUserService userService, IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<StudentCourseUnitModel>> Handle(GetCourseAndUnitByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentCourseUnitModel>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode || studentResult.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            var courseUnit = await _unitResultRepository.Queryable.Include(p => p.Unit).ThenInclude(a => a!.CourseUnitMockTests).Include(x => x.Course).Where(n => n.Status != Domain.Enums.EnumResultStatus.Unfinished && n.StudentId == student!.Id).OrderByDescending(m => m.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (courseUnit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            methodResult.Result = new StudentCourseUnitModel { CourseLevel = courseUnit.Course!.CourseLevel, UnitNumber = courseUnit.Unit!.CourseUnitMockTests.FirstOrDefault(p => p.CourseId == courseUnit.CourseId)!.Number };
            return methodResult;
        }
    }
}
