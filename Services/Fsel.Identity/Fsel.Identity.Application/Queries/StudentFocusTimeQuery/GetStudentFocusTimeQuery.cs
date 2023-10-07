// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentFocusTimeQuery

{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentFocusTimeQuery : IRequest<MethodResult<StudentFocusTimeModel>>
    {
    }

    public class GetStudentFocusTimeQueryHandler : IRequestHandler<GetStudentFocusTimeQuery, MethodResult<StudentFocusTimeModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;
        public GetStudentFocusTimeQueryHandler(IMapper mapper, IStudentFocusTimeRepository studentFocusTimeRepository, AuthContext authContext, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _authContext = authContext;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<StudentFocusTimeModel>> Handle(GetStudentFocusTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFocusTimeModel> methodResult = new MethodResult<StudentFocusTimeModel>();
            var student = await _studentRepository.Queryable.Include(x => x.Human).FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId.ToString(), cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }

            StudentFocusTimeModel studentFocusTime = new StudentFocusTimeModel();
            var studentFocusTimesQuery = _studentFocusTimeRepository.Queryable.Where(x => x.StudentId == student.Id && x.CreatedDate.Date == DateTime.UtcNow.Date);

            studentFocusTime.StudentId = student.Id;
            studentFocusTime = _mapper.Map<StudentFocusTimeModel>(studentFocusTimesQuery.FirstOrDefault());

            methodResult.Result = _mapper.Map<StudentFocusTimeModel>(studentFocusTime);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
