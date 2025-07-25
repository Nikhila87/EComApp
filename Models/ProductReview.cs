﻿namespace EComAPI.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; }

        public int Rating { get; set; } 

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

