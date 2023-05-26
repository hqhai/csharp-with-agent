// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
            , ILessonRepository lessonRepository
            )
        {
            _mapper = mapper;
            _classForumRepository = classForumRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<ClassForumModel>> Handle(CreateClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumModel> methodResult = new MethodResult<ClassForumModel>();

            var isLesson = await _lessonRepository.AnyAsync(request.LessonId);
            if (!isLesson)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonsNotExist));
                return methodResult;
            }

            await _classForumRepository.ExecuteTransactionAsync(async () =>
            {
                var classForum = await _classForumRepository.Queryable.Include(x => x.ClassForumFiles).FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);
                if (classForum != null)
                {
                    _mapper.Map(request, classForum);
                    if (request.FilePaths != null)
                    {
                        classForum.ClassForumFiles = request.FilePaths.Select(x => new ClassForumFile
                        {
                            FilePath = x,
                        }).ToList();
                    }
                    classForum = _classForumRepository.Update(classForum);
                }

                if (classForum == null)
                {
                    classForum = _mapper.Map<ClassForum>(request);
                    classForum = _classForumRepository.Add(classForum);
                }

                if (!classForum.IsValid())
                {
                    methodResult.AddErrorBadRequest(classForum.ErrorMessages);
                    return methodResult;
                }
                if (request.FilePaths != null)
                {
                    classForum.ClassForumFiles = request.FilePaths.Select(x => new ClassForumFile
                    {
                        FilePath = x,
                    }).ToList();
                }

                await _classForumRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumModel>(classForum);
                return methodResult;
            });

            return methodResult;
        }
    }
}
