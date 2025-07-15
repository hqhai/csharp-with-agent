// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateOcCheckInClassForumResultCommand : IRequest<VoidMethodResult>
    {
    }

    public class UpdateOcCheckInClassForumResultCommandHandler : IRequestHandler<UpdateOcCheckInClassForumResultCommand, VoidMethodResult>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public UpdateOcCheckInClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<VoidMethodResult> Handle(UpdateOcCheckInClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            var dateNow = DateTime.UtcNow;

            var classForumResult = await _classForumResultRepository.Queryable
                        .Where(x => x.CheckCsoId != null && x.CheckStartDate!.Value.AddMinutes(30) < dateNow).ToListAsync(cancellationToken);

            foreach (var item in classForumResult)
            {
                item.CheckCsoId = null;
                item.CheckStartDate = null;
            }
            await _classForumResultRepository.BulkUpdateList(classForumResult, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
            });
            return methodResult;
        }
    }
}
