// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.CourseServices;
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

        public SearchClassByAdminQueryHandler(IClassRepository classRepository, IUserService userService, IOrderService orderService, ICourseService courseService)
        {
            _classRepository = classRepository;
            _userService = userService;
            _orderService = orderService;
            _courseService = courseService;
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
            List<Guid>? CourseIds = new List<Guid>();

            if (!string.IsNullOrEmpty(request.Level.ToString()))
            {
                var courses = await _courseService.GetCoursesByLevelAsync(request.Level);
                CourseIds = courses.Content?.Result?.Select(p => p.Id).ToList()!;
            }
            var classes = _classRepository.Queryable.Include(i => i.ClassStudents).Select(p => new ClassSearchModel
            {
                ClassName = p.Name,
                NumberOfStudent = p.ClassStudents.Count,
                TeacherId = p.TeacherId,
                CSOId = p.CsoId,
                ExpectedDate = p.CreatedDate.AddDays(15),
                ActivationDate = p.CreatedDate,
                Status = p.Status,
                PackageId = p.PackageId
            });
            if (!string.IsNullOrEmpty(request.Status.ToString()))
            {
                classes = classes.Where(p => p.Status == request.Status);
            }
            int totalItem = await classes.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classes
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            IList<Guid>? teacherAndCSOIds = new List<Guid>();

            var teacherIds = lists.Select(x => x.TeacherId ?? default).Where(p => p != default).ToList();
            var csoIds = lists.Select(x => x.CSOId ?? default).Where(p => p != default).ToList();
            teacherAndCSOIds = teacherAndCSOIds.Concat(teacherIds).ToList();
            teacherAndCSOIds = teacherAndCSOIds.Concat(csoIds).ToList();
            var teacherAndCSOResult = await _userService.GetTeacherAndCSOByIds(teacherAndCSOIds);

            var packages = await _orderService.GetPackages();

            foreach (var item in lists)
            {
                item.TeacherName = teacherAndCSOResult.Content?.Result?.FirstOrDefault(p => p.Id == item.TeacherId)?.FullName;
                item.CSOName = teacherAndCSOResult.Content?.Result?.FirstOrDefault(p => p.Id == item.CSOId)?.FullName;
                item.PackageName = packages.Content?.Result?.FirstOrDefault(p => p.Id == item.PackageId)?.Code;
            }
            methodResult.Result = new PagingItemsModel<ClassSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
