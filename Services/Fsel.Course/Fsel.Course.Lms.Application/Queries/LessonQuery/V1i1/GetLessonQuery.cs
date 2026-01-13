// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetLessonQueryHandler(ILessonRepository lessonRepository,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            else if (lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(lessonResult.Status));
                return methodResult;
            }

            methodResult.Result = await GetLesson(lessonResult, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonModel?> GetLesson(LessonResult? lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var lesson = await _lessonRepository.Queryable
                            .Include(x => x.LessonInstructions)
                            .Include(x => x.LessonVideos)
                            .FirstOrDefaultAsync(x => x.Id == lessonResult.LessonId, cancellationToken: cancellationToken);
            if (lesson == null)
            {
                return default;
            }
            lessonResult = await GetLessonResult(lessonResult, cancellationToken);
            var lessonDto = _mapper.Map<LessonModel>(lesson);
            lessonDto.LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(lesson.LessonInstructions.OrderBy(x => x.CreatedDate).ToList());
            lessonDto.VideoId = lesson.LessonVideos.Select(x => x.VideoId).FirstOrDefault();
            if (lessonResult != null)
            {
                lessonDto.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
                var classForumResult = lessonResult.ClassForumResults.FirstOrDefault();
                if (lessonResult.VideoResult?.Status == EnumResultStatus.Done)
                {
                    lessonDto.IsClassForumLock = false;
                }
                if (lessonResult.HomeWorkResults.Any(x => x.Status != EnumResultStatus.Unfinished) || (classForumResult != null && classForumResult.Status.HasValue && classForumResult.Status != EnumClassForumResultStatus.Draft))
                {
                    lessonDto.IsHomeWorkLock = false;
                }
            }
            return lessonDto;
        }

        private async Task<LessonResult?> GetLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            return await _lessonResultRepository.Queryable.Include(x => x.VideoResults)
                                                        .Include(x => x.HomeWorkResults.Where(x => x.LessonResultId == lessonResult.Id))
                                                        .Include(x => x.ClassForumResults.Where(x => x.LessonResultId == lessonResult.Id))
                                                        .Where(x => x.Id == lessonResult.Id)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
