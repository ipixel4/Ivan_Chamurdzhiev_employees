using ProjectEmployees.Core.Helpers;
using ProjectEmployees.Core.Interfaces;
using ProjectEmployees.Core.Objects;

namespace ProjectEmployees.Core
{
    public class Manager : IManager
    {
        /// <summary>
        /// Method to build object collection from a provided value that can also be a path to a CSV File.
        /// WARNING: Value to have a raw csv content is not yet implemented
        /// </summary>
        /// <param name="value">String value that would contain either raw csv formatted string or a path to a csv file.</param>
        /// <param name="isFilePath">A flag indicating wether the provided value is file path or raw content.</param>
        /// <returns></returns>
        public List<EmployeePair>? CompileCsvData(string value, bool isFilePath = true)
        {
            if (isFilePath && cachedData.TryGetValue(value, out var cachedValue))
                return cachedValue;

            var dataList = CsvToObjList(value, isFilePath);

            var projectKvp = GenerateProjectDataKVP(dataList);
            if (projectKvp == null) { return null; }

            var employeePairKvp = GenerateEmployeePairsKVP(projectKvp);
            if (employeePairKvp == null) { return null; }

            employeePairKvp = SimplifyEmployeePairKVP(employeePairKvp);
            if (employeePairKvp == null) { return null; }

            var simpleList = UnpairKVP(employeePairKvp);

            simpleList = simpleList.OrderByDescending(ep => ep.SharedTime).ToList();

            if (isFilePath)
                AddToCache(value, simpleList);

            return simpleList;
        }

        /// <summary>
        /// Clear all data from the local cache.
        /// </summary>
        public void ClearCache()
        {
            cachedData.Clear();
        }

        /// <summary>
        /// Clear data from the local cache for specific csv file.
        /// </summary>
        /// <param name="key"></param>
        public void ClearCache(string key)
        {
            if (!string.IsNullOrEmpty(key) && cachedData.ContainsKey(key))
                cachedData.Remove(key);
        }

        /// <summary>
        /// Adds specific csv file processed data to the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        public void AddToCache(string key, List<EmployeePair> list)
        {
            if (cachedData == null)
                cachedData = new Dictionary<string, List<EmployeePair>>();

            cachedData[key] = list;
        }

        /// <summary>
        /// Process the csv data into a workable collection of useable objects.
        /// </summary>
        /// <param name="value">String value that would contain either raw csv formatted string or a path to a csv file.</param>
        /// <param name="isFilePath">A flag indicating wether the provided value is file path or raw content.</param>
        /// <returns>Collection of ProjectDevData objects.</returns>
        /// <exception cref="NotImplementedException">Raw csv content provided in value is not yet implemented.</exception>
        private List<ProjectDevData> CsvToObjList(string value, bool isFilePath = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            List<string> contentLines = new List<string>();

            if (isFilePath && File.Exists(value) && value.ToLower().EndsWith(".csv"))
                using (var fileStream = File.OpenRead(value))
                using (var reader = new StreamReader(fileStream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            contentLines.Add(line);
                    }
                }
            else
            {
                //Comment: NOT IN REQUIREMENTS BUT if a string is passed directly read from other methods, this segment is to split it into lines and populate the contentLines collection
                throw new NotImplementedException("EXC001: Direct reading from raw value string is not yet supported");
            }

            return CsvToObjList(contentLines);
        }

        /// <summary>
        /// Process the csv content lines into a workable collection of useable objects.
        /// </summary>
        /// <param name="csvStringLines">CSV content lines separated into a collection of strings</param>
        /// <returns>Collection of ProjectDevData objects.</returns>
        private List<ProjectDevData> CsvToObjList(List<string> csvStringLines)
        {
            if (csvStringLines.Count == 0)
                return null;

            int empIdCol, projIdCol, dfromCol, dtoCol;

            TryGetCsvColumnIndexes(csvStringLines, out empIdCol, out projIdCol, out dfromCol, out dtoCol);

            List<ProjectDevData> data = new List<ProjectDevData>();
            foreach (var csvLine in csvStringLines)
            {
                var lineData =
                    Extensions.GetProjectDevDataFromCsvLine(csvLine,
                    empIdCol: empIdCol,
                    projIdCol: projIdCol,
                    dFromCol: dfromCol,
                    dToCol: dtoCol);
                if (lineData != null)
                    data.Add(lineData);
            }

            return data;
        }

        /// <summary>
        /// Groups different projectdevdata objects into a single keyvaluepair collection for the purpos of segmenting the projects information for ease of access.
        /// </summary>
        /// <param name="dataList">Collection of ProjectDevData objects.</param>
        /// <returns>Dictionary that uses project IDs as keys to access referenced projectdevdata collections.</returns>
        private Dictionary<string, List<ProjectDevData>>? GenerateProjectDataKVP(List<ProjectDevData> dataList)
        {
            if (dataList?.Count == 0)
                return null;

            var projectCollection = new Dictionary<string, List<ProjectDevData>>();
            foreach (var data in dataList)
            {
                List<ProjectDevData> project;
                if (!projectCollection.TryGetValue(data.ProjectID, out project))
                {
                    project = new List<ProjectDevData>();
                    projectCollection.Add(data.ProjectID, project);
                }
                project.Add(data);
            }

            return projectCollection;
        }

