// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.StudentReviewCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviews;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentReviewCommand : UpdateStudentReviewCommandModel, IRequest<MethodResult<StudentReviewModel>>
    {
    }

    public class UpdateStudentReviewCommandHandler : IRequestHandler<UpdateStudentReviewCommand, MethodResult<StudentReviewModel>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UpdateStudentReviewCommandHandler(IStudentReviewRepository studentReviewRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _studentReviewRepository = studentReviewRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentReviewModel>> Handle(UpdateStudentReviewCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentReviewModel> methodResult = new MethodResult<StudentReviewModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult.Content?.Result?.Id;
            var studentReview = await _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails).FirstOrDefaultAsync(x => x.StudentId == studentId && x.ReviewType == request.ReviewType && (!x.CourseId.HasValue || x.CourseId == request.CourseId), cancellationToken);
            if (studentReview == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentReviewErrorCode.StudentReviewNotExist));
                return methodResult;
            }
            _mapper.Map(request, studentReview);
            if (!studentReview.IsValid())
            {
                methodResult.AddErrorBadRequest(studentReview.ErrorMessages);
                return methodResult;
            }
            await _studentReviewRepository.ExecuteTransactionAsync(async () =>
            {
                _studentReviewRepository.Update(studentReview);
                await _studentReviewRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentReviewModel>(studentReview);
                return methodResult;
            });
            return methodResult;
        }
    }
}
