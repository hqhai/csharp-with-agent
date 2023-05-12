// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Commands.CourseCmd;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateClassForumCommand : CreateClassForumCommandModel, IRequest<MethodResult<ClassForumModel>>
    {
    }

    public class CreateClassForumCommandHandler : IRequestHandler<CreateClassForumCommand, MethodResult<ClassForumModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonRepository _lessonRepository;

        public CreateClassForumCommandHandler(IMapper mapper
            , IClassForumRepository classForumRepository
            , ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _classForumRepository = classForumRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<ClassForumModel>> Handle(CreateClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumModel> methodResult = new MethodResult<ClassForumModel>();
            ClassForum classForum = _mapper.Map<ClassForum>(request);

            if (!classForum.IsValid())
            {
                methodResult.AddErrorBadRequest(classForum.ErrorMessages);
                return methodResult;
            }

            var isLesson = await _lessonRepository.AnyAsync(request.LessonId ?? default);
            if (!isLesson)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonsNotExist));
                return methodResult;
            }
            await _classForumRepository.ExecuteTransactionAsync(async () =>
            {
                classForum = _classForumRepository.Add(classForum);
                await _classForumRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumModel>(classForum);
                return methodResult;
            });

            return methodResult;
        }
    }
}
