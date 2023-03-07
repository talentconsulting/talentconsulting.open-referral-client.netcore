namespace talentconsulting.open_referral_client.Models
{
    public class Pagination
    {
        public int Number { get; set; }

        public int Size { get; set; }

        public int TotalPages { get; set; }

        public int TotalElements { get; set; }

        public bool First { get; set; }

        public bool Last { get; set; }
    }
}

