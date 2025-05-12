namespace EComAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }  // <-- Add this
        public Product Product { get; set; }
        public string ImageUrl { get; set; }     // <-- Add this
        public decimal Price { get; set; }       // <-- Add this
        public int Quantity { get; set; }

        // Relationship to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
