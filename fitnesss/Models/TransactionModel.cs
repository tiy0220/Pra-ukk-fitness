using System;

namespace fitnesss.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public int IdProduk { get; set; }
        public string NamaProduk { get; set; } = string.Empty; // Untuk tampilan JOIN
        public string NamaPelanggan { get; set; } = string.Empty;
        public string NomorUnik { get; set; } = string.Empty;
        public int UangBayar { get; set; }
        public int UangKembali { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}