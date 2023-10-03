// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.DailyStreakCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokensStudentCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
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
            var methodResult = new MethodResult<bool>();
            if (request.Ids == null || !request.Ids.Any())
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var student = await _studentRepository.Queryable.Include(x => x.StudentDailyStreaks).Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == _authContext.CurrentUserId.ToString(), cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var date = DateTime.Now.Date;
            var studentDailyStreaks = student.StudentDailyStreaks.Where(x => request.Ids.Contains(x.Id) && x.LevelOfGift != null && !x.IsReceiveGift).ToList();
            studentDailyStreaks.ForEach(x => x.IsReceiveGift = true);
            student.NumberOfToken += NumberTokenHelper.GetNumbersToken(studentDailyStreaks.Select(x => x.LevelOfGift ?? default).ToList());
            await _studentDailyStreakRepository.ExecuteTransactionAsync(async () =>
            {
                _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                _studentDailyStreakRepository.UpdateList(studentDailyStreaks);
                await _studentDailyStreakRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
