using ProjectEmployees.Core.Objects;
using System.Runtime.CompilerServices;

namespace ProjectEmployees.Core.Helpers
{
    internal static class Extensions
    {
        internal static ProjectDevData? GetProjectDevDataFromCsvLine(string csvDataLine, int empIdCol = 0, int projIdCol = 1, int dFromCol = 2, int dToCol = 3)
        {
            var segments = csvDataLine.Split(',', StringSplitOptions.TrimEntries);

            if (string.IsNullOrEmpty(segments[empIdCol]) || string.IsNullOrEmpty(segments[projIdCol]) || string.IsNullOrEmpty(segments[dFromCol]))
                return null;

            DateTime dateFrom;
            if (!DateTime.TryParse(segments[dFromCol], out dateFrom))
                return null;

            DateTime dateTo;
            if (!DateTime.TryParse(segments[dToCol], out dateTo))
                dateTo = DateTime.Today.AddDays(1); 
            // Presumably, the last date at a project would be considered that the employee works until the end of the day, thus one more day.
            // Considering that - one employee's last day is on the day another starts, which means they would have one day of intersection on a project.

            return new ProjectDevData()
            {
                EmpID = segments[empIdCol],
                ProjectID = segments[projIdCol],
                DateFrom = dateFrom,
                DateTo = dateTo
            };
        }

        internal static EmployeePair? GenerateEmplPair(ProjectDevData first, ProjectDevData second)
        {
            if (first == null || second == null) return null;


            var span = first.GetIntersectionSpan(second);
            if (!span.HasValue)
                return null;

            var res = new EmployeePair()
            {
                FirstID = first.EmpID,
                SecondID = second.EmpID,
                ProjectID = first.ProjectID,
                SharedTime = span.Value
            };

            return res;
        }

        internal static bool IntersectFlag(this ProjectDevData first, ProjectDevData second)
        {
            if (second == null || first.ProjectID != second.ProjectID)
                return false;

            return first.DateFrom <= second.DateFrom && first.DateTo >= second.DateFrom ||
                first.DateFrom >= second.DateFrom && second.DateTo >= first.DateFrom;
        }

        internal static TimeSpan? GetIntersectionSpan(this ProjectDevData first, ProjectDevData second)
        {
            if (second == null || first.ProjectID != second.ProjectID)
                return null;

            if(first.DateFrom <= second.DateFrom)
            {
                if(first.DateTo <= second.DateTo && first.DateTo > second.DateFrom)
                    return first.DateTo - second.DateFrom;

                if (first.DateTo >= second.DateTo)
                    return second.DateTo - second.DateFrom;
            }
            if (first.DateFrom >= second.DateFrom)
            {
                if(first.DateTo >= second.DateTo && second.DateTo > first.DateFrom)
                    return second.DateTo - first.DateFrom;

                if(first.DateTo <= second.DateTo)
                    return first.DateTo - first.DateFrom;
            }

            return null;
        }

        internal static bool SameEmployeesFlag(this EmployeePair primary, EmployeePair target)
        {
            if (target == null || primary.ProjectID != target.ProjectID)
                return false;

            return primary.FirstID == target.FirstID && primary.SecondID == target.SecondID ||
                primary.FirstID == target.SecondID && primary.SecondID == target.FirstID;
        }
    }
}
