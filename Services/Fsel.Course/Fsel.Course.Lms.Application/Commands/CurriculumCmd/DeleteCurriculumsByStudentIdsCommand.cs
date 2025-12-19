// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCurriculumsByStudentIdsCommand : DeleteCurriculumsByStudentIdsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteCurriculumsByStudentIdsCommandHandler : IRequestHandler<DeleteCurriculumsByStudentIdsCommand, MethodResult<bool>>
    {
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;

        public DeleteCurriculumsByStudentIdsCommandHandler(ICurriculumStudentRepository curriculumStudentRepository)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCurriculumsByStudentIdsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                return methodResult;
            }

            var curriculumStudents = await _curriculumStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            if (curriculumStudents.Any())
            {
                await _curriculumStudentRepository.ExecuteTransactionAsync(async () =>
                {
                    await _curriculumStudentRepository.BulkDeleteList(curriculumStudents);
                    await _curriculumStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
