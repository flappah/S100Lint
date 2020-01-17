using S100Lint.Types.Interfaces;
using System;

namespace S100Lint.Types
{
    public class ReportItem : IReportItem
    {
        public Enumerations.Chapter Chapter { get; set; }
        public Enumerations.Level Level { get; set; }
        public Enumerations.Type Type { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
    }
}
