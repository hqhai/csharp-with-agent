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
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private const string VIDEOTYPE =  "Video bài giảng";
        private const string CLASSFORUMTYPE =  "Diễn dàn";
        private const string HOMEWORKTYPE =  "Bài tập về nhà";
        private const string DOCUMENTTYPE =  "Tài liệu";

        public GetListLessonByUnitIdQueryHandler(
            IMapper mapper,
            ILessonResultRepository lessonResultRepository,
            IUnitLessonRepository unitLessonRepository)
        {
            _mapper = mapper;
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
                SetDataForModule(lessonModulesModel, previousModuleCompleted, orderModules, lessonResult);
                bool lessonCompleted = lessonModulesModel.Any() && lessonModulesModel.All(m => m.IsDone);

                var lessonModel = _mapper.Map<LessonModel>(lesson);

                lessonModel.UnitId = request.UnitId;
                lessonModel.DisplayOrder = ul.DisplayOrder;
                lessonModel.IsLocked = !previousLessonCompleted;
                lessonModel.Status = lessonResult.Status;
                lessonModel.LessonModules = lessonModulesModel;
                lessonModel.CourseId =  request.CourseId;
                lessonModel.LessonResult = lessonResult.Id;

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
                EnumLessonConfigType.Video => VIDEOTYPE,
                EnumLessonConfigType.ClassForum => CLASSFORUMTYPE,
                EnumLessonConfigType.HomeWork => HOMEWORKTYPE,
                EnumLessonConfigType.Document => DOCUMENTTYPE,
                _ => type.ToString()
            };
        }

        private static void SetDataForModule(List<LessonModuleModel> lessonModulesModel, bool previousModuleCompleted, List<LessonModule> orderModules, LessonResult lessonResult)
        {
            for (int i = 0; i < lessonModulesModel.Count; i++)
            {
                var moduleEntity = orderModules[i];
                var moduleModel = lessonModulesModel[i];

                bool isDone = IsModuleDone(moduleEntity, lessonResult);
                bool isLocked = !previousModuleCompleted;

                if (string.IsNullOrEmpty(moduleModel.Name))
                {
                    moduleModel.Name = GetDefaultModuleName(moduleModel.LessonConfigType);
                }

                moduleModel.IsDone = isDone;
                moduleModel.IsLocked = isLocked;

                previousModuleCompleted = previousModuleCompleted && isDone;
            }
        }
    }
}
