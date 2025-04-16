namespace _3PS.Models.Vaccines
{
    public class VwVaccineBatchSend
    {
        public decimal? Ticket { get; set; }

        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Surname { get; set; }

        public int? Dosenumber { get; set; }

        public string? Batch { get; set; }

        public string? Brand { get; set; } = null!;

        public int? Brand_Generic { get; set; }

        public string? Vaccine_Type { get; set; } = null!;

        public DateTime? Date { get; set; }

        public int? SendingBatch { get; set; }
    }
}
