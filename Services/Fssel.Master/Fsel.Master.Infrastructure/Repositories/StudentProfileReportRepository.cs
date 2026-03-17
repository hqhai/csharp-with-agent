// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Infrastructure.Repositories
{
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;

    public class StudentProfileReportRepository : MasterBaseRepository<StudentProfileReport>, IStudentProfileReportRepository
    {
        public StudentProfileReportRepository(MasterDBContext context) : base(context)
        {
        }
    }
}
