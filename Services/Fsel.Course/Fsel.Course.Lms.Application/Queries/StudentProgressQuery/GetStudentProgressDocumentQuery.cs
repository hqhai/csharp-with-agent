// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressDocumentQuery : IRequest<MethodResult<IList<DocumentStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetStudentProgressDocumentQueryHandler : IRequestHandler<GetStudentProgressDocumentQuery, MethodResult<IList<DocumentStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly ISystemService _systemService;

        public GetStudentProgressDocumentQueryHandler(IUserService userService, ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository, IDocumentRepository documentRepository, IDocumentResultRepository documentResultRepository, ISystemService systemService)
        {
            _userService = userService;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _documentRepository = documentRepository;
            _documentResultRepository = documentResultRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<DocumentStudentProgressModel>>> Handle(GetStudentProgressDocumentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<DocumentStudentProgressModel>>();

            var studentResults = await _userService.GetUserByStudentIdWithCache(request.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var userId = student.UserId;

            var lesson = await _lessonRepository.Queryable.Include(p => p.LessonModules).FirstOrDefaultAsync(p => p.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var modules = lesson.LessonModules.Where(p => p.LessonConfigType == EnumLessonConfigType.Document).OrderBy(p => p.DisplayOrder).ToList();
            if (modules == null || modules.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var originalIds = modules.Select(p => p.OriginalId).ToList();

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == request.StudentId, cancellationToken);

            if (lessonResult == null || lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var documentResults = await _documentResultRepository.Queryable.Include(x => x.Document).Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == request.StudentId).ToListAsync(cancellationToken);

            var documents = await _documentRepository.Queryable.Where(p => originalIds.Contains(p.OriginalId) && p.VersionStatus == EnumVersionStatus.LastVersion).ToListAsync(cancellationToken);

            var documentStudentProgressModels = new List<DocumentStudentProgressModel>();

            foreach (var module in modules)
            {
                var documentStudentProgressModel = new DocumentStudentProgressModel();
                documentStudentProgressModel.DisplayOrder = module.DisplayOrder;

                var documentResult = documentResults.FirstOrDefault(p => p.LessonModuleId == module.Id);

                if (documentResult != null)
                {
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                    {
                        UserId = userId,
                        CourseResultId = lessonResult.CourseResultId,
                        UnitId = request.UnitId,
                        LessonId = request.LessonId,
                        CourseId = request.CourseId,
                        EnumFeature = EnumFeature.Document,
                        ObjectId = documentResult.Id
                    });

                    if (!featureAccessTimeResult.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResult));
                        return methodResult;
                    }

                    var featureAccessTime = featureAccessTimeResult.Content?.Result;

                    if (featureAccessTime != null)
                    {
                        documentStudentProgressModel.Visit = featureAccessTime.Visit;
                        documentStudentProgressModel.LastVisited = featureAccessTime.LastVisited;
                        documentStudentProgressModel.TimeSpent = featureAccessTime.AccessTime;
                    }

                    documentStudentProgressModel.Status = documentResult.Status;

                    documentStudentProgressModel.Files = documentResult.Document?.Files;

                    documentStudentProgressModels.Add(documentStudentProgressModel);
                }
                else
                {
                    var document = documents.FirstOrDefault(p => p.OriginalId == module.OriginalId);
                    if (document != null)
                    {
                        documentStudentProgressModel.Files = document.Files;
                        documentStudentProgressModel.Status = EnumResultStatus.Unfinished;
                        documentStudentProgressModels.Add(documentStudentProgressModel);
                    }
                }
            }

            methodResult.Result = documentStudentProgressModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
