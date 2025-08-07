// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentFocusTimeQuery

{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;

        public GetStudentFocusTimeQueryHandler(IMapper mapper, IStudentFocusTimeRepository studentFocusTimeRepository, IMediator mediator, AuthContext authContext, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _mediator = mediator;
            _authContext = authContext;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<StudentFocusTimeModel>> Handle(GetStudentFocusTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentFocusTimeModel> methodResult = new MethodResult<StudentFocusTimeModel>();
            var student = await _studentRepository.Queryable.Include(x => x.Human).FirstOrDefaultAsync(x => x.Human!.UserId == _authContext.CurrentUserId, cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(_authContext.CurrentUserId), _authContext.CurrentUserId);
                return methodResult;
            }

            var studentFocusTimes = await _studentFocusTimeRepository.Queryable.Where(x => x.StudentId == student.Id && x.CreatedDate >= DateTime.UtcNow.AddDays(-1).Date).ToListAsync(cancellationToken);
            if (studentFocusTimes == null)
            {
                methodResult.Result = new StudentFocusTimeModel();
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            //Check xem học sinh có học liên tiếp trong 7 ngày hay không
            var hasContinuousData = await _mediator.Send(new CheckSuperFireModeQuery(), cancellationToken);

            var studentFocusTime = _mapper.Map<StudentFocusTimeModel>(studentFocusTimes.Where(x => x.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == DateTime.Now.Date).FirstOrDefault());
            if (studentFocusTime != null)
            {
                studentFocusTime.StudentId = student.Id;
                studentFocusTime.IsWeekStreak = hasContinuousData.Result;
            }
            else
            {
                studentFocusTime = new StudentFocusTimeModel();
                studentFocusTime.IsFirstTimeInDay = true;
            }

            studentFocusTime.NearestTargetTime = GetNearestConfigTime(student.Id);
            methodResult.Result = studentFocusTime;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private double GetNearestConfigTime(Guid? studentId)
        {
            var nearestConfigTargetTime = _studentFocusTimeRepository.Queryable.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.StudentId == studentId && x.CreatedDate.Date != DateTime.UtcNow.Date && x.TargetTime != 0)?.TargetTime ?? 0;

            return nearestConfigTargetTime;
        }
    }
}
