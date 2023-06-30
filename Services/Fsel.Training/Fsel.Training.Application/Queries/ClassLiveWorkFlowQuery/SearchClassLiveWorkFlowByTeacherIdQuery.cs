//// Copyright (c) Atlantic. All rights reserved.

//namespace Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery
//{
//    using System;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using Fsel.Common.ActionResults;
//    using Fsel.Core.Base;
//    using Fsel.Core.Base.BaseModels;
//    using Fsel.Core.Extensions;
//    using Fsel.Training.Application.Services.CourseServices;
//    using Fsel.Training.Application.Services.SystemServices;
//    using Fsel.Training.Application.Services.UserServices;
//    using Fsel.Training.Domain.IRepositories;
//    using Fsel.Training.Domain.Models.EntityModels;
//    using Fsel.Training.Domain.Models.QueryModels.ClassLiveWorkFlowQuery;
//    using MediatR;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.EntityFrameworkCore;

//    public class SearchClassLiveWorkFlowByTeacherIdQuery : SearchClassLiveWorkFlowByTeacherIdQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>>
//    {
//    }

//    public class SearchClassLiveWorkFlowByTeacherIdQueryHandler : IRequestHandler<SearchClassLiveWorkFlowByTeacherIdQuery, MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>>
//    {
//        private readonly AuthContext _authContext;
//        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
//        private readonly IClassRepository _classRepository;
//        private readonly IUserService _userService;
//        private readonly ICourseService _courseService;
//        private readonly ISystemService _systemService;

//        public SearchClassLiveWorkFlowByTeacherIdQueryHandler(AuthContext authContext,
//            IClassLiveCalendarRepository classLiveCalendarRepository,
//            IClassRepository classRepository,
//            IUserService userService,
//            ICourseService courseService,
//            ISystemService systemService)
//        {
//            _authContext = authContext;
//            _classLiveCalendarRepository = classLiveCalendarRepository;
//            _classRepository = classRepository;
//            _userService = userService;
//            _courseService = courseService;
//            _systemService = systemService;
//        }

//        public async Task<MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>> Handle(SearchClassLiveWorkFlowByTeacherIdQuery request, CancellationToken cancellationToken)
//        {
//            ArgumentNullException.ThrowIfNull(request);
//            var methodResult = new MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>();
//            if (request.PageSize > 100)
//            {
//                methodResult.StatusCode = StatusCodes.Status400BadRequest;
//                return methodResult;
//            }
//            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
//            var teacher = teacherResult.Content?.Result;

//            var classQuery = _classRepository.Queryable.Where(x => x.TeacherId == teacher!.Id)
//                .Select(x => new ClassLiveModel
//                {
//                    Code = x.Code,
//                    CourseId = x.CourseId,
//                    CreatedDate = x.CreatedDate,
//                });
//            var classLiveQuery = _classLiveCalendarRepository.Queryable.Where(x => x.TeacherId == teacher!.Id);

//            var query = classQuery

//            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
//            var lists = await query
//                    .ApplySortAndPaging(request)
//                    .AsNoTracking()
//                    .ToListAsync(cancellationToken: cancellationToken)
//                    .ConfigureAwait(false);
//            var courseResults = await _courseService.GetListCourseByIds(courseIds);
//            var courses = courseResults.Content?.Result;
//            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
//            var timeFrames = timeFramesResult.Content?.Result;

//            methodResult.Result = new PagingItemsModel<ClassLiveWorkFlowSearchModel>(lists, request, totalItem);
//            methodResult.StatusCode = StatusCodes.Status200OK;
//            return methodResult;
//        }

//        public
//    }
//}
