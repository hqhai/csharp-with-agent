// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LessonQuery.V1i1
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILessonInstructionRepository _lessonInstructionRepository;

        public GetLessonQueryHandler(IMapper mapper,
                                     ILessonRepository lessonRepository,
                                     IHomeWorkRepository homeWorkRepository,
                                     ISkillRepository skillRepository,
                                     ILessonModuleRepository lessonModuleRepository,
                                     ICategoryRepository categoryRepository,
                                     ILevelRepository levelRepository,
                                     IVideoRepository videoRepository,
                                     IClassForumRepository classForumRepository,
                                     IDocumentRepository documentRepository,
                                     ILessonInstructionRepository lessonInstructionRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _videoRepository = videoRepository;
            _classForumRepository = classForumRepository;
            _documentRepository = documentRepository;
            _lessonInstructionRepository = lessonInstructionRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            ArgumentNullException.ThrowIfNull(request);

            var lesson = await (from l in _lessonRepository.Queryable
                                join lm in _lessonModuleRepository.Queryable on l.Id equals lm.LessonId
                                where l.Status != Shared.Enums.EnumStatus.Archive
                                group new { lm } by l into g
                                select new LessonModel
                                {
                                    Id = g.Key.Id,
                                    CreatedDate = g.Key.CreatedDate,
                                    CreatedUserId = g.Key.CreatedUserId,
                                    CreatedFullName = g.Key.CreatedFullName,
                                    UpdatedDate = g.Key.UpdatedDate,
                                    UpdatedFullName = g.Key.UpdatedFullName,
                                    UpdatedUserId = g.Key.UpdatedUserId,
                                    ClassForumCount = g.Key.ClassForumCount,
                                    HomeWorkCount = g.Key.HomeWorkCount,
                                    VideoCount = g.Key.VideoCount,
                                    DocumentCount = g.Key.DocumentCount,
                                    InstructionContent = g.Key.InstructionContent,
                                    Name = g.Key.Name,
                                    Status = g.Key.Status,
                                    LevelId = g.Key.LevelId,
                                    ProgramId = g.Key.ProgramId,
                                    LessonInstructions = _lessonInstructionRepository.Queryable.Where(x => x.LessonId == g.Key.Id).Select(x => new Domain.Models.EntityModels.LessonInstructionModel
                                    {
                                        Id = x.Id,
                                        SkillId = x.SkillId,
                                        Instruction = x.Instruction,

                                    }).ToList(),
                                    LessonModules = g.Select(x => new LessonModuleModel
                                    {
                                        Id = x.lm.Id,
                                        Name = x.lm.Name,
                                        Thumbnail = x.lm.Thumbnail,
                                        Description = x.lm.Description,
                                        CreatedDate = x.lm.CreatedDate,
                                        CreatedFullName = x.lm.CreatedFullName,
                                        CreatedUserId = x.lm.CreatedUserId,
                                        UpdatedDate = x.lm.UpdatedDate,
                                        UpdatedFullName = x.lm.UpdatedFullName,
                                        UpdatedUserId = x.lm.UpdatedUserId,
                                        DisplayNumber = x.lm.DisplayNumber,
                                        DisplayOrder = x.lm.DisplayOrder,
                                        OpenOrder = x.lm.OpenOrder,
                                        Percent = x.lm.Percent,
                                        LessonConfigType = x.lm.LessonConfigType,
                                        ClassForumId = x.lm.ClassForumId,
                                        HomeWorkId = x.lm.HomeWorkId,
                                        DocumentId = x.lm.DocumentId,
                                        VideoId = x.lm.VideoId
                                    }).OrderByDescending(x => x.DisplayOrder).ToList(),
                                }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            await SetFieldName(lesson, cancellationToken);

            methodResult.Result = lesson;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetFieldName(LessonModel lesson, CancellationToken cancellationToken)
        {
            var level = await _levelRepository.Queryable.FirstOrDefaultAsync(x => x.Id == lesson.LevelId, cancellationToken);
            lesson.NameLevel = level?.Name;
            var category = await _categoryRepository.Queryable.FirstOrDefaultAsync(x => x.Id == lesson.ProgramId, cancellationToken);
            lesson.NameProgram = category?.Name;

            if (lesson.LessonInstructions != null && lesson.LessonInstructions.Any())
            {
                var skillIds = lesson.LessonInstructions.Where(x => x.SkillId.HasValue).Select(x => x.SkillId!.Value).ToList() ?? new List<Guid>();
                var skills = await _skillRepository.Queryable.WhereBulkContains(skillIds, x => x.Id).AsNoTracking().ToListAsync(cancellationToken);

                foreach (var lessonInstruction in lesson.LessonInstructions)
                {
                    if (lessonInstruction.SkillId.HasValue)
                    {
                        lessonInstruction.SkillName = skills.FirstOrDefault(c => c.Id == lessonInstruction.SkillId)?.Name;
                    }
                }
            }

            if (lesson.LessonModules != null && lesson.LessonModules.Any())
            {
                var classForumIds = lesson.LessonModules.Where(x => x.ClassForumId.HasValue).Select(x => x.ClassForumId!.Value).ToList() ?? new List<Guid>();
                var classForum = await _classForumRepository.Queryable.WhereBulkContains(classForumIds, x => x.Id).Include(c => c.ClassForumFiles).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                var videoIds = lesson.LessonModules.Where(x => x.VideoId.HasValue).Select(x => x.VideoId!.Value).ToList() ?? new List<Guid>();
                var videos = await _videoRepository.Queryable.WhereBulkContains(videoIds, x => x.Id).Include(x => x.VideoTimeCodes.OrderBy(c => c.DisplayTime)).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                var homeWorkIds = lesson.LessonModules.Where(x => x.HomeWorkId.HasValue).Select(x => x.HomeWorkId!.Value).ToList() ?? new List<Guid>();
                var homeWorks = await _homeWorkRepository.Queryable.WhereBulkContains(homeWorkIds, x => x.Id).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                var documentIds = lesson.LessonModules.Where(x => x.DocumentId.HasValue).Select(x => x.DocumentId!.Value).ToList() ?? new List<Guid>();
                var documents = await _documentRepository.Queryable.WhereBulkContains(documentIds, x => x.Id).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var lessonModule in lesson.LessonModules)
                {
                    if (lessonModule.ClassForumId.HasValue)
                    {
                        lessonModule.ClassForum = _mapper.Map<ClassForumModel>(classForum.FirstOrDefault(c => c.Id == lessonModule.ClassForumId));
                    }

                    if (lessonModule.HomeWorkId.HasValue)
                    {
                        lessonModule.HomeWork = _mapper.Map<Domain.Models.EntityModels.HomeWorkModel>(homeWorks.FirstOrDefault(c => c.Id == lessonModule.HomeWorkId));

                    }

                    if (lessonModule.VideoId.HasValue)
                    {
                        lessonModule.Video = _mapper.Map<Domain.Models.EntityModels.VideoModel>(videos.FirstOrDefault(c => c.Id == lessonModule.VideoId));

                    }

                    if (lessonModule.DocumentId.HasValue)
                    {
                        lessonModule.Document = _mapper.Map<DocumentModel>(documents.FirstOrDefault(c => c.Id == lessonModule.DocumentId));

                    }
                }
            }
        }
    }
}
