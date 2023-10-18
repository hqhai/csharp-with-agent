// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultFlagCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResultFlags;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RateClassForumResultFlagCommand : RateClassForumResultFlagCommandModel, IRequest<MethodResult<ClassForumResultFlagModel>>
    {
    }

    public class RateClassForumResultFlagCommandHandler : IRequestHandler<RateClassForumResultFlagCommand, MethodResult<ClassForumResultFlagModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassForumResultFlagRepository _classForumResultFlagRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public RateClassForumResultFlagCommandHandler(IMapper mapper, IClassForumResultFlagRepository classForumResultFlagRepository, IClassForumResultRepository classForumResultRepository)
        {
            _mapper = mapper;
            _classForumResultFlagRepository = classForumResultFlagRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<ClassForumResultFlagModel>> Handle(RateClassForumResultFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumResultFlagModel> methodResult = new MethodResult<ClassForumResultFlagModel>();
            ClassForumResultFlag classForumResultFlag = _mapper.Map<ClassForumResultFlag>(request);

            var classForumResult = await _classForumResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == classForumResultFlag.ClassForumResultId,cancellationToken);
       
            if (classForumResult?.Status != EnumClassForumResultStatus.Graded && classForumResult?.Status != EnumClassForumResultStatus.PendingForGrading)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendingForGradingOrGraded));
                return methodResult;
            }

            var isExistFeedback = await _classForumResultFlagRepository.Queryable.AnyAsync(x => x.ClassForumResultId == request.ClassForumResultId, cancellationToken);
            if (isExistFeedback)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isExistFeedback));
                return methodResult;
            }
            await _classForumResultFlagRepository.ExecuteTransactionAsync(async () =>
            {
                classForumResultFlag.Status = EnumClassForumResultFlagStatus.New;
                classForumResultFlag = _classForumResultFlagRepository.Add(classForumResultFlag);
                await _classForumResultFlagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultFlagModel>(classForumResultFlag);
                return methodResult;
            });

            return methodResult;
        }
    }
}
