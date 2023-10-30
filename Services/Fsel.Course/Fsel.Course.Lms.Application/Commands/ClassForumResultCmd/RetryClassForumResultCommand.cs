// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
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

        public RetryClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(RetryClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Where(e => e.Id == request.Id)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            _mapper.Map(request, classForumResult);

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
