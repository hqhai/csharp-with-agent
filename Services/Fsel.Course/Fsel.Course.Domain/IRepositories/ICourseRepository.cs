using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ICourseRepository : IRepository<EntityCourse>
    {
    }
}