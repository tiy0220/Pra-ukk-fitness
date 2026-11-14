using System;

namespace fitnesss.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitPrice { get; set; }
        public int Qty { get; set; } = 1;
        public int Subtotal { get; set; }
        public bool IsFreeMemberBenefit { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
