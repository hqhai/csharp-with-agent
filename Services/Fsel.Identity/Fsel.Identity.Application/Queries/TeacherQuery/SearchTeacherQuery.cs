// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Domain.Models.QueryModels.Teachers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    public class SearchTeacherQuery : SearchTeacherQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherModel>>>
    {
    }

    public class SearchTeacherQueryHandler : IRequestHandler<SearchTeacherQuery, MethodResult<PagingItemsModel<TeacherModel>>>
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;

        public SearchTeacherQueryHandler(IMapper mapper, ITeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherModel>>> Handle(SearchTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TeacherModel>> methodResult = new MethodResult<PagingItemsModel<TeacherModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teacherQuery = _teacherRepository.Queryable
                              .Include(x => x.Human)
                              .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.InvariantCulture);
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    teacherQuery = teacherQuery.Where(m => m.Id == guid);
                }
                else
                {
                    teacherQuery = teacherQuery.Where(m => m.Human != null && m.Human.FullName != null && m.Human.FullName.Contains(request.Keyword));
                }
            }

            var dataQuery = teacherQuery.Select(x => new TeacherModel
            {
                Id = x.Id,
                CourseLevels = x.CourseLevels,
                CourseTypes = x.CourseTypes,
                PassportPath = x.PassportPath,
                UniversityDegreePath = x.UniversityDegreePath,
                CertificationPath = x.CertificationPath,
                PoliceClearancePath = x.PoliceClearancePath,
                HumanId = x.HumanId,
                CreatedDate = x.CreatedDate,
                CreatedUserId = x.CreatedUserId,
                CreatedFullName = x.CreatedFullName,
                UpdatedDate = x.UpdatedDate,
                UpdatedUserId = x.UpdatedUserId,
                UpdatedFullName = x.UpdatedFullName,
                Human = new HumanModel
                {
                    Id = x!.Human!.Id,
                    FullName = x.Human.FullName,
                    AvatarPath = x.Human.AvatarPath,
                    Birthday = x.Human.Birthday,
                    PhoneNumber = x.Human.PhoneNumber,
                    Gender = x.Human.Gender,
                    Email = x.Human.Email,
                    Address = x.Human.Address
                }
            });

            int totalItem = await dataQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await dataQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<TeacherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
