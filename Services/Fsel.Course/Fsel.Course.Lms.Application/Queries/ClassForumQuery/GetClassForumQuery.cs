// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.NotificationServices;
    using Fsel.Course.Lms.Application.Services.NotificationServices.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumQuery : IRequest<MethodResult<ClassForumByStudentModel>>
    {
        public Guid LessonId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetClassForumQueryHandler : IRequestHandler<GetClassForumQuery, MethodResult<ClassForumByStudentModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IInteractionService _interactionService;
        private readonly ITrainingService _trainingService;
        private readonly INotificationService _notificationService;
        private readonly IClassForumResultRandomRepository _classForumResultRandomRepository;
        private const int STUDENT_RANDOM_TAKE = 2; // lấy random 2 bài post của học sinh bất kì từ lớp khác, cùng unit, cùng level

        public GetClassForumQueryHandler(IClassForumRepository classForumRepository
            , IClassForumResultRepository classForumResultRepository
            , IUserService userService
            , AuthContext authContext
            , IMapper mapper
            , ILessonRepository lessonRepository
            , IInteractionService interactionService,
              ITrainingService trainingService,
              INotificationService notificationService,
              IClassForumResultRandomRepository classForumResultRandomRepository)
        {
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _interactionService = interactionService;
            _trainingService = trainingService;
            _notificationService = notificationService;
            _classForumResultRandomRepository = classForumResultRandomRepository;
        }

        public async Task<MethodResult<ClassForumByStudentModel>> Handle(GetClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumByStudentModel> methodResult = new MethodResult<ClassForumByStudentModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            if (studentResult == null || student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var isLesson = await _lessonRepository.AnyAsync(request.LessonId);
            if (!isLesson)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isLesson));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable
                .Include(x => x.ClassForumFiles)
                .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            var classForumByStudentModel = _mapper.Map<ClassForumByStudentModel>(classForum);

            // Lấy list StudentId đang học trong class hiện tại
            IList<Guid>? classStudentIds = new List<Guid>();
            var currentClass = await _trainingService.GetClassByStudentId(student.Id);
            classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var query = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.ClassForumId == classForum.Id && classStudentIds!.Contains(x.StudentId))
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            //Lấy ngẫu nhiên 2 học sinh khác lớp nhưng cùng lesson và course
            var totalRecords = await _classForumResultRepository.Queryable
                            .Where(x => x.ClassForumId == classForum.Id && !classStudentIds!.Contains(x.StudentId))
                            .CountAsync(cancellationToken);

            var skip = totalRecords < STUDENT_RANDOM_TAKE ? 0 : new Random().Next(0, totalRecords - STUDENT_RANDOM_TAKE);
            var queryRandomStudent = await FilterClassForumResultRandom(STUDENT_RANDOM_TAKE, classForum, classStudentIds, skip, cancellationToken);

            var classForumResults = _mapper.Map<IList<ClassForumResultModel>>(query);
            var classForumResultsRandom = _mapper.Map<IList<ClassForumResultModel>>(queryRandomStudent);

            if (classForumResults != null)
            {
                classForumResults = await GetClassForumResult(classForumResults, student);
                classForumResultsRandom = await GetClassForumResult(classForumResultsRandom, student);

                //Lấy ClassForumCurrent - học sinh submit tài khoản hiện tại
                var classForumResultCurrentStudent = classForumResults.FirstOrDefault(x => x.ClassForumId == classForum.Id && x.LessonResultId == request.LessonResultId);
                classForumByStudentModel.ClassForumResultCurrentStudent = classForumResultCurrentStudent;

                var classForumResultRandom = _classForumResultRandomRepository.Queryable.Where(x => x.ClassForumId == classForumResultCurrentStudent!.ClassForumId && x.ClassId == student.ClassId).ToList();

                if (classForumResultRandom.Count < STUDENT_RANDOM_TAKE)
                {
                    int quantityRecord = STUDENT_RANDOM_TAKE - classForumResultRandom.Count;
                    var addRandomClassForumResult = await FilterClassForumResultRandom(quantityRecord, classForum, classStudentIds, skip, cancellationToken);
                    var randomClassForumResultToAdd = _mapper.Map<IList<ClassForumResultRandom>>(addRandomClassForumResult);
                    randomClassForumResultToAdd.ForEach(item =>
                    {
                        item.ClassId = student.ClassId;
                        item.ClassForumId = classForum.Id;
                    });

                    await _classForumResultRandomRepository.AddList(randomClassForumResultToAdd);
                    await _classForumResultRandomRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }


                if (classForumResultCurrentStudent != null && classForumResultCurrentStudent.Status != EnumClassForumResultStatus.Draft)
                {
                    // Lấy bài post học sinh trong lớp
                    var classForumResultAllStudents = classForumResults.Where(x => x.ClassForumId == classForum.Id &&
                                    x.Status != EnumClassForumResultStatus.Draft &&
                                    x.Id != classForumResultCurrentStudent.Id
                                    ).ToList();
                    classForumByStudentModel.ClassForumResultAllStudents = classForumResultAllStudents;

                    // Lấy bài post ngẫu nhiên học sinh khác lớp
                    var classForumResultRandomStudents = classForumResultsRandom?.Where(x =>
                                x.ClassForumId == classForum.Id &&
                                x.Id != classForumResultCurrentStudent?.Id).ToList() ?? new List<ClassForumResultModel>();

                    classForumByStudentModel.ClassForumResultRandomStudents = classForumResultRandomStudents;
                }
            }

            methodResult.Result = classForumByStudentModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<List<ClassForumResult>> FilterClassForumResultRandom(int quantityRecord, ClassForum classForum, IList<Guid>? classStudentIds, int skip, CancellationToken cancellationToken)
        {
            var result = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.ClassForumId == classForum.Id && !classStudentIds!.Contains(x.StudentId) && x.Status != EnumClassForumResultStatus.Draft)
                .Skip(skip)
                .Take(quantityRecord)
                .ToListAsync(cancellationToken);
            return result;
        }

        public async Task<IList<ClassForumResultModel>> GetListClassForumResultRandomToDisplay(int currentRandomQuantity, int skip, StudentModel student, ClassForum classForum, IList<Guid>? classStudentIds, CancellationToken cancellationToken)
        {

            if (currentRandomQuantity < STUDENT_RANDOM_TAKE)
            {
                int quantityRecord = STUDENT_RANDOM_TAKE - currentRandomQuantity;
                var randomClassForumResult = await FilterClassForumResultRandom(quantityRecord, classForum, classStudentIds, skip, cancellationToken);
                var toAdd = _mapper.Map<IList<ClassForumResultRandom>>(randomClassForumResult);
                toAdd.ForEach(item =>
                {
                    item.ClassId = student.ClassId;
                    item.ClassForumId = classForum.Id;
                });

                await _classForumResultRandomRepository.AddList(toAdd);
                await _classForumResultRandomRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }



            return new List<ClassForumResultModel>();
        }

        /// <summary>
        /// Trả về classForumResult với nghiệp vụ tương ứng
        /// </summary>
        /// <param name="classForumResults"></param>
        /// <returns></returns>
        public async Task<IList<ClassForumResultModel>> GetClassForumResult(IList<ClassForumResultModel> classForumResults, StudentModel student)
        {
            var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForumResults.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
            var actions = actionsResult.Content?.Result;

            GetListNotificationRemindQuery query = new GetListNotificationRemindQuery
            {
                ObjectIds = classForumResults.Select(x => x.Id).ToList(),
                Status = EnumNotificationRemindStatus.Off
            };

            var notificationRemind = await _notificationService.GetListNotificationRemind(query);
            var notificationTurnOff = notificationRemind.Content?.Result;

            if (actions != null)
            {
                classForumResults = classForumResults.Where(x => !actions.Any(n => n.IsDisable && n.ObjectId == x.Id)).ToList();
                foreach (var item in classForumResults)
                {
                    var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                    item.CommentNumber = action?.CommentNumber;
                    item.LikeNumber = action?.LikeNumber;
                    item.IsLiked = action?.IsLiked;
                    item.IsTurnedOffNotification = notificationTurnOff!.Any(x => x.ObjectId == item.Id);
                    item.CourseLevel = student!.CourseLevel;
                }
            }

            return classForumResults;
        }
    }
}
