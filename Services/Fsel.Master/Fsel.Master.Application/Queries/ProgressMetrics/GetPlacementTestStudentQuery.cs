// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class PlacementTestStudentModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SkillScoresJson { get; set; }

        public IList<SkillScore>? SkillScores
        {
            get
            {
                return !string.IsNullOrEmpty(SkillScoresJson) ? JsonSerializer.Deserialize<IList<SkillScore>>(SkillScoresJson) : null;
            }
        }

        public Guid? ProgramId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? CompetitionEventId { get; set; }
        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }
    }

    public class GetPlacementTestStudentQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PlacementTestStudentModel>>>
    {
        public IList<Guid>? ProvinceIds { get; set; }
        public IList<Guid>? DistrictIds { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
        public Guid SubjectId { get; set; }
        public IList<Guid>? LevelIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
    }

    public class GetPlacementTestStudentQueryHandler : IRequestHandler<GetPlacementTestStudentQuery, MethodResult<PagingItemsModel<PlacementTestStudentModel>>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<PlacementTestGroup> _placementTestGroupRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CompetitionEvent> _competitionEventRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;

        public GetPlacementTestStudentQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<PlacementTestGroup> placementTestGroupRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CompetitionEvent> competitionEventRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Level> levelRepository)
        {
            _studentRepository = studentRepository;
            _placementTestGroupRepository = placementTestGroupRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _programRepository = programRepository;
            _subjectRepository = subjectRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PlacementTestStudentModel>>> Handle(GetPlacementTestStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PlacementTestStudentModel>>();

            var baseQuery = from s in _studentRepository.Queryable
                            join sce in _studentCompetitionEventRepository.Queryable
                                on s.StudentId equals sce.StudentId
                            where
                              s.ProvinceId != null
                              && s.DistrictId != null
                              && s.SchoolId != null
                              && s.ProvinceId != default
                              && s.DistrictId != default
                              && s.SchoolId != default
                            select new PlacementTestStudentModel
                            {
                                StudentId = s.StudentId,
                                CompetitionEventId = sce.CompetitionEventId,
                                ProvinceId = s.ProvinceId,
                                DistrictId = s.DistrictId,
                                SchoolId = s.SchoolId
                            };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    baseQuery = baseQuery.Where(x => x.Email != null && x.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    baseQuery = baseQuery.Where(x => x.PhoneNumber != null && x.PhoneNumber == request.Keyword);
                }
                else
                {
                    baseQuery = baseQuery.Where(x => x.FullName != null && x.FullName.Contains(request.Keyword));
                }
            }

            if (request.ProvinceIds != null && request.ProvinceIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.ProvinceId.HasValue && request.ProvinceIds.Contains(x.ProvinceId.Value));
            }

            if (request.DistrictIds != null && request.DistrictIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.DistrictId.HasValue && request.DistrictIds.Contains(x.DistrictId.Value));
            }

            if (request.SchoolIds != null && request.SchoolIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.SchoolId.HasValue && request.SchoolIds.Contains(x.SchoolId.Value));
            }

            var placementQuery = from b in baseQuery
                                 join p in _placementTestGroupRepository.Queryable
                                    on b.StudentId equals p.StudentId
                                 join l in _levelRepository.Queryable on p.LevelId equals l.LevelId
                                 where p.Status == EnumResultStatus.Done
                                 select new PlacementTestStudentModel
                                 {
                                     StudentId = b.StudentId,
                                     CompetitionEventId = b.CompetitionEventId,
                                     ProvinceId = b.ProvinceId,
                                     DistrictId = b.DistrictId,
                                     SchoolId = b.SchoolId,
                                     FullName = b.FullName,
                                     Email = b.Email,
                                     PhoneNumber = b.PhoneNumber,
                                     SkillScoresJson = p.SkillScoresJson,
                                     ProgramId = p.ProgramId,
                                     LevelId = p.LevelId,
                                     LevelName = l.LevelName,
                                 };

            placementQuery = from p in placementQuery
                             join prog in _programRepository.Queryable
                                 on p.ProgramId equals prog.ProgramId
                             where prog.SubjectId == request.SubjectId
                             select p;

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.LevelId.HasValue && request.LevelIds.Contains(p.LevelId.Value));
            }

            int totalItem = await placementQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await placementQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<PlacementTestStudentModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
