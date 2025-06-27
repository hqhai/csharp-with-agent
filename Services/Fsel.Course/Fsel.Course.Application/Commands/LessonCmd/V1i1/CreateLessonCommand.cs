// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
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

    public class CreateLessonCommand : CreateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly LessonConverter _lessonConverter;

        public CreateLessonCommandHandler(ILessonRepository lessonRepository,
                                          IMapper mapper,
                                          ICategoryRepository categoryRepository,
                                          ILevelRepository levelRepository,
                                          LessonConverter lessonConverter)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _lessonConverter = lessonConverter;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            Regex regexCode = new Regex("^[a-zA-Z0-9._]+$");
            Regex regexInstructionContent = new Regex("^[^<>&#*]{1,2000}$");

            #region Validate
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

            var checkCode = await _lessonRepository.Queryable.AnyAsync(x => x.Name == request.Name.Trim(), cancellationToken);
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

            var newLesson = _mapper.Map<Lesson>(request);
            if (!newLesson.IsValid())
            {
                methodResult.AddErrorBadRequest(newLesson.ErrorMessages);
                return methodResult;
            }
            #endregion

            if (request.LessonInstructions != null && request.LessonInstructions.Any())
            {
                var lessonInstruction = await _lessonConverter.LessonInstructionHandler(request.LessonInstructions, newLesson, cancellationToken);
                if (!lessonInstruction.IsOK)
                {
                    methodResult.AddErrorBadRequest(lessonInstruction.ErrorMessages);
                    return methodResult;
                }
            }

            var lessonModule = await _lessonConverter.LessonModuleHandler(request.LessonModules, newLesson, cancellationToken);
            if (!lessonModule.IsOK)
            {
                methodResult.AddErrorBadRequest(lessonModule.ErrorMessages);
                return methodResult;
            }

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                newLesson.VideoCount = newLesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.Video);
                newLesson.ClassForumCount = newLesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.ClassForum);
                newLesson.HomeWorkCount = newLesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.HomeWork);
                newLesson.DocumentCount = newLesson.LessonModules.Count(x => x.LessonConfigType == EnumLessonConfigType.Document);
                newLesson.Status = EnumStatus.InActive;

                _lessonRepository.Add(newLesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<LessonModel>(newLesson);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
