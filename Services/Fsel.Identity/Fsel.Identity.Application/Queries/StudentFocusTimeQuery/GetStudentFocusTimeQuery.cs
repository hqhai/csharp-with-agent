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
        private const int NUMBER_OF_WEEKDAY = 7;
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

            if (studentFocusTimesQuery == null)
            {
                studentFocusTime = new StudentFocusTimeModel();
                methodResult.Result = studentFocusTime;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            //Check xem học sinh có học liên tiếp trong 7 ngày hay không 
            var currentDate = DateTime.UtcNow.Date;
            var startDate = currentDate.AddDays(-NUMBER_OF_WEEKDAY).Date; // Ngày bắt đầu từ 7 ngày trước
            var endDate = currentDate.Date; // Ngày kết thúc là hôm nay
            var studentFocusTimesCheckQuery = _studentFocusTimeRepository.Queryable
                                        .Where(x => x.StudentId == student.Id && x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate && x.ExecuteTime >= x.TargetTime)
                                        .OrderBy(x => x.CreatedDate.Date)
                                        .ToList();
            bool hasContinuousData = true;
            for (int i = 0; i < NUMBER_OF_WEEKDAY; i++)
            {
                var expectedDate = currentDate.AddDays(-i).Date;
                studentFocusTimesQuery.Where(x => x.CreatedDate.Date == expectedDate);
                if (!studentFocusTimesQuery.Any(x => x.CreatedDate.Date == expectedDate))
                {
                    hasContinuousData = false;
                    break;
                }
            }

            studentFocusTime.StudentId = student.Id;
            studentFocusTime.IsWeekStreak = hasContinuousData;
            studentFocusTime = _mapper.Map<StudentFocusTimeModel>(studentFocusTimesQuery.FirstOrDefault());

            methodResult.Result = _mapper.Map<StudentFocusTimeModel>(studentFocusTime);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
