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

        public ApproveFlaggedClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(ApproveFlaggedClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Where(e => e.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            if (classForumResult.Status != EnumClassForumResultStatus.Graded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotGraded));
                return methodResult;
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
