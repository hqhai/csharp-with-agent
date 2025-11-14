// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i2
{
    using AutoMapper;
    using Common.ActionResults;
    using Domain.Entities;
    using Domain.Entities.V1i1;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.V1i2;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonByUnitIdQuery : IRequest<MethodResult<IList<LessonModel>>>
    {
        public Guid UnitId { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetListLessonByUnitIdQueryHandler : IRequestHandler<GetListLessonByUnitIdQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;

        public GetListLessonByUnitIdQueryHandler(ILessonRepository lessonRepository,
            IMapper mapper,
            ILessonModuleRepository lessonModuleRepository,
            ILessonResultRepository lessonResultRepository,
            IUnitLessonRepository unitLessonRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitLessonRepository = unitLessonRepository;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetListLessonByUnitIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonModel>>();

            var unitLessons = await _unitLessonRepository.ReadQueryable
                .Where(ul => ul.UnitId == request.UnitId)
                .Include(ul => ul.Lesson!)
                .ThenInclude(l => l.LessonModules)
                .OrderBy(ul => ul.DisplayOrder)
                .ToListAsync(cancellationToken);

            if (!unitLessons.Any())
            {
                methodResult.Result = new List<LessonModel>();
                return methodResult;
            }

            var lessonIds = unitLessons.Select(ul => ul.LessonId).ToList();

            var lessonResults = await _lessonResultRepository.ReadQueryable
                .Where(lr => lr.CourseId == request.CourseId
                             && lr.UnitId == request.UnitId
                             && lr.StudentId == request.StudentId
                             && lessonIds.Contains(lr.LessonId))
                .Include(lr => lr.VideoResult)
                .Include(lr => lr.HomeWorkResults)
                .Include(lr => lr.ClassForumResults)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var lessonResultByLessonId = lessonResults
                .GroupBy(lr => lr.LessonId)
                .ToDictionary(g => g.Key, g => g.First());

            var response = new List<LessonModel>();

            bool previousLessonCompleted = true;
            foreach (var ul in unitLessons)
            {
                var lesson = ul.Lesson!;

                var orderModules = lesson.LessonModules
                    .OrderBy(m => m.DisplayNumber)
                    .ToList();

                var lessonModulesModel = _mapper.Map<List<LessonModuleModel>>(orderModules);
                lessonResultByLessonId.TryGetValue(lesson.Id, out var lessonResult);

                bool previousModuleCompleted = previousLessonCompleted;

                for (int i = 0; i < lessonModulesModel.Count; i++)
                {
                    var moduleEntity = orderModules[i];
                    var moduleModel = lessonModulesModel[i];

                    bool isDone = IsModuleDone(moduleEntity, lessonResult);
                    bool isLocked = !previousModuleCompleted;

                    moduleModel.IsDone = isDone;
                    moduleModel.IsLocked = isLocked;

                    previousModuleCompleted = previousModuleCompleted && isDone;
                }
                bool lessonCompleted = lessonModulesModel.Any() && lessonModulesModel.All(m => m.IsDone);

                var lessonModel = _mapper.Map<LessonModel>(lesson);
                lessonModel.UnitId = request.UnitId;
                lessonModel.DisplayOrder = ul.DisplayOrder;
                lessonModel.IsLocked = !previousLessonCompleted;
                lessonModel.Status = lessonResult?.Status ?? default;
                lessonModel.LessonModules = lessonModulesModel;

                response.Add(lessonModel);

                previousLessonCompleted = previousLessonCompleted && lessonCompleted;
            }

            methodResult.Result = response;
            return methodResult;
        }

        private static bool IsModuleDone(LessonModule module, LessonResult? lessonResult)
        {
            if (lessonResult == null)
            {
                return false;
            }

            return module.LessonConfigType switch
            {
                EnumLessonConfigType.Video => lessonResult.VideoResult is { Status: EnumResultStatus.Done },
                EnumLessonConfigType.ClassForum => lessonResult.ClassForumResults.Any() && lessonResult.ClassForumResults.Any(x => x.TokenFirstTime != null),
                EnumLessonConfigType.HomeWork => lessonResult.HomeWorkResults.Any() && lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done),
                _ => lessonResult.Percent >= module.Percent
            };
        }

        private static string GetDefaultModuleName(EnumLessonConfigType type)
        {
            return type switch
            {
                EnumLessonConfigType.Video => "Video bài giảng",
                EnumLessonConfigType.ClassForum => "Diễn đàn",
                EnumLessonConfigType.HomeWork => "Bài tập về nhà",
                _ => type.ToString()
            };
        }
    }
}
