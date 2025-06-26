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
                                join li in _lessonInstructionRepository.Queryable on l.Id equals li.LessonId
                                join lm in _lessonModuleRepository.Queryable on l.Id equals lm.LessonId
                                where l.Status != Shared.Enums.EnumStatus.Archive
                                group new { lm, li } by l into g
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
                                    NameLevel = _levelRepository.Queryable.FirstOrDefault(x => x.Id == g.Key.LevelId) != null ?
                                                _levelRepository.Queryable.FirstOrDefault(x => x.Id == g.Key.LevelId)!.Name : default,
                                    ProgramId = g.Key.ProgramId,
                                    NameProgram = _categoryRepository.Queryable.FirstOrDefault(x => x.Id == g.Key.ProgramId) != null ?
                                                  _categoryRepository.Queryable.FirstOrDefault(x => x.Id == g.Key.ProgramId)!.Name : default,
                                    LessonInstructions = _lessonInstructionRepository.Queryable.Where(x => x.LessonId == g.Key.Id).Select(x => new Domain.Models.EntityModels.LessonInstructionModel
                                    {
                                        Id = x.Id,
                                        SkillId = x.SkillId,
                                        Instruction = x.Instruction,
                                        SkillName = _skillRepository.Queryable.FirstOrDefault(c => c.Id == x.SkillId) != null ?
                                                    _skillRepository.Queryable.FirstOrDefault(c => c.Id == x.SkillId)!.Name : default,
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
                                        ClassForum = _mapper.Map<ClassForumModel>(_classForumRepository.Queryable.FirstOrDefault(c => c.Id == x.lm.ClassForumId)),
                                        HomeWorkId = x.lm.HomeWorkId,
                                        HomeWork = _mapper.Map<Domain.Models.EntityModels.HomeWorkModel>(_homeWorkRepository.Queryable.FirstOrDefault(c => c.Id == x.lm.HomeWorkId)),
                                        DocumentId = x.lm.DocumentId,
                                        Document = _mapper.Map<DocumentModel>(_documentRepository.Queryable.FirstOrDefault(c => c.Id == x.lm.DocumentId)),
                                        VideoId = x.lm.VideoId,
                                        Video = _mapper.Map<Domain.Models.EntityModels.VideoModel>(_videoRepository.Queryable.FirstOrDefault(c => c.Id == x.lm.VideoId))
                                    }).OrderByDescending(x => x.CreatedDate).ToList(),
                                }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            methodResult.Result = lesson;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
