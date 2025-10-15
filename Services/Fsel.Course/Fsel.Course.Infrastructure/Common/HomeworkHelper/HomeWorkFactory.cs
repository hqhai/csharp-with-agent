// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.HomeworkHelper
{
    using System;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorks;
    using Fsel.Course.Domain.Models.EntityModels;

    public class HomeWorkFactory
    {
        private readonly UpdateHomeWorkCommandModel _createRequest;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;

        protected HomeWorkFactory(UpdateHomeWorkCommandModel createRequest, IMapper mapper, QuestionConverter questionConverter)
        {
            _createRequest = createRequest;
            _mapper = mapper;
            _questionConverter = questionConverter;
        }

        public (bool, HomeWork?) Build(int version = 0, Guid? originalId = null, bool isNew = false, MethodResult<HomeWorkModel>? methodResult = null)
        {
            var homeWork = new HomeWork
            {
                Code = _createRequest.Code,
                Name = _createRequest.Name,
                MediaPost = _createRequest.MediaPost,
                VersionStatus = EnumVersionStatus.LastVersion,
                MediaPostContentRuby = _createRequest.MediaPostContentRuby,
                Version = version,
                LevelId = _createRequest.LevelId,
                ProgramId = _createRequest.ProgramId,
                SkillId = _createRequest.SkillId,
            };

            homeWork.OriginalId = originalId.HasValue ? originalId.Value : homeWork.Id;

            if (_createRequest.Questions != null)
            {
                foreach (var question in _createRequest.Questions)
                {
                    var newQuestion = _mapper.Map<Question>(question);
                    if (isNew)
                    {
                        newQuestion.Id = default;
                    }
                    var method = _questionConverter.HandleQuestion(newQuestion, true);
                    if (!method.IsOK)
                    {
                        methodResult?.AddErrorBadRequest(method.ErrorMessages);
                        return (false, null);
                    }
                    homeWork.HomeWorkQuestions.Add(new HomeWorkQuestion
                    {
                        Question = method.Result
                    });
                }
            }

            return (true, homeWork);
        }

        public static HomeWorkFactory Create(UpdateHomeWorkCommandModel createRequest, IMapper mapper, QuestionConverter questionConverter)
        {
            return new HomeWorkFactory(createRequest, mapper, questionConverter);
        }
    }
}
