namespace EComAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; } 
        public string StripeSessionId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

     
        public string FullName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }


    

}
