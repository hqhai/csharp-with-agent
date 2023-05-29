// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CsoApproveCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Commands.FinalTestCmd;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveClassForumPenddingCommand : ApproveClassForumPenddingCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class ApproveClassForumPenddingCommandHandler : IRequestHandler<ApproveClassForumPenddingCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;

        public ApproveClassForumPenddingCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(ApproveClassForumPenddingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumResultModel> methodResult = new MethodResult<ClassForumResultModel>();

            /*if (request.IsApprove)
            {
                EnumClassForumResultStatus.PendingForGrading == true;
            }*/

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Where(e => e.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumNotExist), nameof(request.ClassForumResultId), request.ClassForumResultId);
                return methodResult;
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                /* classForumResult.id = EnumCourseStatus.Active;*/
                classForumResult = _classForumResultRepository.Update(classForumResult);
                _classForumResultRepository.Update(classForumResult);

                /*foreach (var item in courses)
                {
                    item.Status = EnumCourseStatus.InActive;
                    _classForumResultRepository.Update(item);
                }*/

                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
