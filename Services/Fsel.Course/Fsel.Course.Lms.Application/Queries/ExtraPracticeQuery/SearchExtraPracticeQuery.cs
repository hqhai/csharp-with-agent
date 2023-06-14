// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeLmsQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , IMapper mapper
            , IUserService userService
            , AuthContext authContext)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ExtraPracticeModel>>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;


            var extraPracticeQuery = _extraPracticeRepository.Queryable
                        .Include(x => x.PlacementTest)
                        .Include(x => x.LessonExtraPractices.Where(n => !n.IsDeleted && n.Lesson != null))
                            .ThenInclude(x => x.Lesson)
                            .ThenInclude(x => x.UnitLessons.Where(n => !n.IsDeleted))
                        .Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.Exercise)
                        .Include(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted && n.Exercise != null))
                            .ThenInclude(x => x.Exercise)
                        .Include(x => x.Video)
                        .ThenInclude(x => x.VideoTimeCodes.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.TimeCodeExercises.Where(n => !n.IsDeleted && n.Exercise != null))
                                .ThenInclude(x => x.Exercise).Select(x => new ExtraPracticeModel
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    Name = x.Name,
                                    CreatedDate = x.CreatedDate,
                                    Type = x.Type,
                                    FilePaths = x.FilePaths
                                });
            //.Select(x => new ExtraPracticeModel
            //{
            //    Id = x.Id,
            //    Code = x.Code,
            //    Name = x.Name,
            //    Type = x.Type,
            //    FilePaths = x.FilePaths,
            //    CourseLevel = x.CourseLevel,
            //    CourseSkills = x.ExtraPracticeChapters != null ? x.ExtraPracticeChapters.Where(n => !n.IsDeleted)
            //        .SelectMany(e => e.ExtraPracticeExercises.Where(n => n.Exercise != null && !n.IsDeleted)
            //            .Select(e => e.Exercise.CourseSkill))
            //        .ToList()
            //        : x.ExtraPracticeExercises != null ? x.ExtraPracticeExercises.Where(n => n.Exercise != null && !n.IsDeleted)
            //            .Select(e => e.Exercise.CourseSkill)
            //            .ToList()
            //        : x.Video != null ? x.Video.VideoTimeCodes.SelectMany(v => v.TimeCodeExercises.Where(n => n.Exercise != null && !n.IsDeleted)
            //            .Select(v => v.Exercise.CourseSkill))
            //            .ToList()
            //        : x.PlacementTest != null ? x.PlacementTest.PlacementTestSections.Where(n => n.SectionGroup != null && !n.IsDeleted)
            //            .Select(p => p.SectionGroup.CourseSkill)
            //            .ToList()
            //        : null,
            //    //PlacementTest = x.Type == EnumExtraPracticeType.MockTest ? _mapper.Map<PlacementTestModel>(x.PlacementTest) : null,
            //    UnitId = x.LessonExtraPractices.Where(n => !n.IsDeleted && n.Lesson != null)
            //        .Select(x => x.Lesson)
            //        .SelectMany(x => x.UnitLessons)
            //        .Select(x => x.UnitId)
            //        .FirstOrDefault(),
            //    Percent = x.ExtraPracticeResults.FirstOrDefault(y => y != null && y.ExtraPracticeId == x.Id && y.StudentId == studentId)!.Percent
            //});
            int totalItem = await extraPracticeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await extraPracticeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExtraPracticeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
