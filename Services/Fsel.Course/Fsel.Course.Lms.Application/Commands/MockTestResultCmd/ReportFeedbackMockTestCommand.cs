// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReportFeedbackMockTestCommand : ReportFeedbackMockTestCommandModel, IRequest<MethodResult<StudentFeedbackModel>>
    {
    }

    public class ReportFeedbackMockTestCommandHandler : IRequestHandler<ReportFeedbackMockTestCommand, MethodResult<StudentFeedbackModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public ReportFeedbackMockTestCommandHandler(IMapper mapper, IStudentFeedbackRepository studentFeedbackRepository)
        {
            _mapper = mapper;
            _studentFeedbackRepository = studentFeedbackRepository;
        }

        public async Task<MethodResult<StudentFeedbackModel>> Handle(ReportFeedbackMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFeedbackModel> methodResult = new MethodResult<StudentFeedbackModel>();

            StudentFeedback studentFeedback = _mapper.Map<StudentFeedback>(request);
            if (!EnumFeedBackHelper.IsCheckFeedBack(request.FeedBackNegatives, request.FeedBackPositives))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedbackPositiveOrFeedBackBothHaveValue));
                return methodResult;
            }
            if (studentFeedback.FeedBackStars > 5)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedBackStarOnlyCanHane5));
                return methodResult;
            }
            var isExistFeedback = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == request.ObjectId, cancellationToken);
            if (isExistFeedback)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isExistFeedback));
                return methodResult;
            }

            await _studentFeedbackRepository.ExecuteTransactionAsync(async () =>
            {
                studentFeedback.Feature = EnumFeature.FullTest;
                studentFeedback.Type = EnumStudentFeedBackType.Teacher;
                studentFeedback = _studentFeedbackRepository.Add(studentFeedback);
                await _studentFeedbackRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentFeedbackModel>(studentFeedback);
                return methodResult;
            });

            return methodResult;
        }
    }
}
