// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;

        public GetLessonQueryHandler(IMapper mapper,
            ILessonRepository lessonRepository,
            IHomeWorkRepository homeWorkRepository,
            ISkillRepository skillRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            ArgumentNullException.ThrowIfNull(request);
            var lesson = await _lessonRepository.GetIncludeByIdNoTrackingAsync(request.Id);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }

            var video = lesson.LessonVideos.Select(x => x.Video).FirstOrDefault();
            var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.LessonHomeWorks.Where(n => n.LessonId == lesson.Id)).Where(x => x.LessonHomeWorks.Any(n => n.LessonId == lesson.Id)).ToListAsync(cancellationToken);
            var lessonModel = _mapper.Map<LessonModel>(lesson);
            lessonModel.Video = _mapper.Map<VideoModel>(video);
            lessonModel.VideoId = video?.Id;
            lessonModel.ExtraPracticeIds = lesson.LessonExtraPractices.OrderBy(x => x!.CreatedDate).Select(x => x.ExtracPraticeId).ToList();
            lessonModel.ClassForum = _mapper.Map<ClassForumModel>(lesson.ClassForum);
            var skillId = lesson.ClassForum?.SkillId;
            if (skillId.HasValue)
            {
                var skill = await _skillRepository.GetByIdAsync(skillId.Value);
                lessonModel.ClassForum.SkillName = skill?.Name;
            }

            lessonModel.IsActive = lesson.UnitLessons.Any();
            lessonModel.HomeWorks = _mapper.Map<IList<HomeWorkModel>>(homeWorks.OrderBy(x => x.LessonHomeWorks.Select(x => x.CreatedDate).FirstOrDefault()));
            methodResult.Result = lessonModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
