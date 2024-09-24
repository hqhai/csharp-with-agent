// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListDataUserCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }
    public class DeleteListDataUserCommandHandler : IRequestHandler<DeleteListDataUserCommand, MethodResult<bool>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly IUserService _userService;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;

        public DeleteListDataUserCommandHandler(IStudentReviewRepository studentReviewRepository
                                              , IUserService userService
                                              , IInteractionActionRepository interactionActionRepository
                                              , ICommentRepository commentRepository
                                              , IPostRepository postRepository
                                              , ICustomerSurveyRepository customerSurveyRepository)
        {
            _studentReviewRepository = studentReviewRepository;
            _userService = userService;
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _customerSurveyRepository = customerSurveyRepository;
        }
        public async Task<MethodResult<bool>> Handle(DeleteListDataUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);

            if (studentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var studentId = studentResult.Content?.Result?.Id;

            //delete studentReview
            var studentReview = await _studentReviewRepository.Queryable
                                                              .Include(x => x.StudentReviewDetails)
                                                              .Where(x => x.StudentId == studentId)
                                                              .ToListAsync(cancellationToken);
            if (studentReview.Count != 0)
            {
                await _studentReviewRepository.DeleteListAsync(studentReview);
                await _studentReviewRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken);
            }

            //delete interactionAction
            var interactionAction = await _interactionActionRepository.Queryable
                                                          .Where(x => x.UserId == request.UserId)
                                                          .ToListAsync(cancellationToken);
            if (interactionAction.Count != 0)
            {
                await _interactionActionRepository.DeleteListAsync(interactionAction);
                await _interactionActionRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken);
            }

            //delete comment
            var comment = await _commentRepository.Queryable
                                                  .Where(x => x.UserId == request.UserId)
                                                  .ToListAsync(cancellationToken);
            if (comment.Count != 0)
            {
                await _commentRepository.DeleteListAsync(comment);
                await _commentRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken);
            }

            //delete post
            var post = await _postRepository.Queryable
                                            .Include(x => x.PostTags)
                                            .Where(x => x.UserId == request.UserId)
                                            .ToListAsync(cancellationToken);
            if (post.Count != 0)
            {
                await _postRepository.DeleteListAsync(post);
                await _postRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken);
            }

            //delete customerSurvey
            var customerSurvey = await _customerSurveyRepository.Queryable
                                                                .Where(x => x.UserId == request.UserId)
                                                                .ToListAsync(cancellationToken);
            if (customerSurvey.Count != 0)
            {
                await _customerSurveyRepository.DeleteListAsync(customerSurvey);
                await _customerSurveyRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
