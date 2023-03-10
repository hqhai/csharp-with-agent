using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetVideoQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;

        public GetVideoQueryHandler(IMapper mapper, IVideoRepository videoRepository
            )
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var query = from i in _videoRepository.Queryable
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.TimeCodeExcercises.Where(x => !x.IsDeleted && x.Excercise != null))
                                .ThenInclude(x => x.Excercise ?? new Excercise())
                                .ThenInclude(x => x.ExcerciseQuestions.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.Question ?? new Question())
                                .Where(x => x.Id == request.Id)
                        select new VideoModel
                        {
                            Id = i.Id,
                            Name = i.Name,
                            VideoFilePath = i.VideoFilePath,
                            IsActive = i.IsActive,
                            TeacherId = i.TeacherId,
                            CourseLevel = i.CourseLevel,
                            VideoTimeCodes = i.VideoTimeCodes.Select(x => new VideoTimeCodeModel
                            {
                                Id = x.Id,
                                DisplayTime = x.DisplayTime,
                                ExecutionTime = x.ExecutionTime,
                                TimeCodeType = x.TimeCodeType,
                                VideoId = x.VideoId,
                                Excercises = x.TimeCodeExcercises.Where(n => n.Excercise != null).Select(n => n.Excercise ?? new Excercise()).Select(n => new ExerciseModel
                                {
                                    Id = n.Id,
                                    QuestionType = n.QuestionType,
                                    MediaPost = n.MediaPost,
                                    CourseSkill = n.CourseSkill,
                                    Questions = n.ExcerciseQuestions.Where(n => n.Question != null).Select(m => m.Question ?? new Question()).Select(m => new QuestionModel()
                                    {
                                        Id = m.Id,
                                        QuestionType = m.QuestionType,
                                        IsSave = m.IsSave,
                                        Config = m.Config
                                    }).ToList()
                                }).ToList(),
                            }).ToList(),
                        };
            /*var video2 = from i in _videoRepository.Queryable.Include(i => i.VideoTimeCode)
                         .Include(i => i.VideoTimeCode.TimeCodeExcercises).Include(i => i.VideoTimeCode.TimeCodeExcercises)
                         .Include(i => i.TimeCodeExcercise.Excercise).Include(i => i.ExcerciseQuestion.Question)
                         select i;*/

            var video = await query.FirstOrDefaultAsync();
            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumVideoErrorCode.VD01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            methodResult.Result = video;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}