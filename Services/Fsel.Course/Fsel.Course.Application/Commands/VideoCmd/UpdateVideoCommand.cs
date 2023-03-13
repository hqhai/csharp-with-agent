using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.VideoCmd
{
    public class UpdateVideoCommand : UpdateVideoCommandModel, IRequest<MethodResult<VideoModel>>
    {
    }

    public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExcerciseRepository _excerciseRepository;
        private readonly IExcerciseQuestionRepository _excerciseQuestionRepository;
        private readonly ITimeCodeExcerciseRepository _timeCodeExcerciseRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;

        public UpdateVideoCommandHandler(IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IExcerciseRepository excerciseRepository
            , IExcerciseQuestionRepository excerciseQuestionRepository
            , ITimeCodeExcerciseRepository timeCodeExcerciseRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _excerciseRepository = excerciseRepository;
            _excerciseQuestionRepository = excerciseQuestionRepository;
            _timeCodeExcerciseRepository = timeCodeExcerciseRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            #region Validation

            var IsLessonVideo = await _videoRepository.IsVideoLesson(request.Id);

            if (IsLessonVideo)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumVideoErrorCode.VD02V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            if (request.VideoTimeCodes == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumVideoTimeCodeErrorCode.VTC04C));
                return methodResult;
            }

            // Lưu dữ liệu Video
            var video = await _videoRepository.GetIncludeByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumVideoTimeCodeErrorCode.VTC03V));
                return methodResult;
            }
            video = _mapper.Map(request, video);

            request.VideoTimeCodes.ForEach(x =>
            {
                if (x == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                }
                else
                {
                    if (_videoTimeCodeRepository.IsIdsInValid(new List<Guid>() { x.Id })) // check value khong co trong db == true
                    {
                        VideoTimeCode videoTimeCode = video.VideoTimeCodes[request.VideoTimeCodes.IndexOf(x)];
                        x.Excercises.ForEach(n =>
                        {
                            if (_excerciseRepository.IsIdsInValid(new List<Guid>() { n.Id }))
                            { }
                            else if (n == null)
                            {
                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                            }
                            else
                            {
                                Excercise excercise = _mapper.Map<Excercise>(n);
                                videoTimeCode.TimeCodeExcercises.Add(new TimeCodeExcercise
                                {
                                    Excercise = excercise
                                });
                                n.Questions.ForEach(q =>
                                {
                                    if (_excerciseRepository.IsIdsInValid(new List<Guid>() { q.Id }))
                                    { }
                                    else if (q == null)
                                    {
                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                        methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                                    }
                                    else
                                    {
                                        Question question = _mapper.Map<Question>(q);
                                        excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
                                        {
                                            Question = question
                                        });

                                        if (!question.IsValid())
                                        {
                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                        }
                                    }
                                });

                                if (!excercise.IsValid())
                                {
                                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                    methodResult.AddResultFromErrorList(excercise.ErrorMessages);
                                }
                            }
                        });
                        if (!videoTimeCode.IsValid())
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddResultFromErrorList(videoTimeCode.ErrorMessages);
                        }
                    }
                    else
                    {
                        VideoTimeCode videoTimeCode = video.VideoTimeCodes[request.VideoTimeCodes.IndexOf(x)];
                        // excercises trong videotimecode
                        var excercises = _excerciseRepository.Queryable
                                .Include(x => x.TimeCodeExcercises)
                                .Where(x => x.TimeCodeExcercises.Select(e => e.VideoTimeCodeId).Contains(videoTimeCode.Id)).ToList();

                        x.Excercises.ForEach(n =>
                        {
                            if (n == null)
                            {
                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                            }
                            else
                            {
                                if (!excercises.Any(f => f.Id == n.Id)) //n - excercises khong co trong data excercises-videoTimecode
                                {
                                    var excercisevdt = _excerciseRepository.Queryable.FirstOrDefault(e => e.Id == n.Id);

                                    if (excercisevdt == null)
                                    {
                                        Excercise excercise = _mapper.Map<Excercise>(n);
                                        videoTimeCode.TimeCodeExcercises.Add(new TimeCodeExcercise
                                        {
                                            Excercise = excercise
                                        });
                                        var questions = _questionRepository.Queryable
                                            .Include(x => x.ExcerciseQuestions)
                                            .Where(x => x.ExcerciseQuestions.Select(e => e.ExcerciseId).Contains(excercise.Id)).ToList();
                                        n.Questions.ForEach(q =>
                                        {
                                            if (q == null)
                                            {
                                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                                            }
                                            else
                                            {
                                                if (!questions.Any(f => f.Id == q.Id))// q khong co trong questions -Excercise
                                                {
                                                    var questionex = _questionRepository.Queryable.FirstOrDefault(e => e.Id == q.Id);
                                                    if (questionex == null)
                                                    {
                                                        Question question = _mapper.Map<Question>(q);
                                                        _mapper.Map(q, question);
                                                        excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = question
                                                        });

                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _mapper.Map(q, questionex);
                                                        excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = questionex
                                                        });

                                                        if (!questionex.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(questionex.ErrorMessages);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var question = questions.FirstOrDefault(e => e.Id == q.Id);
                                                    _mapper.Map(q, question);
                                                    if (question == null)
                                                    {
                                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                    }
                                                    else
                                                    {
                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        if (!excercise.IsValid())
                                        {
                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                            methodResult.AddResultFromErrorList(excercise.ErrorMessages);
                                        }
                                    }
                                    else
                                    {
                                        _mapper.Map(n, excercisevdt);
                                        videoTimeCode.TimeCodeExcercises.Add(new TimeCodeExcercise
                                        {
                                            Excercise = excercisevdt
                                        });
                                        var questions = _questionRepository.Queryable
                                            .Include(x => x.ExcerciseQuestions)
                                            .Where(x => x.ExcerciseQuestions.Select(e => e.ExcerciseId).Contains(excercisevdt.Id)).ToList();
                                        n.Questions.ForEach(q =>
                                        {
                                            if (q == null)
                                            {
                                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                                            }
                                            else
                                            {
                                                if (!questions.Any(f => f.Id == q.Id))// q khong co trong questions -Excercise
                                                {
                                                    var questionex = _questionRepository.Queryable.FirstOrDefault(e => e.Id == q.Id);
                                                    if (questionex == null)
                                                    {
                                                        Question question = _mapper.Map<Question>(q);
                                                        _mapper.Map(q, question);
                                                        excercisevdt.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = question
                                                        });

                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _mapper.Map(q, questionex);
                                                        excercisevdt.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = questionex
                                                        });

                                                        if (!questionex.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(questionex.ErrorMessages);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var question = questions.FirstOrDefault(e => e.Id == q.Id);
                                                    _mapper.Map(q, question);
                                                    if (question == null)
                                                    {
                                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                    }
                                                    else
                                                    {
                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        if (!excercisevdt.IsValid())
                                        {
                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                            methodResult.AddResultFromErrorList(excercisevdt.ErrorMessages);
                                        }
                                    }
                                }
                                else  //n - excercises co trong data excercises-videoTimecode
                                {
                                    var excercise = excercises.FirstOrDefault(e => e.Id == n.Id);
                                    if (excercise == null)
                                    {
                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                    }
                                    else
                                    {
                                        _mapper.Map(n, excercise);
                                        var questions = _questionRepository.Queryable
                                         .Include(x => x.ExcerciseQuestions)
                                         .Where(x => x.ExcerciseQuestions.Select(e => e.ExcerciseId).Contains(excercise.Id)).ToList();
                                        n.Questions.ForEach(q =>
                                        {
                                            if (q == null)
                                            {
                                                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                methodResult.AddErrorMessage(nameof(EnumVideoErrorCode.VD03V));
                                            }
                                            else
                                            {
                                                if (!questions.Any(f => f.Id == q.Id))// q khong co trong questions -Excercise
                                                {
                                                    var questionex = _questionRepository.Queryable.FirstOrDefault(e => e.Id == q.Id);
                                                    if (questionex == null)
                                                    {
                                                        Question question = _mapper.Map<Question>(q);
                                                        _mapper.Map(q, question);
                                                        excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = question
                                                        });

                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _mapper.Map(q, questionex);
                                                        excercise.ExcerciseQuestions.Add(new ExcerciseQuestion
                                                        {
                                                            Question = questionex
                                                        });

                                                        if (!questionex.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(questionex.ErrorMessages);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var question = questions.FirstOrDefault(e => e.Id == q.Id);
                                                    _mapper.Map(q, question);
                                                    if (question == null)
                                                    {
                                                        methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                    }
                                                    else
                                                    {
                                                        if (!question.IsValid())
                                                        {
                                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                                            methodResult.AddResultFromErrorList(question.ErrorMessages);
                                                        }
                                                    }
                                                }
                                            }
                                        });

                                        if (!excercise.IsValid())
                                        {
                                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                                            methodResult.AddResultFromErrorList(excercise.ErrorMessages);
                                        }
                                    }
                                }
                            }
                        });
                        if (!videoTimeCode.IsValid())
                        {
                            methodResult.StatusCode = StatusCodes.Status400BadRequest;
                            methodResult.AddResultFromErrorList(videoTimeCode.ErrorMessages);
                        }
                    }
                }
            });

            if (!video.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(video.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            #endregion Validation

            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                video = _videoRepository.Update(video);
                await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoModel>(video);
                return methodResult;
            });

            return methodResult;
        }
    }
}