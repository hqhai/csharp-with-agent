using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = Fsel.Course.Domain.Entities.Unit;
namespace Fsel.Course.Domain.IRepositories
{
    public interface IUnitRepository : IRepository<Unit>
    {
    }
}
