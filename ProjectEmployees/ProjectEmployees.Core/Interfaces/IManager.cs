using ProjectEmployees.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectEmployees.Core.Interfaces
{
    public interface IManager
    {
        List<EmployeePair>? CompileCsvData(string value, bool isFilePath = true);

        void ClearCache();
        void ClearCache(string key);
    }
}
