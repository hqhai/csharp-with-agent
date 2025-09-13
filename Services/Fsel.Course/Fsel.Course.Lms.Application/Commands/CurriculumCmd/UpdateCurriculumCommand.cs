// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Curriculums;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCurriculumCommand : UpdateCurriculumCommandModel, IRequest<MethodResult<CurriculumModel>>
    {
    }

    public class UpdateCurriculumCommandHandler : IRequestHandler<UpdateCurriculumCommand, MethodResult<CurriculumModel>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UpdateCurriculumCommandHandler(ICurriculumRepository curriculumRepository, IMapper mapper, IUserService userService, ICurriculumStudentRepository curriculumStudentRepository)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
            _userService = userService;
            _curriculumStudentRepository = curriculumStudentRepository;
        }

        public async Task<MethodResult<CurriculumModel>> Handle(UpdateCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CurriculumModel>();

            #region validation

            var curriculum = await _curriculumRepository.GetByIdAsync(request.Id);
            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.CurriculumDoesNotExist), nameof(curriculum), request.Id);
                return methodResult;
            }

            if (curriculum.CurriculumStatus == EnumCurriculumStatus.Progress)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.AlreadyActiveCurriculum), nameof(curriculum), request.Id);
                return methodResult;
            }

            if (string.IsNullOrWhiteSpace(request.CurriculumName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (currentDate < request.StartDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.StartDateCannotBeInThePast), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            if (request.EndDate < request.StartDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.InvalidDateRange), nameof(request.StartDate), request.StartDate);
                return methodResult;
            }

            var existingCurriculum = await _curriculumRepository.Queryable.FirstOrDefaultAsync(c => c.CurriculumName == request.CurriculumName && c.Id != curriculum.Id, cancellationToken);
            if (existingCurriculum != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCurriculumErrorCode.AlreadyExistCurriculumName), nameof(request.CurriculumName), request.CurriculumName);
                return methodResult;
            }

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, curriculum);

                if (!curriculum.IsValid())
                {
                    methodResult.AddError(curriculum.ErrorMessages);
                    return methodResult;
                }

                curriculum = _curriculumRepository.Update(curriculum);
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var curriculumStudents = await _curriculumStudentRepository.Queryable.Where(p => p.CurriculumId == curriculum.Id).ToListAsync(cancellationToken);
                if (curriculumStudents.Any())
                {
                    await _userService.UpdateExpiredDateForStudentsCampus(new UpdateExpiredDateForStudentsCampusCommandModels()
                    {
                        Students = curriculumStudents.Select(p => new UpdateExpiredDateForStudentsCampusCommandModel()
                        {
                            StudentId = p.StudentId,
                            ExpiredDate = curriculum.EndDate
                        }).ToList()
                    });
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CurriculumModel>(curriculum);
                return methodResult;
            });

            return methodResult;

            #endregion validation
        }
    }
}
