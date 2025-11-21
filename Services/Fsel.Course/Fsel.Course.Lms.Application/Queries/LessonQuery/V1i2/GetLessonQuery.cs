// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i2
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonDtoModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonDtoModel>>
    {
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleCachingService _lessonModuleCachingService;
        private readonly IVideoRepository _videoRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;

        public GetLessonQueryHandler(
            ILessonModuleRepository lessonModuleRepository,
            ILessonResultRepository lessonResultRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IHomeWorkRepository homeWorkRepository,
            IVideoRepository videoRepository,
            IClassForumRepository classForumRepository,
            IDocumentRepository documentRepository,
            IMapper mapper)
        {
            _lessonModuleRepository = lessonModuleRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _videoRepository = videoRepository;
            _classForumRepository = classForumRepository;
            _homeWorkRepository = homeWorkRepository;
            _documentRepository = documentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonDtoModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonDtoModel>();

            var lessonResult = await GetLessonResultAsync(request, methodResult);
            if (lessonResult == null || !methodResult.IsOK)
            {
                return methodResult;
            }

            var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);
            if (!lessonModules.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new LessonDtoModel();
                return methodResult;
            }
            var moduleLessons = await GetLessonModelsAsync(lessonResult, lessonModules, cancellationToken);

            var lesson = _mapper.Map<LessonDtoModel>(lessonResult.Lesson);
            if (lesson != null)
            {
                lesson.ModuleLessons = moduleLessons;
            }
            methodResult.Result = lesson;
            return methodResult;
        }

        private async Task<IList<ModuleLessonModel>> GetLessonModelsAsync(
        LessonResult lessonResult,
        IList<LessonModule> lessonModules,
        CancellationToken cancellationToken)
        {
            // Lookup theo từng type
            var (videoResultsByOriginalId, videoDics) = await _videoRepository.BuildVideoLookupsAsync(lessonResult, lessonModules);

            var (classForumResultsByOriginalId, classForumDics) = await _classForumRepository.BuildClassForumLookupsAsync(lessonResult, lessonModules);

            var (homeWorkResultsByOriginalId, homeWorkDics) = await _homeWorkRepository.BuildHomeWorkLookupsAsync(lessonResult, lessonModules);

            var (documentResultsByOriginalId, documentDics) = await _documentRepository.BuildDocumentLookupsAsync(lessonResult, lessonModules);

            var moduleResults = new List<ModuleLessonModel>();

            foreach (var module in lessonModules.OrderBy(x => x.DisplayOrder))
            {
                switch (module.LessonConfigType)
                {
                    case EnumLessonConfigType.Video:
                        BuildVideoModuleLesson(module, videoResultsByOriginalId, videoDics, moduleResults);
                        break;

                    case EnumLessonConfigType.ClassForum:
                        BuildClassForumModuleLesson(module, lessonResult, classForumResultsByOriginalId, classForumDics, moduleResults);
                        break;

                    case EnumLessonConfigType.HomeWork:
                        BuildHomeWorkModuleLesson(module, homeWorkResultsByOriginalId, homeWorkDics, moduleResults);
                        break;

                    case EnumLessonConfigType.Document:
                        BuildDocumentModuleLesson(module, lessonResult, documentResultsByOriginalId, documentDics, moduleResults);
                        break;
                }
            }

            return moduleResults;
        }

        private void BuildVideoModuleLesson(
        LessonModule module,
        IDictionary<Guid, (Video Video, VideoResult VideoResult)> videoResultsByOriginalId,
        IDictionary<Guid, Video> videoDics,
        IList<ModuleLessonModel> moduleResults)
        {
            if (videoResultsByOriginalId.TryGetValue(module.OriginalId, out var videoResult))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = videoResult.Video.Name;
                dto.ObjectId = videoResult.Video.Id;
                dto.Result = _mapper.Map<ResultModel>(videoResult.VideoResult);
                moduleResults.Add(dto);
                return;
            }

            if (videoDics.TryGetValue(module.OriginalId, out var video))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = video.Name;
                dto.ObjectId = video.Id;
                moduleResults.Add(dto);
            }
        }

        private void BuildClassForumModuleLesson(
        LessonModule module,
        LessonResult lessonResult,
        IDictionary<Guid, (ClassForum ClassForum, ClassForumResult ClassForumResult)> classForumResultsByOriginalId,
        IDictionary<Guid, ClassForum> classForumDics,
        IList<ModuleLessonModel> moduleResults)
        {
            if (classForumResultsByOriginalId.TryGetValue(module.OriginalId, out var forumResult))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = lessonResult.Lesson?.Name;
                dto.ObjectId = forumResult.ClassForum.Id;
                dto.Result = _mapper.Map<ResultModel>(forumResult.ClassForumResult);
                moduleResults.Add(dto);
                return;
            }

            if (classForumDics.TryGetValue(module.OriginalId, out var forum))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = lessonResult.Lesson?.Name;
                dto.ObjectId = forum.Id;
                moduleResults.Add(dto);
            }
        }

        private void BuildHomeWorkModuleLesson(
        LessonModule module,
        IDictionary<Guid, (HomeWork HomeWork, HomeWorkResult HomeWorkResult)> homeWorkResultsByOriginalId,
        IDictionary<Guid, HomeWork> homeWorkDics,
        IList<ModuleLessonModel> moduleResults)
        {
            if (homeWorkResultsByOriginalId.TryGetValue(module.OriginalId, out var hwResult))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = hwResult.HomeWork.Name;
                dto.Code = hwResult.HomeWork.Code;
                dto.ObjectId = hwResult.HomeWork.Id;
                dto.Result = _mapper.Map<ResultModel>(hwResult.HomeWorkResult);
                moduleResults.Add(dto);
                return;
            }

            if (homeWorkDics.TryGetValue(module.OriginalId, out var hw))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = hw.Name;
                dto.ObjectId = hw.Id;
                moduleResults.Add(dto);
            }
        }

        private void BuildDocumentModuleLesson(
            LessonModule module,
            LessonResult lessonResult,
            IDictionary<Guid, (Document Document, DocumentResult DocumentResult)> documentResultsByOriginalId,
            IDictionary<Guid, Document> documentDics,
            IList<ModuleLessonModel> moduleResults)
        {
            if (documentResultsByOriginalId.TryGetValue(module.OriginalId, out var docResult))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = lessonResult.Lesson?.Name;
                dto.ObjectId = docResult.Document.Id;
                dto.Result = _mapper.Map<ResultModel>(docResult.DocumentResult);
                moduleResults.Add(dto);
                return;
            }

            if (documentDics.TryGetValue(module.OriginalId, out var doc))
            {
                var dto = _mapper.Map<ModuleLessonModel>(module);
                dto.Name = lessonResult.Lesson?.Name;
                dto.ObjectId = doc.Id;
                moduleResults.Add(dto);
            }
        }

        private async Task<LessonResult?> GetLessonResultAsync(GetLessonQuery request, MethodResult<LessonDtoModel> methodResult)
        {
            var lessonResult = await _lessonResultRepository.ReadQueryable.Include(x => x.Lesson).FirstOrDefaultAsync(x => x.Id == request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return null;
            }
            return lessonResult;
        }

        private async Task<IList<LessonModule>> GetLessonModulesAsync(Guid id)
        {
            return await _lessonModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var lessonModules = await _lessonModuleRepository.ReadQueryable
                                                             .Where(x => x.LessonId == id)
                                                             .ToListAsync(_);

                return lessonModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
