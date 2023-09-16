// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RateClassForumResultCommand : RateClassForumResultCommandModel, IRequest<MethodResult<StudentFeedbackModel>>
    {
    }

    public class RateClassForumResultCommandHandler : IRequestHandler<RateClassForumResultCommand, MethodResult<StudentFeedbackModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public RateClassForumResultCommandHandler(IMapper mapper, IStudentFeedbackRepository studentFeedbackRepository)
        {
            _mapper = mapper;
            _studentFeedbackRepository = studentFeedbackRepository;
        }

        public async Task<MethodResult<StudentFeedbackModel>> Handle(RateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFeedbackModel> methodResult = new MethodResult<StudentFeedbackModel>();

            StudentFeedback studentFeed = _mapper.Map<StudentFeedback>(request);
            if (!EnumFeedBackHelper.IsCheckFeedBack(request.FeedBackNegatives, request.FeedBackPositives))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedbackPositiveOrFeedBackBothHaveValue));
                return methodResult;
            }
            if (studentFeed.FeedBackStars > 5)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedBackStarOnlyCanHane5));
                return methodResult;
            }
            var isExistFeedback = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == request.ObjectId && x.Type == request.Type, cancellationToken);
            if (isExistFeedback)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isExistFeedback));
                return methodResult;
            }

            #region temporary delete

            /* if (classForumResult.Status != EnumClassForumResultStatus.Graded)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotGraded));
                 return methodResult;
             }*/

            #endregion temporary delete

            await _studentFeedbackRepository.ExecuteTransactionAsync(async () =>
            {
                studentFeed.Feature = EnumFeature.ClassForum;
                studentFeed = _studentFeedbackRepository.Add(studentFeed);
                await _studentFeedbackRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentFeedbackModel>(studentFeed);
                return methodResult;
            });

            return methodResult;
        }
    }
}
