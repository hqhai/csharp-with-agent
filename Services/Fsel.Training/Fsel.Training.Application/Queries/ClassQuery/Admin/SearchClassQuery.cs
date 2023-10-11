// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.CourseServices.Models;
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassQuery : SearchClassQueryModel, IRequest<MethodResult<PagingItemsModel<ClassSearchModel>>>
    {
    }

    public class SearchClassQueryHandler : IRequestHandler<SearchClassQuery, MethodResult<PagingItemsModel<ClassSearchModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ICourseService _courseService;

        public SearchClassQueryHandler(IClassRepository classRepository, IUserService userService, IOrderService orderService, ICourseService courseService)
        {
            _classRepository = classRepository;
            _userService = userService;
            _orderService = orderService;
            _courseService = courseService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassSearchModel>>> Handle(SearchClassQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classes = await _classRepository.Queryable.Where(x => !request.ClassId.HasValue || request.ClassId != x.Id).Include(i => i.ClassStudents).Select(p => new ClassSearchModel
            {
                Id = p.Id,
                ClassName = p.Name,
                NumberOfStudent = p.ClassStudents.Where(n => n.IsActive).Count(),
                TeacherId = p.TeacherId,
                CSOId = p.CsoId,
                ExpectedDate = p.CreatedDate.AddDays(15),
                ActivationDate = p.CreatedDate,
                Status = p.Status,
                PackageId = p.PackageId,
                CourseId = p.CourseId,
                StudentIds = p.ClassStudents.Where(m => m.IsActive).Select(x => x.StudentId).ToList(),
                CreatedDate = p.CreatedDate,
            }).ToListAsync(cancellationToken);
            if (request.Status.HasValue)
            {
                classes = classes.Where(p => p.Status == request.Status).ToList();
            }
            if (request.Level.HasValue)
            {
                var courses = await _courseService.GetCoursesByLevelAsync(request.Level);
                var courseIds = courses.Content?.Result?.Select(p => p.Id).ToList();

                classes = classes.Where(p => courseIds != null && courseIds.Contains(p.CourseId)).ToList();
            }
            var teacherResult = _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel
            {
                Ids = classes.Select(x => x.TeacherId ?? default).ToList()
            });

            var csosResult = _userService.GetCSOByIds(classes.Select(x => x.CSOId ?? default).ToList());

            var packagesResult = _orderService.GetPackages();

            var ptrResult = _courseService.GetAveragePTPoint(new GetAveragePTPointByIdsQueryModel
            {
                PointByIdQueryModels = classes.Select(x => new GetAveragePTPointByIdQueryModel
                {
                    ClassId = x.Id,
                    StudentIds = x.StudentIds
                }).ToList()
            });

            await Task.WhenAll(teacherResult, csosResult, packagesResult, ptrResult);

            var ptr = ptrResult.GetAwaiter().GetResult().Content?.Result;

            if (request.SortedByPT.HasValue && request.SortedByPT == true && ptr != null)
            {
                var ptrIds = ptr.OrderByDescending(p => p.AveragePTPoint).Select(x => x.ClassId).ToList();
                classes = classes.OrderBy(p => ptrIds.IndexOf(p.Id)).ToList();
            }
            else if (request.SortedByPT.HasValue && request.SortedByPT == false && ptr != null)
            {
                var ptrIds = ptr.OrderBy(p => p.AveragePTPoint).Select(x => x.ClassId).ToList();
                classes = classes.OrderBy(p => ptrIds.IndexOf(p.Id)).ToList();
            }

            IQueryable<ClassSearchModel> queryable = classes.AsQueryable();

            int totalItem = queryable.Count();

            var lists = queryable
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToList();

            foreach (var item in lists)
            {
                item.TeacherName = teacherResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.TeacherId)?.Human?.FullName;
                item.CSOName = csosResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.CSOId)?.FullName;
                item.PackageCode = packagesResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.PackageId)?.Code;
                item.AveragePT = Math.Round(ptr!.FirstOrDefault(p => p.ClassId == item.Id)!.AveragePTPoint, 2);
            }

            methodResult.Result = new PagingItemsModel<ClassSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
