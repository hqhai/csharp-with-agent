// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Common.HomeworkHelper;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateHomeWorkCommand : UpdateHomeWorkCommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class UpdateHomeWorkCommandHandler : IRequestHandler<UpdateHomeWorkCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IVersionEntityUpdater<HomeWork> _versionEntityUpdater;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;

        public UpdateHomeWorkCommandHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , QuestionConverter questionConverter
            , IQuestionRepository questionRepository
            , ISkillRepository skillRepository,
            ICategoryRepository categoryRepository,
            ILevelRepository levelRepository,
            IVersionEntityUpdater<HomeWork> versionEntityUpdater,
            IHomeWorkQuestionRepository homeWorkQuestionRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionConverter = questionConverter;
            _questionRepository = questionRepository;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _versionEntityUpdater = versionEntityUpdater;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(UpdateHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            if (request.Questions == null || request.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Questions));
                return methodResult;
            }

            if (request.SkillId.HasValue)
            {
                var skillExists = await _skillRepository.AnyGuidAsync(request.SkillId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.SkillId), request.SkillId);
                    return methodResult;
                }
            }
            if (request.ProgramId.HasValue)
            {
                var skillExists = await _categoryRepository.AnyGuidAsync(request.ProgramId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ProgramId), request.ProgramId);
                    return methodResult;
                }
            }

            if (request.LevelId.HasValue)
            {
                var skillExists = await _levelRepository.AnyGuidAsync(request.LevelId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LevelId), request.LevelId);
                    return methodResult;
                }
            }

            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x.HomeWorkQuestions.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.Question)
                            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            if (await _homeWorkRepository.Queryable.AnyAsync(p => p.Code == request.Code && p.Id != homeWork.Id && p.OriginalId != homeWork.OriginalId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            bool isUsed = await _homeWorkRepository.IsUsingByClient(homeWork.Id);

            var build = HomeWorkFactory.Create(request, _mapper, _questionConverter).Build(0, null, isUsed, methodResult);
            if (!build.Item1)
            {
                return methodResult;
            }
            var newVersionHomeWork = build.Item2;

            if (newVersionHomeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(newVersionHomeWork));
                return methodResult;
            }

            if (!newVersionHomeWork.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionHomeWork.ErrorMessages);
                return methodResult;
            }

            await _versionEntityUpdater.UpdateEntity(homeWork, newVersionHomeWork,
                async (_, entity) => await _homeWorkRepository.IsUsingByClient(entity.Id),
                async (oldEntity, newEntity) =>
                {
                    oldEntity.Code = newEntity.Code;
                    oldEntity.Name = newEntity.Name;
                    oldEntity.MediaPost = newEntity.MediaPost;
                    oldEntity.LevelId = newEntity.LevelId;
                    oldEntity.ProgramId = newEntity.ProgramId;
                    oldEntity.SkillId = newEntity.SkillId;
                    oldEntity.MediaPostContentRuby = newEntity.MediaPostContentRuby;

                    var removedModules = homeWork.HomeWorkQuestions
                                         .ExceptBy(newVersionHomeWork.HomeWorkQuestions.Where(x => x.Question != null).Select(x => x.Question!.Id), u => u.QuestionId)
                                         .ToList();

                    foreach (var removed in removedModules)
                    {
                        homeWork.HomeWorkQuestions.Remove(removed);
                    }

                    foreach (var item in newEntity.HomeWorkQuestions)
                    {
                        if (item.Question != null && item.Question.Id != default)
                        {
                            var existing = homeWork.HomeWorkQuestions
                                .FirstOrDefault(p => p.QuestionId == item.Question.Id);

                            if (existing != null && existing.Question != null)
                            {
                                existing.Question.QuestionType = item.Question.QuestionType;
                                existing.Question.Ungraded = item.Question.Ungraded;
                                existing.Question.Explanation = item.Question.Explanation;
                                existing.Question.CorrectTotal = item.Question.CorrectTotal;
                                existing.Question.ConfigStr = item.Question.ConfigStr;
                                existing.Question.SubQuestionIndexsStr = item.Question.SubQuestionIndexsStr;
                                existing.Question.Description = item.Question.Description;
                            }
                        }
                        else
                        {
                            homeWork.HomeWorkQuestions.Add(item);
                        }
                    }
                }

            );
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
