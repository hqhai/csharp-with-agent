// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.LessonCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Common.LessonHelpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateLessonCommand : UpdateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly LessonConverter _lessonConverter;

        public CreateLessonCommandHandler(ILessonRepository lessonRepository,
                                          IMapper mapper,
                                          LessonConverter lessonConverter)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _lessonConverter = lessonConverter;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            var validate = await _lessonConverter.ValidateLesson(request, true, null, cancellationToken);
            if (!validate.IsOK)
            {
                methodResult.AddErrorBadRequest(validate.ErrorMessages);
                return methodResult;
            }

            var lessonConverter = await _lessonConverter.LessonModuleHandler(request.LessonModules!, false, false, cancellationToken);
            if (!lessonConverter.IsOK)
            {
                methodResult.AddErrorBadRequest(lessonConverter.ErrorMessages);
                return methodResult;
            }

            var lesson = LessonFactory.Create(request).Build(version: 0, originalId: Guid.NewGuid());
            if (!lesson.IsValid())
            {
                methodResult.AddErrorBadRequest(lesson.ErrorMessages);
                return methodResult;
            }

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                _lessonRepository.Add(lesson);
                await _lessonRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
