// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokensStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ReceiveTokensStudentCommandHandler : IRequestHandler<ReceiveTokensStudentCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public ReceiveTokensStudentCommandHandler(IStudentRepository studentRepository, AuthContext authContext, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<bool>> Handle(ReceiveTokensStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            IList<(int, bool)> receiveTokens;
            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId.ToString(), cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var date = DateTime.Now.Date;
            var studentDailyStreak = student.StudentDailyStreaks.FirstOrDefault(x => x.Id == request.Id && x.LevelOfGift != null && !x.IsGiftReceive);
            if (studentDailyStreak == null)
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            studentDailyStreak.IsGiftReceive = true;
            student.NumberOfToken += studentDailyStreak.LevelOfGift.GetNumberToken().Item1;
            studentDailyStreak.IsArmorialReceive = studentDailyStreak.LevelOfGift.GetNumberToken().Item2;
            await _studentDailyStreakRepository.ExecuteTransactionAsync(async () =>
             {
                 _studentRepository.Update(student);
                 await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                 _studentDailyStreakRepository.Update(studentDailyStreak);
                 await _studentDailyStreakRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                 methodResult.StatusCode = StatusCodes.Status201Created;
                 methodResult.Result = true;
                 return methodResult;
             });
            return methodResult;
        }
    }
}
