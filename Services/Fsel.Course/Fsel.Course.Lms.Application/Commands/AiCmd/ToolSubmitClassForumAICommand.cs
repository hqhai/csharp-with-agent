// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Shared.Helpers;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ToolSubmitClassForumAICommand : IRequest<MethodResult<bool>>
    {
        public Guid? UserId { get; set; }

        public string? LessonName { get; set; }
    }

    public class ToolSubmitClassForumAICommandHandler : IRequestHandler<ToolSubmitClassForumAICommand, MethodResult<bool>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;

        public ToolSubmitClassForumAICommandHandler(ILessonResultRepository lessonResultRepository, IMediator mediator, IClassForumDetailResultRepository classForumDetailResultRepository, AppSetting appSetting, ILessonRepository lessonRepository, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _appSetting = appSetting;
            _lessonRepository = lessonRepository;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolSubmitClassForumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> result = new MethodResult<bool>();

            var lessonId = await _lessonRepository.Queryable.FirstOrDefaultAsync(x => x.Name == request.LessonName, cancellationToken).Select(x => x.Id);

            var lessonResultId = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == lessonId && x.CreatedUserId == request.UserId, cancellationToken).Select(x => x.Id);

            var classForumResultNeedUpdate = await _classForumResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);

            if (classForumResultNeedUpdate == null)
            {
                result.Result = false;
                return result;
            }

            var classForumNeedUpdate = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.Id == classForumResultNeedUpdate!.ClassForumId, cancellationToken);

            var userAiConfig = classForumNeedUpdate!.UserAlConfig?.Replace("{0}", classForumResultNeedUpdate.WordContent, StringComparison.CurrentCulture);
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = classForumNeedUpdate!.SettingModel,
                SettingTemperature = classForumNeedUpdate!.SettingTemperature,
                SettingFrequecy = classForumNeedUpdate!.SettingFrequecy,
                SettingWordMaxLength = classForumNeedUpdate!.SettingWordMaxLength,
                SettingPresence = classForumNeedUpdate!.SettingPresence,
                SettingTopP = classForumNeedUpdate!.SettingTopP,
                SystemRoleAlConfig = classForumNeedUpdate!.SystemRoleAlConfig,
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);

            var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse));

            await _classForumDetailResultRepository.ExecuteTransactionAsync(async () =>
            {
                var classForumDetailResult = await _classForumDetailResultRepository.Queryable.FirstOrDefaultAsync(x => x.ClassForumResultId == classForumResultNeedUpdate.Id, cancellationToken);
                if (classForumDetailResult != null)
                {
                    classForumDetailResult.GradingAlFeedback = classForumAIs != null ? ConvertHelper.Serialize(GetClassForumAIs(classForumAIs)) : default;
                    await _classForumDetailResultRepository.BulkUpdateList(new List<ClassForumDetailResult> { classForumDetailResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.WordContent, c.Content, c.WordCount, c.SubmissionCount, c.ProcessDate, c.CompletionDate, c.Status, c.ClassForumResultId };
                    });
                }
                return result;
            });

            return result;
        }

        private static IList<ClassForumAIModel>? GetClassForumAIs(List<ClassForumAIModel>? classForumAIs)
        {
            if (classForumAIs == null || !classForumAIs.Any())
            {
                return classForumAIs;
            }

            foreach (var item in classForumAIs)
            {
                item.SuccessCriteriaItemFix = ConvertDataToStrings(item.SuccessCriteriaItemFix);
                item.SuccessCriteriaItemEvidence = ConvertDataToStrings(item.SuccessCriteriaItemEvidence);
            }
            return classForumAIs;
        }

        private static IList<string> ConvertDataToStrings(object? data)
        {
            var listStr = data.Deserialize<IList<string>>();
            if (listStr != null)
            {
                return listStr.ToList();
            }
            return new List<string> { data?.ToString() ?? string.Empty };
        }
    }
}
