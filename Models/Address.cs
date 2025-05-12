using System.Text.Json.Serialization;

namespace EComAPI.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        public string? UserId { get; set; } // Foreign key to AspNetUsers
        public bool IsDefault { get; set; }

        //[JsonIgnore] // Add this to prevent infinite loop
        //public User User { get; set; }
    }

    public class AddressDto
    {
        public string FullName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

}
