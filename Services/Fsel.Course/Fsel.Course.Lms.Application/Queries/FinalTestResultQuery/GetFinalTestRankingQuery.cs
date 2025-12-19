// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestRankingQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestRankingQueryHandler : IRequestHandler<GetFinalTestRankingQuery, MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetFinalTestRankingQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, IUserService userService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> Handle(GetFinalTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult = new MethodResult<PagingItemsModel<TestResultRankingModel>>();
            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);

            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }

            var query = _finalTestResultRepository.Queryable.Where(x => x.FinalTestId == finalTestResult.FinalTestId && x.CourseId == finalTestResult.CourseId && x.Status == EnumResultStatus.Done)
                                                            .Select(x => new TestResultRankingModel
                                                            {
                                                                WorkingTime = x.WorkingTime,
                                                                CorrectCount = x.CorrectCount,
                                                                CorrectTotal = x.CorrectTotal,
                                                                Id = x.Id,
                                                                Percent = x.CorrectTotal != 0 ? Math.Round((double)x.CorrectCount * 100 / x.CorrectTotal, 0) : default,
                                                                CreatedDate = x.CreatedDate,
                                                                Status = x.Status,
                                                                StudentId = x.StudentId,
                                                                Score = x.CorrectCount,
                                                                UpdatedDate = x.UpdatedDate,
                                                            });
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Percent).ThenBy(x => x.WorkingTime)
                                   .ApplyPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result;
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.IsCurrentStudent = student?.Id == finalTestResult.StudentId;
                item.FullName = student?.User?.FullName;
                item.AvatarPath = student?.User?.AvatarPath;
                testResultRankings.Add(item);
            }

            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}