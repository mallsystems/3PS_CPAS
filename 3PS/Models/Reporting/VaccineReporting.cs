namespace _3PS.Models.Reporting
{
    public class VaccineReporting
    {
        public int RECNUM { get; set; }
        public decimal? Ticket { get; set; }
        public string? Id { get; set; }
        public string? Batch { get; set; }
        public DateTime? VaccinationDate { get; set; }
        public int? SendingBatch { get; set; }
        public DateTime? SendingBatchDate { get; set; }
        public string? RequestBody { get; set; }
        public int? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
