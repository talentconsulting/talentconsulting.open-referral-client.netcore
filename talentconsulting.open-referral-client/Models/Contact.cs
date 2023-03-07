namespace talentconsulting.open_referral_client.Models
{
    public class Contact : BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}