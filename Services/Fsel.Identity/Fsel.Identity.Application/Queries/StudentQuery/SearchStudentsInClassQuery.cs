// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsInClassQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SearchStudentsInClassModel>>>
    {
        public Guid ClassId { get; set; }
    }

    public class SearchStudentsInClassQueryHandler : IRequestHandler<SearchStudentsInClassQuery, MethodResult<PagingItemsModel<SearchStudentsInClassModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _lmsCourseService;

        public SearchStudentsInClassQueryHandler(IStudentRepository studentRepository, ITrainingService trainingService, ILmsCourseService lmsCourseService)
        {
            _studentRepository = studentRepository;
            _trainingService = trainingService;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<PagingItemsModel<SearchStudentsInClassModel>>> Handle(SearchStudentsInClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<SearchStudentsInClassModel>> methodResult = new MethodResult<PagingItemsModel<SearchStudentsInClassModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var studentIdsResult = await _trainingService.GetStudentIdsByClassId(request.ClassId);
            if (!studentIdsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentIdsResult.Error?.Content, studentIdsResult.StatusCode);
                return methodResult;
            }
            var studentIds = studentIdsResult.Content?.Result;

            var students = _studentRepository.Queryable.Where(p => studentIds != null && studentIds!.Contains(p.Id)).Include(x => x.Human).Select(i => new SearchStudentsInClassModel
            {
                Id = i.Id,
                FullName = i.Human!.FullName,
                BirthDay = i.Human.Birthday,
                Code = i.Human.Code,
                CreatedDate = i.CreatedDate,
                Email = i.Human.Email,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                students = students.Where(m => (m.FullName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            int totalItem = await students.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await students
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var ptrResult = await _lmsCourseService.GetPTPointByIds(lists.Select(p => p.Id).ToList());
            var ptr = ptrResult.Content?.Result;

            foreach (var item in lists)
            {
                item.PTPoint = ptr!.FirstOrDefault(p => p.StudentId == item.Id) == null ? 0 : ptr!.FirstOrDefault(p => p.StudentId == item.Id)!.PTPoint;
            }

            methodResult.Result = new PagingItemsModel<SearchStudentsInClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
