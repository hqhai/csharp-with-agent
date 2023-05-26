// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassForumResultCommand : CreateClassForumResultCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class CreateClassForumResultCommandHandler : IRequestHandler<CreateClassForumResultCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumFileRepository _classForumFileRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public CreateClassForumResultCommandHandler(IMapper mapper, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository, IClassForumFileRepository classForumFileRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
            _classForumFileRepository = classForumFileRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumResultModel> methodResult = new MethodResult<ClassForumResultModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultsNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == lessonResult.Id, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumNull), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var classForumResult = await _classForumResultRepository.Queryable
                    .Include(x => x.ClassForumResultFiles)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);
            if (classForumResult == null)
            {
                classForumResult = new ClassForumResult
                {
                    Content = request.Content,
                    StudentId = studentId ?? default,
                    LessonResultId = request.LessonResultId,
                    Status = request.IsSubmit ? EnumClassForumResultStatus.Pending : EnumClassForumResultStatus.Draft,
                    ClassForumId = classForum.Id
                };
            }
            else if (classForumResult.Status != EnumClassForumResultStatus.Draft)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumHasNotSubmit));
                return methodResult;
            }

            if (request.FilePaths != null)
            {
                classForumResult.ClassForumResultFiles = request.FilePaths.Select(x => new ClassForumResultFile
                {
                    FilePath = x,
                }).ToList();
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                _classForumResultRepository.Add(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
