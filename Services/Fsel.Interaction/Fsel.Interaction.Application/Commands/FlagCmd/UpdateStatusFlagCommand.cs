// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusFlagCommand : UpdateStatusFlagsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateStatusFlagCommandHandler : IRequestHandler<UpdateStatusFlagCommand, MethodResult<bool>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseService _courseService;

        public UpdateStatusFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper, ICommentRepository commentRepository, ICourseService courseService)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
            _commentRepository = commentRepository;
            _courseService = courseService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var objectIds = request.ListFlag!.Select(x => x.ObjectId).ToList();
            var flags = await _flagRepository.Queryable.Where(x => x.ObjectId.HasValue && objectIds.Contains(x.ObjectId.Value)).ToListAsync(cancellationToken);

            if (request.ListFlag == null || request.ListFlag.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var courseResult = _courseService.GetClassForumResultQueryAsync
            foreach (var item in request.ListFlag)
            {
                var listFlag = flags.Where(x => x.ObjectId == item.ObjectId).ToList();
                if (listFlag == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(listFlag));
                    return methodResult;
                }
                listFlag = flags.Select(x => _mapper.Map(item, x)).ToList();

                var comment = await _commentRepository.Queryable.Where(x => x.ObjectId == item.ObjectId).FirstOrDefaultAsync(cancellationToken);
                if (comment == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(comment));
                    return methodResult;
                }
                if (listFlag.Select(x => x.Status == EnumFlagStatus.Approve).FirstOrDefault() && flags.Select(x => x.Type == EnumInteractionType.Comment).FirstOrDefault())
                {
                    await _commentRepository.DeleteAsync(comment);
                }
            }

            _flagRepository.UpdateList(flags);
            if (flags.Select(x => x.Status == EnumFlagStatus.Approve).FirstOrDefault() && flags.Select(x => x.Status == EnumFlagStatus.Reject).FirstOrDefault())
            {
                await _flagRepository.DeleteListAsync(flags);
            }

            await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
