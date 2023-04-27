// Copyright (c) Atlantic. All rights reserved.

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
                              .Select(x => new TeacherModel
                              {
                                  Id = x.Id,
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

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                teacherQuery = teacherQuery.Where(m => m.Id.ToString() == request.Keyword || m.Human!.FullName!.Contains(request.Keyword));
            }

            int totalItem = await teacherQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await teacherQuery
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
