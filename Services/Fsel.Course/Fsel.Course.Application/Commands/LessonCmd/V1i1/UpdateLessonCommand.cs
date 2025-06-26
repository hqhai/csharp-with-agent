// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using System.Text.RegularExpressions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
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
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly LessonConverter _lessonConverter;
        private readonly IMediator _mediator;

        public UpdateLessonCommandHandler(ILessonRepository lessonRepository,
                                          IMapper mapper,
                                          ICategoryRepository categoryRepository,
                                          ILevelRepository levelRepository,
                                          LessonConverter lessonConverter,
                                          IMediator mediator)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _lessonConverter = lessonConverter;
            _mediator = mediator;
        }

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            Regex regexCode = new Regex("^[a-zA-Z0-9._]+$");
            Regex regexInstructionContent = new Regex("^[^<>&#*]{1,2000}$");

            #region Validate
            var lesson = await _lessonRepository.Queryable
                                                .Include(x => x.LessonInstructions)
                                                .Include(x => x.LessonModules)
                                                .ThenInclude(x => x.ClassForum)
                                                .ThenInclude(x => x.ClassForumFiles)
                                                .Include(x => x.LessonModules)
                                                .ThenInclude(x => x.Document)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson), request.Id);
                return methodResult;
            }

            if (lesson.Status == EnumStatus.Active)
            {
                var handlerLessonActive = await HandlerLessonActive(request, lesson, cancellationToken);
                if (!handlerLessonActive.IsOK)
                {
                    methodResult.AddErrorBadRequest(handlerLessonActive.ErrorMessages);
                    return methodResult;
                }

                methodResult.Result = handlerLessonActive.Result;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeNotNullOrEmpty), request.Name);
                return methodResult;
            }

            if (!regexCode.IsMatch(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeNotValid), request.Name);
                return methodResult;
            }

            var checkCode = await _lessonRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Status != EnumStatus.InActive && x.Name == request.Name.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeAlreadyExist), request.Name);
                return methodResult;
            }

            var checkProgram = await _categoryRepository.AnyGuidAsync(request.ProgramId);
            if (!checkProgram)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ProgramId));
                return methodResult;
            }

            var checkLevel = await _levelRepository.AnyGuidAsync(request.LevelId);
            if (!checkLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LevelId));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.InstructionContent) && !regexInstructionContent.IsMatch(request.InstructionContent))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.InstructionContentNotValid), request.InstructionContent);
                return methodResult;
            }

            if (request.LessonModules == null || !request.LessonModules.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonModuleNotNull), nameof(request.LessonModules));
                return methodResult;
            }

            _mapper.Map(request, lesson);
            if (!lesson.IsValid())
            {
                methodResult.AddErrorBadRequest(lesson.ErrorMessages);
                return methodResult;
            }
            #endregion

            if (request.LessonInstructions != null && request.LessonInstructions.Any())
            {
                var lessonInstruction = await _lessonConverter.LessonInstructionHandler(request.LessonInstructions, lesson, cancellationToken);
                if (!lessonInstruction.IsOK)
                {
                    methodResult.AddErrorBadRequest(lessonInstruction.ErrorMessages);
                    return methodResult;
                }
            }

            var lessonModule = await _lessonConverter.LessonModuleHandler(request.LessonModules, lesson, cancellationToken);
            if (!lessonModule.IsOK)
            {
                methodResult.AddErrorBadRequest(lessonModule.ErrorMessages);
                return methodResult;
            }

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                lesson.VideoCount = lesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.Video);
                lesson.ClassForumCount = lesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.ClassForum);
                lesson.HomeWorkCount = lesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.HomeWork);
                lesson.DocumentCount = lesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.Document);

                _lessonRepository.Update(lesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<LessonModel>> HandlerLessonActive(UpdateLessonCommandModel request, Lesson lesson, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            // New lesson
            var newLesson = await _mediator.Send(new CreateLessonCommand
            {
                Name = request.Name,
                InstructionContent = request.InstructionContent,
                LevelId = request.LevelId,
                ProgramId = request.ProgramId,
                LessonInstructions = request.LessonInstructions,
                LessonModules = request.LessonModules
            }, cancellationToken);

            if (!newLesson.IsOK)
            {
                methodResult.AddErrorBadRequest(newLesson.ErrorMessages);
                return methodResult;
            }

            // InActive Lesson cũ
            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                lesson.Status = EnumStatus.InActive;
                _lessonRepository.Update(lesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = newLesson.Result;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
