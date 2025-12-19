// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCurriculumCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCurriculumCommandHandler : IRequestHandler<DeleteCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public DeleteCurriculumCommandHandler(ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, IUserService userService, IMediator mediator)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            #region validation

            var curriculum = await _curriculumRepository.Queryable.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var curriculumStudents = await _curriculumStudentRepository.Queryable.Where(p => p.CurriculumId == curriculum.Id).ToListAsync(cancellationToken);

            #endregion validation

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _curriculumRepository.DeleteAsync(curriculum);

                if (curriculumStudents.Any())
                {
                    var studentIds = curriculumStudents.Select(p => p.StudentId).ToList();

                    var deleteResult = await _mediator.Send(new DeleteStudentsFromCurriculumCommand()
                    {
                        StudentIds = studentIds,
                        CurriculumId = curriculum.Id,
                    }, cancellationToken);

                    if (!deleteResult.IsOK)
                    {
                        methodResult.AddError(deleteResult.ErrorMessages);
                        return methodResult;
                    }
                }

                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
