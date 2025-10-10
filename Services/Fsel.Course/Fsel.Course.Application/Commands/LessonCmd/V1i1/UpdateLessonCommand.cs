// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Common.LessonHelpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateLessonCommand : UpdateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly LessonConverter _lessonConverter;
        private readonly IVersionEntityUpdater<Lesson> _versionEntityUpdater;
        private readonly IUnitRepository _unitRepository;

        public UpdateLessonCommandHandler(ILessonRepository lessonRepository,
                                          IMapper mapper,
                                          LessonConverter lessonConverter,
                                          IVersionEntityUpdater<Lesson> versionEntityUpdater,
                                          IUnitRepository unitRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _lessonConverter = lessonConverter;
            _versionEntityUpdater = versionEntityUpdater;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            var lesson = await _lessonRepository.Queryable
                                                .Include(x => x.LessonModules)
                                                .Include(x => x.LessonInstructions)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            var validate = await _lessonConverter.ValidateLesson(request, true, lesson.OriginalId, cancellationToken);
            if (!validate.IsOK)
            {
                methodResult.AddErrorBadRequest(validate.ErrorMessages);
                return methodResult;
            }

            bool isCheckUnit = await _unitRepository.Queryable
                                                    .AnyAsync(x => x.UnitModules.Any(c => c.UnitConfigType == EnumUnitConfigType.Lesson && c.OriginalId == lesson.OriginalId), cancellationToken);

            var lessonConverter = await _lessonConverter.LessonModuleHandler(request.LessonModules!, true, isCheckUnit, cancellationToken);
            if (!lessonConverter.IsOK)
            {
                methodResult.AddErrorBadRequest(lessonConverter.ErrorMessages);
                return methodResult;
            }

            var newVersionLesson = LessonFactory.Create(request).Build();
            if (!newVersionLesson.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionLesson.ErrorMessages);
                return methodResult;
            }

            await _versionEntityUpdater.UpdateEntity(lesson, newVersionLesson,
                async (_, entity) => isCheckUnit,
                async (oldEntity, newEntity) =>
                {
                    oldEntity.Name = newEntity.Name;
                    oldEntity.InstructionContent = newEntity.InstructionContent;
                    oldEntity.VideoCount = newEntity.VideoCount;
                    oldEntity.ClassForumCount = newEntity.ClassForumCount;
                    oldEntity.HomeWorkCount = newEntity.HomeWorkCount;
                    oldEntity.DocumentCount = newEntity.DocumentCount;
                    oldEntity.Status = newEntity.Status;
                    oldEntity.LevelId = newEntity.LevelId;
                    oldEntity.ProgramId = newEntity.ProgramId;

                    LessonModuleHandler(lesson, newVersionLesson, newEntity, oldEntity);
                    LessonInstructionHandler(lesson, newVersionLesson, newEntity, oldEntity);

                    await Task.Yield();
                }
            );

            methodResult.Result = _mapper.Map<LessonModel>(lesson);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void LessonModuleHandler(Lesson lesson, Lesson newVersionLesson, Lesson newEntity, Lesson oldEntity)
        {
            var keys = newVersionLesson.LessonModules
                .Select(x => (x.OriginalId, x.LessonConfigType, x.OpenOrder))
                .ToHashSet();

            var removedModules = lesson.LessonModules
                .Where(u => !keys.Contains((u.OriginalId, u.LessonConfigType, u.OpenOrder)))
                .ToList();
            if (removedModules.Any())
            {
                removedModules.ForEach(module =>
                {
                    lesson.LessonModules.Remove(module);
                });
            }

            foreach (var module in newEntity.LessonModules)
            {
                var existingModule = lesson.LessonModules.FirstOrDefault(m => m.OriginalId == module.OriginalId && m.LessonConfigType == module.LessonConfigType);
                if (existingModule != null)
                {
                    existingModule.Name = module.Name;
                    existingModule.Thumbnail = module.Thumbnail;
                    existingModule.Description = module.Description;
                    existingModule.Percent = module.Percent;
                    existingModule.OpenOrder = module.OpenOrder;
                    existingModule.DisplayOrder = module.DisplayOrder;
                    existingModule.DisplayNumber = module.DisplayNumber;
                    existingModule.OriginalId = module.OriginalId;
                }
                else
                {
                    oldEntity.LessonModules.Add(module);
                }
            }
        }

        private static void LessonInstructionHandler(Lesson lesson, Lesson newVersionLesson, Lesson newEntity, Lesson oldEntity)
        {
            var removedInstructions = lesson.LessonInstructions.ExceptBy(newVersionLesson.LessonInstructions.Select(x => x.SkillId), u => u.SkillId).ToList();
            if (removedInstructions.Any())
            {
                removedInstructions.ForEach(instruction =>
                {
                    lesson.LessonInstructions.Remove(instruction);
                });
            }

            foreach (var instruction in newEntity.LessonInstructions)
            {
                var existingInstruction = lesson.LessonInstructions.FirstOrDefault(m => m.SkillId == instruction.SkillId);
                if (existingInstruction != null)
                {
                    existingInstruction.Instruction = instruction.Instruction;
                }
                else
                {
                    oldEntity.LessonInstructions.Add(instruction);
                }
            }
        }
    }
}
