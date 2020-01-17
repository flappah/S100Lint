using System;

namespace S100Lint.Types.Interfaces
{
    public interface IReportItem
    {
        Enumerations.Chapter Chapter { get; set; }
        Enumerations.Level Level { get; set; }
        string Message { get; set; }
        DateTime TimeStamp { get; set; }
        Enumerations.Type Type { get; set; }
    }
}