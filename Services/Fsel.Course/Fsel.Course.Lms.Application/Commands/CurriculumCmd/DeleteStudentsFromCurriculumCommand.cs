// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentsFromCurriculumCommand : IRequest<MethodResult<bool>>
    {
        public Guid CurriculumId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }

    public class DeleteStudentsFromCurriculumCommandHandler : IRequestHandler<DeleteStudentsFromCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IUserService _userService;

        public DeleteStudentsFromCurriculumCommandHandler(ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, IUserService userService)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentsFromCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = true;
                return methodResult;
            }

            var query = await (from cs in _curriculumStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId)
                               join c in _curriculumRepository.Queryable on cs.CurriculumId equals c.Id
                               select new
                               {
                                   CurriculumStudent = cs,
                                   Curriculum = c
                               }).ToListAsync(cancellationToken);

            var currentCurriculums = query.Where(p => p.Curriculum.Id == request.CurriculumId);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var studentModels = new List<UpdateExpiredDateForStudentsCampusCommandModel>();

            currentCurriculums.ForEach(p =>
            {
                var curriculumStudent = query.Where(x => x.CurriculumStudent.Id != p.CurriculumStudent.Id && x.CurriculumStudent.StudentId == p.CurriculumStudent.StudentId).OrderByDescending(x => x.Curriculum.EndDate).FirstOrDefault();
                studentModels.Add(new UpdateExpiredDateForStudentsCampusCommandModel()
                {
                    StudentId = p.CurriculumStudent.StudentId,
                    ExpiredDate = curriculumStudent != null && curriculumStudent.Curriculum.EndDate > currentDate ? currentDate : null,
                });
            });

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                await _curriculumStudentRepository.DeleteListAsync(currentCurriculums.Select(p => p.CurriculumStudent).ToList());
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var updateExpiredDateResult = await _userService.UpdateExpiredDateForStudentsCampus(new UpdateExpiredDateForStudentsCampusCommandModels() { Students = studentModels });
                if (!updateExpiredDateResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateExpiredDateResult.Error);
                    return methodResult;
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
