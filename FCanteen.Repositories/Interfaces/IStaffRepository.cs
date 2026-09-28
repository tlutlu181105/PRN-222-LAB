using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByCodeAsync(string staffCode);
}