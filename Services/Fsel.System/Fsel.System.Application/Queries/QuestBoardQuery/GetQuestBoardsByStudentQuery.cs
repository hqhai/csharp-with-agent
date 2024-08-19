// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
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

    public class GetQuestBoardsByStudentQuery : IRequest<MethodResult<IList<DashboardQuestBoardModel>>>
    {
    }

    public class GetQuestBoardsByStudentQueryHandler : IRequestHandler<GetQuestBoardsByStudentQuery, MethodResult<IList<DashboardQuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly AuthContext _authContext;
        private readonly IQuestBoardOverallRepository _questBoardOverallRepository;
        private readonly IUserService _userService;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IQuestBoardOverallStudentRepository _questBoardOverallStudentRepository;
        private readonly IMapper _mapper;

        public GetQuestBoardsByStudentQueryHandler(IQuestBoardRepository questBoardRepository, AuthContext authContext, IQuestBoardOverallRepository questBoardOverallRepository, IUserService userService, IQuestBoardStudentRepository questBoardStudentRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _authContext = authContext;
            _questBoardOverallRepository = questBoardOverallRepository;
            _userService = userService;
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<DashboardQuestBoardModel>>> Handle(GetQuestBoardsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<DashboardQuestBoardModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var model = new List<DashboardQuestBoardModel>();

            var beginnerQuests = new DashboardQuestBoardModel();
            beginnerQuests.Type = EnumQuestBoardType.BeginnerQuests;
            var learningQuests = new DashboardQuestBoardModel();
            learningQuests.Type = EnumQuestBoardType.LearningQuests;

            var questBoards = await _questBoardRepository.Queryable.Include(p => p.QuestBoardStudents).Where(p => p.IsActive).ProjectTo<QuestBoardModel>(_mapper.ConfigurationProvider).OrderBy(p => p.Name).ToListAsync(cancellationToken);

            questBoards = questBoards.Where(p => p.IsActive).ToList();

            beginnerQuests.QuestBoardModels = questBoards.Where(p => p.Type == EnumQuestBoardType.BeginnerQuests).ToList();
            learningQuests.QuestBoardModels = questBoards.Where(p => p.Type == EnumQuestBoardType.LearningQuests).ToList();

            var weekDays = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
            var monDay = weekDays.First();
            var sunDay = weekDays.Last();

            var questBoardStudents = await _questBoardStudentRepository.Queryable.Where(p => questBoards.Select(x => x.Id).Contains(p.QuestBoardId) && p.StudentId == student!.Id).ToListAsync(cancellationToken);

            beginnerQuests.QuestBoardModels.ForEach(p =>
            {
                var questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id);
                p.Status = questBoardStudent == null ? EnumQuestBoardStudentStatus.NotReceived : questBoardStudent.Status;
                p.CurrentValue = questBoardStudent == null ? 0 : questBoardStudent.CurrentValue;
                p.IsFinish = questBoardStudent != null && (p.CurrentValue >= p.TargetValue);
            });
            beginnerQuests.QuestBoardModels = beginnerQuests.QuestBoardModels.OrderBy(x => x.IsFinish).ThenBy(x => x.RepeatType).ToList();

            learningQuests.QuestBoardModels.ForEach(p =>
            {
                if (p.RepeatType == EnumRepeatType.Daily)
                {
                    var questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id && x.CreatedDate.Date == DateTime.UtcNow.Date);
                    p.Status = questBoardStudent == null ? EnumQuestBoardStudentStatus.NotReceived : questBoardStudent.Status;
                    p.CurrentValue = questBoardStudent == null ? 0 : questBoardStudent.CurrentValue;
                    p.IsFinish = questBoardStudent != null && (p.CurrentValue >= p.TargetValue);
                }
                else
                {
                    var questBoardStudent = questBoardStudents.FirstOrDefault(x => x.QuestBoardId == p.Id && x.CreatedDate.Date >= monDay.Date && x.CreatedDate.Date <= sunDay.Date);
                    p.Status = questBoardStudent == null ? EnumQuestBoardStudentStatus.NotReceived : questBoardStudent.Status;
                    p.CurrentValue = questBoardStudent == null ? 0 : questBoardStudent.CurrentValue;
                    p.IsFinish = questBoardStudent != null && (p.CurrentValue >= p.TargetValue);
                }
            });
            learningQuests.QuestBoardModels = learningQuests.QuestBoardModels.OrderBy(x => x.IsFinish).ThenBy(x => x.RepeatType).OrderBy(x => x.Status).ToList();

            var beginnerQuestBoardIds = questBoards.Where(p => p.Type == EnumQuestBoardType.BeginnerQuests).Select(x => x.Id).ToList();
            var learningQuestBoardIds = questBoards.Where(p => p.Type == EnumQuestBoardType.LearningQuests).Select(x => x.Id).ToList();

            var questBoardOveralls = await _questBoardOverallRepository.Queryable.ToListAsync(cancellationToken);

            await GetBeginnerQuestBoardOverall(questBoardOveralls, model, beginnerQuests, student!, beginnerQuestBoardIds, methodResult, cancellationToken);
            await GetLearningQuestBoardOverall(questBoardOveralls, model, learningQuests, student!, methodResult, cancellationToken);

            methodResult.Result = model;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<IList<DashboardQuestBoardModel>>> GetBeginnerQuestBoardOverall(IList<QuestBoardOverall> questBoardOveralls, IList<DashboardQuestBoardModel> models, DashboardQuestBoardModel beginnerQuests, StudentModel student, List<Guid> beginnerQuestBoardIds, MethodResult<IList<DashboardQuestBoardModel>> methodResult, CancellationToken cancellationToken)
        {
            questBoardOveralls = questBoardOveralls.Where(p => p.Type == EnumQuestBoardType.BeginnerQuests).OrderBy(x => x.TargetValue).ToList();
            if (questBoardOveralls == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var questBoardOverallStudents = await _questBoardOverallStudentRepository.Queryable.Where(p => p.StudentId == student.Id && questBoardOveralls.Select(x => x.Id).Contains(p.QuestBoardOverallId)).ToListAsync(cancellationToken);

            beginnerQuests.CurrentValue = questBoardOverallStudents.Count == 0 ? 0 : questBoardOverallStudents.Max(p => p.CurrentValue);

            beginnerQuests.QuestBoardOveralls = questBoardOveralls.Select(p =>
            {
                var questBoardOverallStudent = questBoardOverallStudents.FirstOrDefault(x => x.QuestBoardOverallId == p.Id);
                return new QuestBoardOverallModel
                {
                    Id = p.Id,
                    Type = p.Type,
                    TargetValue = p.TargetValue,
                    Token = p.Token,
                    Status = questBoardOverallStudent == null ? EnumQuestBoardOverallStudentStatus.NotReceived : questBoardOverallStudent.Status,
                };
            }).OrderBy(x => x.Status).ToList();
            models.Add(beginnerQuests);
            return methodResult;
        }

        private async Task<MethodResult<IList<DashboardQuestBoardModel>>> GetLearningQuestBoardOverall(IList<QuestBoardOverall> questBoardOveralls, IList<DashboardQuestBoardModel> models, DashboardQuestBoardModel learningQuests, StudentModel student, MethodResult<IList<DashboardQuestBoardModel>> methodResult, CancellationToken cancellationToken)
        {
            var questBoardOverall = questBoardOveralls.FirstOrDefault(p => p.Type == EnumQuestBoardType.LearningQuests);
            if (questBoardOverall == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == student!.Id && p.QuestBoardOverallId == questBoardOverall.Id && p.CurrentValue >= questBoardOverall.TargetValue && p.Status == EnumQuestBoardOverallStudentStatus.NotReceived, cancellationToken);

            if (questBoardOverallStudent != null)
            {
                var questBoardOverallModel = new QuestBoardOverallModel
                {
                    Id = questBoardOverall.Id,
                    TargetValue = questBoardOverall.TargetValue,
                    Token = questBoardOverall.Token,
                    Status = EnumQuestBoardOverallStudentStatus.NotReceived
                };
                learningQuests.QuestBoardOveralls?.Add(questBoardOverallModel);
            }
            else
            {
                var weekDays = Shared.Helpers.DateTimeHelper.GetWeekDays(DateTime.UtcNow).OrderBy(p => p).ToList();
                var monDay = weekDays.First();
                var sunDay = weekDays.Last();

                questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == student!.Id && p.QuestBoardOverallId == questBoardOverall.Id && p.CreatedDate.Date >= monDay.Date && p.CreatedDate.Date <= sunDay.Date, cancellationToken);

                var questBoardOverallModel = new QuestBoardOverallModel
                {
                    Id = questBoardOverall.Id,
                    TargetValue = questBoardOverall.TargetValue,
                    Token = questBoardOverall.Token,
                    Status = questBoardOverallStudent == null ? EnumQuestBoardOverallStudentStatus.NotReceived : questBoardOverallStudent.Status,
                };
                learningQuests.QuestBoardOveralls?.Add(questBoardOverallModel);
            }

            learningQuests.CurrentValue = questBoardOverallStudent?.CurrentValue ?? 0;
            models.Add(learningQuests);
            return methodResult;
        }
    }
}
