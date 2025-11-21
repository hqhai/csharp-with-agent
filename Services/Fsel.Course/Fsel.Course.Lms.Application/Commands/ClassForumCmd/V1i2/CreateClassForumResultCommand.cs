// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i2
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;

    public class CreateClassForumResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class CreateClassForumResultCommandHandler : IRequestHandler<CreateClassForumResultCommand, MethodResult<bool>>
    {
        public CreateClassForumResultCommandHandler(ILessonResultRepository lessonResultRepository)
        {
        }

        public async Task<MethodResult<bool>> Handle(CreateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            return methodResult;
        }
    }
}
