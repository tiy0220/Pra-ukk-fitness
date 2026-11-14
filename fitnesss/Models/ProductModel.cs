using System;

namespace fitnesss.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string NamaProduk { get; set; } = string.Empty;
        public int HargaProduk { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}