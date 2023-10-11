// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveFlaggedClassForumResultCommand : ApproveFlaggedClassForumResultCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class ApproveFlaggedClassForumResultCommandHandler : IRequestHandler<ApproveFlaggedClassForumResultCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly IClassForumResultFlagRepository _classForumResultFlagRepository;

        public ApproveFlaggedClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, IClassForumResultFlagRepository classForumResultFlagRepository)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _classForumResultFlagRepository = classForumResultFlagRepository;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(ApproveFlaggedClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Where(e => e.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var classForumResultFlag = await _classForumResultFlagRepository.Queryable.Where(e => e.ClassForumResultId == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (classForumResultFlag == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResultFlag));
                return methodResult;
            }
            if (classForumResult.Status != EnumClassForumResultStatus.Graded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotGraded));
                return methodResult;
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.IsApprove)
                {
                    await _classForumResultRepository.DeleteAsync(classForumResult);

                    classForumResultFlag.Status = EnumClassForumResultFlagStatus.Approve;
                }
                else
                {
                    classForumResultFlag.Status = EnumClassForumResultFlagStatus.Reject;
                }
                _classForumResultFlagRepository.Update(classForumResultFlag);
                await _classForumResultFlagRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
