// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Queries.CategoryQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MassTransit.SignalR.Contracts;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetEnumQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public Domain.Enums.EnumCourseSourceData? EnumCourseSourceData { get; set; }
    }

    public class GetEnumHandler : IRequestHandler<GetEnumQuery, MethodResult<IList<EnumModel>>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;

        public GetEnumHandler(IExamPracticeRepository examPracticeRepository)
        {
            _examPracticeRepository = examPracticeRepository;
        }

        public async Task<MethodResult<IList<EnumModel>>> Handle(GetEnumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumModel>> methodResult = new MethodResult<IList<EnumModel>>();

            switch (request.EnumCourseSourceData)
            {
                case Domain.Enums.EnumCourseSourceData.CourseSkill:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumCourseSkill>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeStatus:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExamPracticeStatus>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeSubType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumExamPracticeSubType>();
                    break;

                case Domain.Enums.EnumCourseSourceData.ExamPracticeType:
                    var data = ConvertHelper.EnumToListModel<EnumExamPracticeType>().AsEnumerable();
                    var activeSet = await _examPracticeRepository.Queryable.AsNoTracking().Where(x => x.Status == EnumExamPracticeStatus.Active)
                                                             .Select(x => x.Type)
                                                             .Distinct()
                                                             .ToListAsync(cancellationToken);

                    var filtered = data.Where(m =>
                              !string.IsNullOrWhiteSpace(m.Value)
                              && Enum.TryParse<EnumExamPracticeType>(m.Value, ignoreCase: true, out var v)
                              && activeSet.Contains(v)).ToList();

                    methodResult.Result = filtered;
                    break;

                case Domain.Enums.EnumCourseSourceData.SectionExamPracticeType:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumSectionExamPracticeType>();
                    break;

                case Domain.Enums.EnumCourseSourceData.CourseLevel:
                    methodResult.Result = ConvertHelper.EnumToListModel<Domain.Enums.EnumCourseLevel>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
