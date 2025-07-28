// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AutoApprovalClassForumCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassForumResulId { get; set; }

        public Guid? ClassForumDetailResulId { get; set; }
    }

    public class AutoApprovalClassForumCommandHandler : IRequestHandler<AutoApprovalClassForumCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private const string AIAccessContext = "NO";
        private const string ApprovalAiModel = "gpt-4o-mini";
        private readonly AppSetting _appSetting;
        public AutoApprovalClassForumCommandHandler(IMediator mediator, IClassForumResultRepository classForumResultRepository, AppSetting appSetting)
        {
            _mediator = mediator;
            _classForumResultRepository = classForumResultRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(AutoApprovalClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.ClassForum)
                                                                    .Include(x => x.ClassForumDetailResults)
                                                                    .FirstOrDefaultAsync(x => x.Id == request.ClassForumResulId, cancellationToken);

            #region Validate
            if (classForumResult == null)
            {
                return methodResult;
            }

            if (classForumResult.Status != Domain.Enums.EnumClassForumResultStatus.Pending)
            {
                return methodResult;
            }
            #endregion

            //var aiApprovalAndComment = await GetAIModeration(_mediator, classForumResult, cancellationToken);
            bool isForbidden = request.ClassForumDetailResulId.HasValue && classForumResult.ClassForumDetailResults.Any(x => x.Id == request.ClassForumDetailResulId && (x.IsForbiddenWork || x.IsForbiddenImage));
            await UpdateStatusClassForumAfterApproval(classForumResult.Id, _mediator, isForbidden, cancellationToken);

            return methodResult;
        }


        /// <summary>
        /// Phê duyệt nội dung bằng AI
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="classForumResult"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<AIApprovalModel>> GetAIModeration(IMediator mediator, ClassForumResult classForumResult, CancellationToken cancellationToken)
        {

            string role = File.ReadAllText(ResourceSettings.AIClassForumRole);
            string systemConfig = File.ReadAllText(ResourceSettings.AIClassForumInstruction);
            var userAiConfig = role!.Replace("{0}", classForumResult?.WordContent ?? string.Empty, StringComparison.CurrentCulture);
            var aiApprovalAndComment = new List<AIApprovalModel>();
            var aiApprovalModel = _appSetting.OpenAiConfig?.ApprovalAIModel ?? ApprovalAiModel;

            if (mediator == null)
            {
                return aiApprovalAndComment;
            }

            var aIResponse = await mediator.Send(new SubmitAICommand
            {
                SettingModel = aiApprovalModel,
                SettingTemperature = classForumResult!.ClassForum!.SettingTemperature,
                SettingFrequecy = classForumResult!.ClassForum!.SettingFrequecy,
                SettingWordMaxLength = classForumResult!.ClassForum!.SettingWordMaxLength,
                SettingPresence = classForumResult!.ClassForum!.SettingPresence,
                SettingTopP = classForumResult!.ClassForum!.SettingTopP,
                SystemRoleAlConfig = userAiConfig,
                UserAIConfig = systemConfig,
            }, cancellationToken).ConfigureAwait(false);


            aiApprovalAndComment = !string.IsNullOrEmpty(aIResponse) ? ConvertHelper.Deserialize<List<AIApprovalModel>>(Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse)) : aiApprovalAndComment;

            if (aiApprovalAndComment == null)
            {
                return new List<AIApprovalModel>();
            }

            return aiApprovalAndComment;
        }


        /// <summary>
        /// Tự động phê duyệt hoặc từ chối bài viết ClassForum
        /// </summary>
        /// <param name="aiApprovalAndComment"></param>
        /// <returns></returns>
        public async Task UpdateStatusClassForumAfterApproval(Guid classForumResultId, IMediator mediator, bool isForbidden, CancellationToken cancellationToken)
        {
            if (mediator == null)
            {
                return;
            }

            bool isApprove = !isForbidden;
            await mediator.Send(new ApproveClassForumPenddingCommand
            {
                ClassForumResultId = classForumResultId,
                IsApprove = isApprove,
            }, cancellationToken);
        }
    }

}
