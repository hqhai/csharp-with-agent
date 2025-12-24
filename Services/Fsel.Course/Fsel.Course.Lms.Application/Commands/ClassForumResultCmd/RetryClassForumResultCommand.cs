// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RetryClassForumResultCommand : RetryClassForumResultCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class RetryClassForumResultCommandHandler : IRequestHandler<RetryClassForumResultCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMediator _mediator;
        private SubmitClassForumGradingPublisher _submitClassForumGradingPublisher;

        public RetryClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, IClassForumRepository classForumRepository, IMediator mediator, SubmitClassForumGradingPublisher submitClassForumGradingPublisher)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _classForumRepository = classForumRepository;
            _mediator = mediator;
            _submitClassForumGradingPublisher = submitClassForumGradingPublisher;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(RetryClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.Id);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.Where(x => x.Id == classForumResult.ClassForumId).FirstOrDefaultAsync(cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            if (request.FilePaths != null)
            {
                request.FilePaths.ForEach(x => classForumResult.ClassForumResultFiles.Add(new ClassForumResultFile
                {
                    FilePath = x,
                }));
            }

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
                });
                await SendToAIGrading(classForum, classForumResult, request.WordContent!, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            return methodResult;
        }

        public async Task SendToAIGrading(ClassForum classForum, ClassForumResult classForumResult, string wordContent, CancellationToken cancellationToken)
        {
            if (classForum != null && classForumResult != null && classForum.IsAlFeedBack)
            {
                await _submitClassForumGradingPublisher.Publish(new ClassForumAIResponseModel
                {
                    ClassForumResultId = classForumResult.Id,
                    WordContent = wordContent,
                    IsRetry = true,
                }, cancellationToken);
            }
        }
    }
}
