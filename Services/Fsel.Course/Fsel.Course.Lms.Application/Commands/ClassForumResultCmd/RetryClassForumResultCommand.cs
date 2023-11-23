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
    using Fsel.Course.Lms.Application.Commands.AiCmd;
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

        public RetryClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, IClassForumRepository classForumRepository, IMediator mediator)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _classForumRepository = classForumRepository;
            _mediator = mediator;
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

            _mapper.Map(request, classForumResult);

            if (classForum.IsAlFeedBack)
            {
                var aIResponse = await _mediator.Send(new SubmitAICommand
                {
                    WordContent = request.RetryWordContent,
                    ClassForum = classForum,
                }, cancellationToken).ConfigureAwait(false);
                classForumResult.RetryGradingAlFeedBack = aIResponse;
            }

            if (request.RetryFilePaths != null)
            {
                classForumResult.ClassForumResultFiles = request.RetryFilePaths.Select(x => new ClassForumResultFile
                {
                    IsRetry = true,
                    FilePath = x,
                }).ToList();
            }

            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                classForumResult = _classForumResultRepository.Update(classForumResult);

                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
