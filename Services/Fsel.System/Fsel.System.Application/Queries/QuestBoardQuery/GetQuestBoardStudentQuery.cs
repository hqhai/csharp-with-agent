// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardStudentQuery : IRequest<MethodResult<IList<QuestBoardModel>>>
    {
    }

    public class SearchQuestBoardStudentQueryHandler : IRequestHandler<GetQuestBoardStudentQuery, MethodResult<IList<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IMapper _mapper;

        public SearchQuestBoardStudentQueryHandler(IQuestBoardRepository questBoardRepository,
            AuthContext authContext,
            IUserService userService,
            IQuestBoardStudentRepository questBoardStudentRepository,
            IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardStudentRepository = questBoardStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<QuestBoardModel>>> Handle(GetQuestBoardStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestBoardModel>>();

            var methodStudent = await GetStudentModelAsync();
            if (!methodStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodStudent.Result!;

            var questBoards = await GetQuestBoardsAsync(cancellationToken);
            var questBoardStudents = await GetQuestBoardStudentsAsync(student.Id, cancellationToken);
            var weekDays = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
            var monDay = weekDays.First();
            var sunDay = weekDays.Last();

            questBoards.ForEach(p =>
            {
                QuestBoardStudent? questBoardStudent;
                if (p.RepeatType == EnumRepeatType.Daily)
                {
                    questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id && x.CreatedDate.Date == DateTime.UtcNow.Date);
                }
                else if (p.RepeatType.HasValue)
                {
                    questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id && x.CreatedDate.Date >= monDay.Date && x.CreatedDate.Date <= sunDay.Date);
                }
                else
                {
                    questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id);
                }

                p.Status = questBoardStudent?.Status;
                p.CurrentValue = questBoardStudent?.CurrentValue ?? default;
                p.IsFinish = questBoardStudent != null && (p.CurrentValue >= p.TargetValue);
            });

            methodResult.Result = questBoards.Where(x => x.Status != EnumQuestBoardStudentStatus.Received).OrderByDescending(x => x.IsFinish).ThenBy(x => x.Type).ThenBy(p => p.Name).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentModelAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            methodResult.Result = student;
            return methodResult;
        }

        private async Task<IList<QuestBoardModel>> GetQuestBoardsAsync(CancellationToken cancellationToken)
        {
            return await _questBoardRepository.Queryable.Where(p => p.IsActive)
                                                    .Select(x => _mapper.Map<QuestBoardModel>(x))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);
        }

        private async Task<IList<QuestBoardStudent>> GetQuestBoardStudentsAsync(Guid studentId, CancellationToken cancellationToken)
        {
            var monDay = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).First();
            var query = from baseQ in _questBoardRepository.Queryable
                        join qbs in _questBoardStudentRepository.Queryable on baseQ.Id equals qbs.QuestBoardId
                        where baseQ.IsActive && qbs.StudentId == studentId
                        select new
                        {
                            QuestBoardStudent = qbs,
                            QuestBoard = baseQ
                        };
            var queryBeginner = query.Where(x => x.QuestBoard.Type == EnumQuestBoardType.BeginnerQuests);
            var queryLearning = query.Where(x => x.QuestBoard.Type == EnumQuestBoardType.LearningQuests)
                                     .Where(x => x.QuestBoardStudent.CreatedDate.Date >= monDay.Date);
            query = queryLearning.Union(queryBeginner);
            return await query.Select(x => x.QuestBoardStudent).ToListAsync(cancellationToken);
        }
    }
}
