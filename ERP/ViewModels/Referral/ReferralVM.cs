namespace ERP.ViewModels.Referral
{
    public class ReferralVM
    {
        public int ReferralID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string CreateON { get; set; }
        public string Description { get; set; }
        public DateTime? FirstView { get; set; }
        public string Guid { get; set; }
        public string? GuidGroup { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Post { get; set; }
        public string Image { get; set; }
        public string ReceiverFullName2 { get; set; }
        public string ReceiverPost { get; set; }
        public string ReceiverImage { get; set; }
    }
}
