// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Admins
{
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassStudentInfoQuery : IRequest<MethodResult<IList<ClassStudentInfoModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetClassStudentInfoQueryHandler : IRequestHandler<GetClassStudentInfoQuery, MethodResult<IList<ClassStudentInfoModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly ICourseService _courseService;
        private readonly IOrderService _orderService;

        public GetClassStudentInfoQueryHandler(IClassRepository classRepository, ICourseService courseService, IOrderService orderService)
        {
            _classRepository = classRepository;
            _courseService = courseService;
            _orderService = orderService;
        }

        public async Task<MethodResult<IList<ClassStudentInfoModel>>> Handle(GetClassStudentInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<ClassStudentInfoModel>> methodResult = new MethodResult<IList<ClassStudentInfoModel>>();

            var classStudents = await _classRepository.Queryable.Include(x => x.ClassStudents.Where(n => !n.IsDeleted))
                                            .Where(e => e.ClassStudents.Select(n => n.StudentId).Contains(request.StudentId))
                                            .Select(x => new ClassStudentInfoModel
                                            {
                                                Id = x.Id,
                                                ClassName = x.Name,
                                                CourseId = x.CourseId,
                                                StartDate = x.StartDate,
                                                EndDate = x.EndDate,
                                                CreatedDate = x.CreatedDate,
                                                PackageId = x.PackageId,
                                                Status = x.Status,
                                            })
                                            .ToListAsync(cancellationToken: cancellationToken);
            var packagesReq = _orderService.GetPackages();
            var coursesReq = _courseService.GetListCourseByIds(classStudents.Select(x => x.CourseId).ToList());
            await Task.WhenAll(packagesReq, coursesReq);

            var coursesResult = coursesReq.GetAwaiter().GetResult();
            var packagesResult = packagesReq.GetAwaiter().GetResult();

            var courses = coursesResult.Content?.Result;
            var packages = packagesResult.Content?.Result;

            foreach (var classStudent in classStudents)
            {
                var course = courses?.FirstOrDefault(x => x.Id == classStudent.CourseId);
                var package = packages?.FirstOrDefault(x => x.Id == classStudent.PackageId);
                classStudent.CourseName = course?.Code;
                classStudent.Membership = package?.Code.ToString();
            }

            methodResult.Result = classStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
