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
    using Fsel.Course.Infrastructure.Repositories;
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
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;
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
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private const int STUDENT_RANDOM_TAKE = 2; // lấy random 2 bài post của học sinh bất kì từ lớp khác, cùng unit, cùng level

        public GetClassForumQueryHandler(IStudentFeedbackRepository studentFeedbackRepository
            , IClassForumRepository classForumRepository
            , IClassForumResultRepository classForumResultRepository
            , IUserService userService
            , AuthContext authContext
            , IMapper mapper
            , ILessonRepository lessonRepository
            , IInteractionService interactionService
            , ITrainingService trainingService
            , INotificationService notificationService
            , IClassForumResultRandomRepository classForumResultRandomRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseResultRepository courseResultRepository)
        {
            _studentFeedbackRepository = studentFeedbackRepository;
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
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<ClassForumByStudentModel>> Handle(GetClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumByStudentModel> methodResult = new MethodResult<ClassForumByStudentModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            #region Validate

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

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
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

            #endregion Validate

            var query = from cfr in _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                        join lr in _lessonResultRepository.Queryable on cfr.LessonResultId equals lr.Id
                        where cfr.Status == EnumClassForumResultStatus.Graded
                            && cfr.ClassForumId == classForum.Id
                            && lr.CourseResultId == courseResult.Id
                        select cfr;

            var totalRecords = await query.CountAsync(cancellationToken);

            //Lấy ngẫu nhiên 2 học sinh khác lớp nhưng cùng lesson và course
            var skip = totalRecords < STUDENT_RANDOM_TAKE ? 0 : new Random().Next(0, totalRecords - STUDENT_RANDOM_TAKE);
            var classForumResults = _mapper.Map<IList<ClassForumResultModel>>(await query.Skip(skip).Take(10).ToListAsync(cancellationToken));

            #region Handler

            if (classForumResults != null)
            {
                classForumResults = await GetClassForumResult(classForumResults, student);

                //Lấy ClassForumCurrent - học sinh submit tài khoản hiện tại
                var classForumResultCurrentStudent = _mapper.Map<ClassForumResultModel>(await query.FirstOrDefaultAsync(x => x.ClassForumId == classForum.Id && x.LessonResultId == request.LessonResultId, cancellationToken));
                classForumByStudentModel.ClassForumResultCurrentStudent = classForumResultCurrentStudent;
                var classForumResultRandom = _classForumResultRandomRepository.Queryable.Where(x => classForumResultCurrentStudent != null && x.ClassForumId == classForumResultCurrentStudent.ClassForumId && x.ClassId == student.ClassId).ToList();

                // Lấy list Random, nếu chưa có thì tạo list random và lưu xuống DB, lần sau call API sẽ lấy list Random được khởi tạo ban đầu
                IList<ClassForumResultModel> listRandom = new List<ClassForumResultModel>();
                if (classForumResultRandom.Count < STUDENT_RANDOM_TAKE)
                {
                    listRandom = await GetRandomClassForumResultFirstTime(classForumResultRandom.Count, skip, student, classForum, classForumResults, cancellationToken);
                }
                else
                {
                    var filterClassForumResult = classForumResultRandom.Select(x => x.ClassForumResult).ToList();
                    listRandom = _mapper.Map<IList<ClassForumResultModel>>(filterClassForumResult);
                }

                ///Xử lý kết quả trả về
                if (classForumResultCurrentStudent != null && classForumResultCurrentStudent.Status != EnumClassForumResultStatus.Draft)
                {
                    //feed back
                    classForumResultCurrentStudent.IsTeacherFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultCurrentStudent.Id && x.Type == EnumStudentFeedBackType.Teacher, cancellationToken);
                    classForumResultCurrentStudent.IsAIFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultCurrentStudent.Id && x.Type == EnumStudentFeedBackType.AI, cancellationToken);

                    // Lấy bài post học sinh trong lớp
                    var classForumResultAllStudents = classForumResults.Where(x => x.ClassForumId == classForum.Id &&
                                    x.Id != classForumResultCurrentStudent.Id && x.Status != EnumClassForumResultStatus.Draft && x.Status != EnumClassForumResultStatus.Pending
                                    ).ToList();
                    classForumByStudentModel.ClassForumResultAllStudents = classForumResultAllStudents;

                    // Lấy bài post ngẫu nhiên học sinh khác lớp
                    var classForumResultRandomStudents = listRandom ?? new List<ClassForumResultModel>();

                    classForumByStudentModel.ClassForumResultRandomStudents = classForumResultRandomStudents;
                }
            }

            #endregion Handler

            methodResult.Result = classForumByStudentModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region Function

        public async Task<IList<ClassForumResultModel>> GetRandomClassForumResultFirstTime(int currentRandomQuantity, int skip, StudentModel student, ClassForum classForum, IList<ClassForumResultModel> classForumResults, CancellationToken cancellationToken)
        {
            int quantityRecord = STUDENT_RANDOM_TAKE - currentRandomQuantity;
            var randomClassForumResult = classForumResults
                .Skip(skip)
                .Take(quantityRecord);

            var toAdd = _mapper.Map<IList<ClassForumResultRandom>>(randomClassForumResult);
            toAdd.ForEach(item =>
            {
                item.ClassId = student.ClassId;
                item.ClassForumId = classForum.Id;
            });

            //Dùng Trasaction để toàn vẹn dữ liệu
            await _classForumResultRandomRepository.ExecuteTransactionAsync(async () =>
            {
                await _classForumResultRandomRepository.AddList(toAdd);
                await _classForumResultRandomRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                return new MethodResult<IList<ClassForumResultModel>>();
            });

            return _mapper.Map<IList<ClassForumResultModel>>(randomClassForumResult);
        }

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
                    item.CourseLevel = student?.CourseLevel ?? default;
                }
            }

            return classForumResults;
        }

        #endregion Function
    }
}
