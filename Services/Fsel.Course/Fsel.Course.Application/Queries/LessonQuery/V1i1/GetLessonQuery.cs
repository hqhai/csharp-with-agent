// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LessonQuery.V1i1
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
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
        private readonly ISkillRepository _skillRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILessonInstructionRepository _lessonInstructionRepository;

        public GetLessonQueryHandler(IMapper mapper,
                                     ILessonRepository lessonRepository,
                                     ISkillRepository skillRepository,
                                     ILessonModuleRepository lessonModuleRepository,
                                     ICategoryRepository categoryRepository,
                                     ILevelRepository levelRepository,
                                     IClassForumRepository classForumRepository,
                                     IDocumentRepository documentRepository,
                                     ILessonInstructionRepository lessonInstructionRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
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
                                where !l.IsArchive
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
                                    OriginalId = g.Key.OriginalId,
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
                                        OriginalId = x.lm.OriginalId
                                    }).OrderBy(x => x.DisplayOrder).ToList(),
                                }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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
            var level = await _levelRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lesson.LevelId, cancellationToken);
            lesson.NameLevel = level?.Name;
            var category = await _categoryRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lesson.ProgramId, cancellationToken);
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
                var classForumIds = lesson.LessonModules.Where(x => x.LessonConfigType == EnumLessonConfigType.ClassForum).Select(x => x.OriginalId).ToList() ?? new List<Guid>();
                var classForum = await _classForumRepository.Queryable.WhereBulkContains(classForumIds, x => x.OriginalId).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).Include(c => c.ClassForumFiles).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                var documentIds = lesson.LessonModules.Where(x => x.LessonConfigType == EnumLessonConfigType.Document).Select(x => x.OriginalId).ToList() ?? new List<Guid>();
                var documents = await _documentRepository.Queryable.WhereBulkContains(documentIds, x => x.OriginalId).Where(x => x.VersionStatus == EnumVersionStatus.LastVersion).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var lessonModule in lesson.LessonModules)
                {
                    switch (lessonModule.LessonConfigType)
                    {
                        case EnumLessonConfigType.ClassForum:
                            lessonModule.ClassForum = _mapper.Map<ClassForumModel>(classForum.FirstOrDefault(c => c.OriginalId == lessonModule.OriginalId));
                            break;
                        case EnumLessonConfigType.Document:
                            lessonModule.Document = _mapper.Map<DocumentModel>(documents.FirstOrDefault(c => c.OriginalId == lessonModule.OriginalId));
                            break;
                    }
                }
            }
        }
    }
}