        /// <summary>
        /// Converts the Dictionary with ProjectDevData into one that uses EmployeePair objects.
        /// Pairs the different employees together based on their shared time within a project.
        /// </summary>
        /// <param name="projectKvp">Dictionary that uses project IDs as keys to access referenced ProjectDevData collections.</param>
        /// <returns>Dictionary that uses project IDs as keys to access referenced EmployeePair collections.</returns>
        private Dictionary<string, List<EmployeePair>>? GenerateEmployeePairsKVP(Dictionary<string, List<ProjectDevData>> projectKvp)
        {
            if (projectKvp == null || projectKvp.Count == 0)
                return null;

            Dictionary<string, List<EmployeePair>> employeePairs = new Dictionary<string, List<EmployeePair>>();
            foreach (var kvp in projectKvp)
            {
                employeePairs.Add(kvp.Key, new List<EmployeePair>());

                int index = 0;
                foreach (var listItem in kvp.Value)
                {
                    index++;
                    var remaining = kvp.Value.Skip(index);
                    foreach (var comp in remaining)
                    {
                        var newPair = Extensions.GenerateEmplPair(listItem, comp);
                        if (newPair != null)
                            employeePairs[kvp.Key].Add(newPair);
                    }
                }
            }
            return employeePairs;
        }

        /// <summary>
        /// For each project in use, the EmployeePair collections are iterated and all repeating instances have their shared time added up, clearing repeating instances.
        /// </summary>
        /// <param name="employeePairs">Dictionary that uses project IDs as keys to access referenced EmployeePair collections.</param>
        /// <returns>Dictionary that uses project IDs as keys to access referenced EmployeePair collections that don't have repeating instances.</returns>
        private Dictionary<string, List<EmployeePair>>? SimplifyEmployeePairKVP(Dictionary<string, List<EmployeePair>> employeePairs)
        {
            if (employeePairs == null || employeePairs.Count == 0)
                return null;

            var newPairs = new Dictionary<string, List<EmployeePair>>();

            foreach (var ep in employeePairs)
            {
                int index = 0;
                foreach (var listItem in ep.Value)
                {
                    if (listItem.SharedTime.Ticks > 0)
                    {
                        index++;
                        var remaining = ep.Value.Skip(index);
                        foreach (var comp in remaining)
                        {
                            if (comp.SharedTime.Ticks > 0 && comp.SameEmployeesFlag(listItem))
                            {
                                listItem.SharedTime += comp.SharedTime;
                                comp.SharedTime = new TimeSpan(0);
                            }
                        }
                    }
                }
                newPairs.Add(ep.Key, ep.Value.Where(ept => ept.SharedTime.Ticks > 0).ToList());
            }

            return newPairs;
        }

        /// <summary>
        /// Reverts the KeyValuePairs from the project segmentation into a List that just contains all EmployeePair data.
        /// </summary>
        /// <param name="kvpData"></param>
        /// <returns></returns>
        private List<EmployeePair> UnpairKVP(Dictionary<string, List<EmployeePair>> kvpData)
        {
            List<EmployeePair>? newPairList = new List<EmployeePair>();
            foreach (var kvp in kvpData)
            {
                newPairList.AddRange(kvp.Value);
            }

            return newPairList;
        }

        /// <summary>
        /// Helper method to determine whether the csv content actually adheres to the predetermined order for the columns.
        /// </summary>
        /// <param name="csvStringLines"></param>
        /// <param name="empIdCol"></param>
        /// <param name="projIdCol"></param>
        /// <param name="dfromCol"></param>
        /// <param name="dtoCol"></param>
        /// <returns></returns>
        private bool TryGetCsvColumnIndexes(List<string> csvStringLines, out int empIdCol, out int projIdCol, out int dfromCol, out int dtoCol)
        {
            empIdCol = 0;
            projIdCol = 1;
            dfromCol = 2;
            dtoCol = 3;

            string? header = csvStringLines.FirstOrDefault(l =>
                l.Contains(Constants.emplHeader, StringComparison.OrdinalIgnoreCase) &&
                l.Contains(Constants.projHeader, StringComparison.OrdinalIgnoreCase) &&
                l.Contains(Constants.dateFromHeader, StringComparison.OrdinalIgnoreCase) &&
                l.Contains(Constants.dateToHeader, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(header))
                return false;

            header = header.ToLower();
            string[] columns = header.Split(',', StringSplitOptions.TrimEntries);

            empIdCol = Array.IndexOf(columns, Constants.emplHeader);
            projIdCol = Array.IndexOf(columns, Constants.projHeader);
            dfromCol = Array.IndexOf(columns, Constants.dateFromHeader);
            dtoCol = Array.IndexOf(columns, Constants.dateToHeader);

            if (empIdCol == -1 || projIdCol == -1 || dfromCol == -1 || dtoCol == -1)
            {
                empIdCol = 0;
                projIdCol = 1;
                dfromCol = 2;
                dtoCol = 3;
                return false;
            }

            return true;
        }

        private Dictionary<string, List<ProjectDevData>> projectCollection = new Dictionary<string, List<ProjectDevData>>();

        /// <summary>
        /// Field meant to contain processed files in order to avoid repeat processing of files.
        /// </summary>
        private Dictionary<string, List<EmployeePair>> cachedData = new Dictionary<string, List<EmployeePair>>();
    }
}
