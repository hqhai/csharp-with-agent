using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Querys.PlacementTestQuery;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Common.Models.Queries;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid Id { get; set; }
    }
    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery,MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public GetLessonQueryHandler(IMapper mapper, ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            var lesson = await _lessonRepository.GetByIdAsync(request.Id);

            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.LS01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            methodResult.Result = _mapper.Map<LessonModel>(lesson);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
