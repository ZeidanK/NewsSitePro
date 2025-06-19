namespace NewsSite.BL
{
    public class Report
    {
        public int ReportID { get; set; }
        public int SharedID { get; set; }
        public int ReporterUserID { get; set; }
        public string Reason { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
