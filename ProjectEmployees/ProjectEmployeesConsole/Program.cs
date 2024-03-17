// See https://aka.ms/new-console-template for more information
using ProjectEmployees.Core;

Console.WriteLine("Hello, World!");


string dummyFile = "D:\\work\\projectemployees_test.csv";

var mngr = new Manager();

var dataList = mngr.CompileCsvData(dummyFile);
if(dataList != null)
dataList.ForEach(data => Console.WriteLine($"{data.ProjectID} - {data.FirstID} + {data.SecondID} - {data.SharedTime.TotalDays} Days"));