namespace EComAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Maps to the logged-in user
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
