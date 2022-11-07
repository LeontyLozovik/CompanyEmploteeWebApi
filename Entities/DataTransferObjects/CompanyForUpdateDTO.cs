using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public class CompanyForUpdateDTO : CompanyForManipulationDTO
    {
        public IEnumerable<EmployeeForUpdateDTO> Employees { get; set; }
    }
}
