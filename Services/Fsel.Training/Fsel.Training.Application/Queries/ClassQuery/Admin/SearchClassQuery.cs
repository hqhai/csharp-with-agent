// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Diagnostics;
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
            var listDa = new List<(string, long)>();

            var query = _classRepository.Queryable;
            if (request.ClassId.HasValue)
            {
                query = query.Where(x => x.Id != request.ClassId);
            }
            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status);
            }
            if (request.Level.HasValue)
            {
                var courses = await _courseService.GetCoursesByLevelAsync(request.Level);
                var courseIds = courses.Content?.Result?.Select(p => p.Id).ToList() ?? new List<Guid>();

                query = query.WhereBulkContains(courseIds, p => p.CourseId);
            }

            //if (request.SortedByPT.HasValue)
            //{
            //    var dataQuery = await query.Select(x => new GetAveragePTPointByIdQueryModel
            //    {
            //        ClassId = x.Id,
            //        StudentIds = x.ClassStudents.Where(m => m.IsActive).Select(x => x.StudentId).ToList()
            //    }).ToListAsync(cancellationToken);

            //    var placementTestResults = await _courseService.GetAveragePTPoint(new GetAveragePTPointByIdsQueryModel
            //    {
            //        PointByIdQueryModels = dataQuery
            //    });

            //    var ptrR = placementTestResults.Content?.Result;
            //    var ptrIds = request.SortedByPT.Value ? ptrR?.OrderByDescending(p => p.AveragePTPoint).ApplyPaging(request).Select(x => x.ClassId).ToList() :
            //                                            ptrR?.OrderBy(p => p.AveragePTPoint).ApplyPaging(request).Select(x => x.ClassId).ToList();

            //    query = query.Where(p => ptrIds != null && ptrIds.Contains(p.Id));
            //}

            var queryClass = query.Select(p => new ClassSearchModel
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
            });

            int totalItem = await queryClass.CountAsync(cancellationToken);

            var lists = await queryClass.ApplySortAndPaging(request)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken);

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

            var ptr = ptrResult?.GetAwaiter().GetResult().Content?.Result;
            foreach (var item in lists)
            {
                var averagePTPoint = ptr?.Where(p => p.ClassId == item.Id).Select(x => x.AveragePTPoint).FirstOrDefault() ?? default;
                item.TeacherName = teacherResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.TeacherId)?.User?.FullName;
                item.CSOName = csosResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.CSOId)?.User?.FullName;
                item.PackageCode = packagesResult.GetAwaiter().GetResult().Content?.Result?.FirstOrDefault(p => p.Id == item.PackageId)?.Code;
                item.AveragePT = Math.Round(averagePTPoint, 2);
                item.StudentIds = null;
            }

            //if (request.SortedByPT.HasValue)
            //{
            //    lists = request.SortedByPT.Value ? lists.OrderByDescending(x => x.AveragePT).ToList() : lists.OrderBy(x => x.AveragePT).ToList();
            //}

            methodResult.Result = new PagingItemsModel<ClassSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
