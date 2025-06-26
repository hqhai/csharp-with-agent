// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
    using Fsel.Course.Domain.Models.CommandModels.LessonModules;
    using Microsoft.EntityFrameworkCore;

    public class LessonConverter
    {
        private readonly Dictionary<EnumLessonConfigType, Func<CreateLessonModuleCommandModel, LessonModule, CancellationToken, Task<MethodResult<bool>>>> _lessonModule;
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IDocumentRepository _documentRepository;

        public LessonConverter(IMapper mapper,
                               IVideoRepository videoRepository,
                               IHomeWorkRepository homeWorkRepository,
                               ISkillRepository skillRepository,
                               IClassForumRepository classForumRepository,
                               IDocumentRepository documentRepository)
        {
            _lessonModule = new Dictionary<EnumLessonConfigType, Func<CreateLessonModuleCommandModel, LessonModule, CancellationToken, Task<MethodResult<bool>>>>()
            {
                { EnumLessonConfigType.Video, LessonModuleVideoHandler },
                { EnumLessonConfigType.ClassForum, LessonModuleClassForumHandler},
                { EnumLessonConfigType.HomeWork, LessonModuleHomeWorkHandler},
                { EnumLessonConfigType.Document, LessonModuleDocumentHandler}
            };
            _mapper = mapper;
            _videoRepository = videoRepository;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
            _classForumRepository = classForumRepository;
            _documentRepository = documentRepository;
        }

        public async Task<MethodResult<bool>> LessonInstructionHandler(IList<CreateLessonInstructionCommandModel> lessonInstructions, Lesson lesson, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            ArgumentNullException.ThrowIfNull(lessonInstructions);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            Regex regexInstruction = new Regex("^[^<>&#*]{1,1000}$");

            foreach (var request in lessonInstructions)
            {
                if (!string.IsNullOrEmpty(request.Instruction) && !regexInstruction.IsMatch(request.Instruction))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.InstructionNotValid), request.Instruction);
                    return methodResult;
                }

                if (request.Id.HasValue)
                {
                    var lessonInstruction = lesson.LessonInstructions.FirstOrDefault(x => x.Id == request.Id);
                    if (lessonInstruction == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonInstruction));
                        return methodResult;
                    }

                    _mapper.Map(request, lessonInstruction);
                    if (!lessonInstruction.IsValid())
                    {
                        methodResult.AddErrorBadRequest(lessonInstruction.ErrorMessages);
                        return methodResult;
                    }
                }
                else
                {
                    var newLessonInstruction = _mapper.Map<LessonInstruction>(request);
                    if (!newLessonInstruction.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newLessonInstruction.ErrorMessages);
                        return methodResult;
                    }

                    lesson.LessonInstructions.Add(newLessonInstruction);
                }
            }

            return await Task.FromResult(methodResult);
        }

        public async Task<MethodResult<bool>> LessonModuleHandler(IList<CreateLessonModuleCommandModel> lessonModules, Lesson lesson, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            ArgumentNullException.ThrowIfNull(lessonModules);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            double minPercent = 99;
            double maxPercent = 100;
            var sumPercent = lessonModules.Sum(x => x.Percent);

            if (sumPercent < minPercent && sumPercent > maxPercent)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.PercentNotValid), nameof(sumPercent), sumPercent);
                return methodResult;
            }

            foreach (var request in lessonModules)
            {
                LessonModule lessonModule = new LessonModule();
                if (request.Id.HasValue)
                {
                    var lessonModuleQuery = lesson.LessonModules.FirstOrDefault(x => x.Id == request.Id);
                    if (lessonModuleQuery == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonModule));
                        return methodResult;
                    }

                    lessonModule = lessonModuleQuery;
                    _mapper.Map(request, lessonModule);

                    if (!lessonModule.IsValid())
                    {
                        methodResult.AddErrorBadRequest(lessonModule.ErrorMessages);
                        return methodResult;
                    }
                }
                else
                {
                    lessonModule = _mapper.Map<LessonModule>(request);
                    if (!lessonModule.IsValid())
                    {
                        methodResult.AddErrorBadRequest(lessonModule.ErrorMessages);
                        return methodResult;
                    }

                    lesson.LessonModules.Add(lessonModule);
                }

                var fun = _lessonModule[request.LessonConfigType];
                var handler = await fun(request, lessonModule, cancellationToken);
                if (!handler.IsOK)
                {
                    methodResult.AddErrorBadRequest(handler.ErrorMessages);
                    return methodResult;
                }
            }

            return await Task.FromResult(methodResult);
        }

        private async Task<MethodResult<bool>> LessonModuleVideoHandler(CreateLessonModuleCommandModel request, LessonModule lessonModule, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!request.VideoId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.VideoIdNotNull), nameof(request.VideoId), request.VideoId);
                return methodResult;
            }

            var checkVideo = await _videoRepository.Queryable.AnyAsync(x => x.Id == request.VideoId, cancellationToken);
            if (!checkVideo)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkVideo));
                return methodResult;
            }
            return methodResult;
        }

        private async Task<MethodResult<bool>> LessonModuleHomeWorkHandler(CreateLessonModuleCommandModel request, LessonModule lessonModule, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!request.HomeWorkId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.HomeWorkIdNotNull), nameof(request.HomeWorkId), request.HomeWorkId);
                return methodResult;
            }

            var checkHomeWork = await _homeWorkRepository.Queryable.AnyAsync(x => x.Id == request.HomeWorkId, cancellationToken);
            if (!checkHomeWork)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkHomeWork));
                return methodResult;
            }
            return methodResult;
        }

        private async Task<MethodResult<bool>> LessonModuleDocumentHandler(CreateLessonModuleCommandModel request, LessonModule lessonModule, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.Document == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.DocumentNotNull), nameof(request.Document));
                return methodResult;
            }

            if (request.Document.Files == null || !request.Document.Files.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.DocumentNotNull), nameof(request.Document.Files));
                return methodResult;
            }

            foreach (var file in request.Document.Files)
            {
                if (string.IsNullOrEmpty(file.Name) || string.IsNullOrEmpty(file.FilePath))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.NameFileOrFilePathNull), nameof(file.FilePath));
                    return methodResult;
                }

                if (file.Name.Length > 150)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.NameFileMaxLength), nameof(file.Name));
                    return methodResult;
                }

                if (file.FilePath.Length > 3000)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.FilePathMaxLength), nameof(file.FilePath));
                    return methodResult;
                }
            }

            if (lessonModule.Document != null)
            {
                _mapper.Map(request.Document, lessonModule.Document);
            }
            else
            {
                lessonModule.Document = _mapper.Map<Document>(request.Document);
            }

            if (!lessonModule.Document.IsValid())
            {
                methodResult.AddErrorBadRequest(lessonModule.Document.ErrorMessages);
                return methodResult;
            }

            return await Task.FromResult(methodResult);
        }

        private async Task<MethodResult<bool>> LessonModuleClassForumHandler(CreateLessonModuleCommandModel request, LessonModule lessonModule, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.ClassForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.ClassForumNotNull), nameof(request.ClassForum));
                return methodResult;
            }

            if (request.ClassForum.IsAlFeedBack && (string.IsNullOrEmpty(request.ClassForum.SystemRoleAlConfig) && string.IsNullOrEmpty(request.ClassForum.UserAlConfig)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.AlConfigNotNull), nameof(request.ClassForum));
                return methodResult;
            }

            var checkSkill = await _skillRepository.AnyGuidAsync(request.ClassForum.SkillId);
            if (!checkSkill)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ClassForum.SkillId), request.ClassForum.SkillId);
                return methodResult;
            }

            if (lessonModule.ClassForum != null)
            {
                var classForum = _mapper.Map(request.ClassForum, lessonModule.ClassForum);
                _classForumRepository.Update(classForum);
            }
            else
            {
                lessonModule.ClassForum = _mapper.Map<ClassForum>(request.ClassForum);
            }

            if (request.ClassForum.FilePaths != null && request.ClassForum.FilePaths.Count > 0)
            {
                lessonModule.ClassForum.ClassForumFiles = request.ClassForum.FilePaths.Select(x => new ClassForumFile
                {
                    FilePath = x,
                }).ToList();
            }

            if (!lessonModule.ClassForum.IsValid())
            {
                methodResult.AddErrorBadRequest(lessonModule.ClassForum.ErrorMessages);
                return methodResult;
            }

            await _classForumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await Task.FromResult(methodResult);
        }
    }
}
