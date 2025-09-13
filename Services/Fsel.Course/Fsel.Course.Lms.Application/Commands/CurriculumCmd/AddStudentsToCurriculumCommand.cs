// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queries.CurriculumQuery;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddStudentsToCurriculumCommand : AddStudentsToCurriculumCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddStudentsToCurriculumCommandHandler : IRequestHandler<AddStudentsToCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly ITrainingService _trainingService;
        private readonly IMediator _mediator;

        public AddStudentsToCurriculumCommandHandler(ICurriculumStudentRepository curriculumStudentRepository, ITrainingService trainingService, MediatR.IMediator mediator)
        {
            _curriculumStudentRepository = curriculumStudentRepository;
            _trainingService = trainingService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(AddStudentsToCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = true;
                return methodResult;
            }

            var curriculumResult = await _mediator.Send(new GetCurriculumByIdQuery() { Id = request.CurriculumId }, cancellationToken);
            var curriculum = curriculumResult.Result;
            if (curriculum == null)
            {
                methodResult.AddError(curriculumResult.ErrorMessages);
                return methodResult;
            }

            var curriculumStudents = request.StudentIds.Select(p => new CurriculumStudent()
            {
                StudentId = p,
                CurriculumId = request.CurriculumId
            });

            await _curriculumStudentRepository.ExecuteTransactionAsync(async () =>
            {
                await _curriculumStudentRepository.BulkMergeAsync(curriculumStudents, x =>
                {
                    x.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.CurriculumId };
                });

                var result = await _trainingService.AddStudentsCampusIntoClass(new AddStudentsCampusIntoClassCommandModel()
                {
                    CourseId = curriculum.CourseCloneId,
                    StudentIds = curriculumStudents.Select(p => p.StudentId).ToList()
                });

                await _curriculumStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
