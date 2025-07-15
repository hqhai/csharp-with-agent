using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.StudentEditHistory;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Identity.Application.Commands.StudentEditHistoryCmd
{
    public class CreateStudentEditHistoryCommand : CreateStudentEditHistoryCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentEditHistoryCommandHandler : IRequestHandler<CreateStudentEditHistoryCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentEditHistoryRepository _studentEditHistoryRepository;

        public CreateStudentEditHistoryCommandHandler(IMapper mapper, IStudentEditHistoryRepository studentEditHistoryRepository)
        {
            _mapper = mapper;
            _studentEditHistoryRepository = studentEditHistoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentEditHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentEditHistory = _mapper.Map<StudentEditHistory>(request);
            if (!studentEditHistory.IsValid())
            {
                methodResult.AddError(studentEditHistory.ErrorMessages);
                return methodResult;
            }
            await _studentEditHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                studentEditHistory = _studentEditHistoryRepository.Add(studentEditHistory);
                await _studentEditHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
