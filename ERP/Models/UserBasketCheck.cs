namespace ERP.Models
{
    public class UserBasketCheck
    {
        public UserBasketCheck(string id)
        {
            Id = id;
            basketCheckItems = new List<UserBasketCheckItem>();
        }

        public string Id { get; set; }
        public List<UserBasketCheckItem> basketCheckItems { get; set; }
    }
}
