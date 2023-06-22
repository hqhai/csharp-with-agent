// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.CourseServices.Models;
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassByAdminQuery : SearchClassQueryModel, IRequest<MethodResult<PagingItemsModel<ClassSearchModel>>>
    {
    }

    public class SearchClassByAdminQueryHandler : IRequestHandler<SearchClassByAdminQuery, MethodResult<PagingItemsModel<ClassSearchModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ICourseService _courseService;
        private readonly IClassStudentRepository _classStudentRepository;

        public SearchClassByAdminQueryHandler(IClassRepository classRepository, IUserService userService, IOrderService orderService, ICourseService courseService, IClassStudentRepository classStudentRepository)
        {
            _classRepository = classRepository;
            _userService = userService;
            _orderService = orderService;
            _courseService = courseService;
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ClassSearchModel>>> Handle(SearchClassByAdminQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            List<Guid>? courseIds = new List<Guid>();

            if (!string.IsNullOrEmpty(request.Level.ToString()))
            {
                var courses = await _courseService.GetCoursesByLevelAsync(request.Level);
                courseIds = courses.Content?.Result?.Select(p => p.Id).ToList()!;
            }
            var classes = _classRepository.Queryable.Include(i => i.ClassStudents).Where(p => courseIds.Count == 0 || courseIds.Contains(p.CourseId)).Select(p => new ClassSearchModel
            {
                Id = p.Id,
                ClassName = p.Name,
                NumberOfStudent = p.ClassStudents.Count,
                TeacherId = p.TeacherId,
                CSOId = p.CsoId,
                ExpectedDate = p.CreatedDate.AddDays(15),
                ActivationDate = p.CreatedDate,
                Status = p.Status,
                PackageId = p.PackageId,
                StudentIds = p.ClassStudents.Select(x => x.StudentId).ToList(),
            });
            if (request.Status.HasValue)
            {
                classes = classes.Where(p => p.Status == request.Status);
            }
            int totalItem = await classes.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classes
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResult = _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel
            {
                Ids = lists.Select(x => x.TeacherId ?? default).ToList()
            });

            var csosResult = _userService.GetCSOByIds(lists.Select(x => x.CSOId ?? default).ToList());

            var packagesResult = _orderService.GetPackages();

            var ptrResult = _courseService.GetAveragePTPoint(new GetAveragePTPointByIdsQueryModel
            {
                PointByIdQueryModels = lists.Select(x => new GetAveragePTPointByIdQueryModel
                {
                    ClassId = x.Id,
                    StudentIds = x.StudentIds
                }).ToList()
            });

            await Task.WhenAll(teacherResult, csosResult, packagesResult, ptrResult);

            var ptr = ptrResult.GetAwaiter().GetResult().Content?.Result;

            foreach (var item in lists)
            {
                item.TeacherName = teacherResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.TeacherId)?.Human?.FullName;
                item.CSOName = csosResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.CSOId)?.FullName;
                item.PackageName = packagesResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.PackageId)?.Code;
                item.AveragePT = ptr!.FirstOrDefault(p => p.ClassId == item.Id)!.AveragePTPoint;
            }

            methodResult.Result = new PagingItemsModel<ClassSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
