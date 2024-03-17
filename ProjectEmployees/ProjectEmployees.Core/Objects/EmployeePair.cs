namespace ProjectEmployees.Core.Objects
{
    public sealed class EmployeePair
    {
        public string FirstID {  get; set; }
        public string SecondID { get; set; }
        public string ProjectID { get; set; }
        public TimeSpan SharedTime { get; set; }
    }
}
