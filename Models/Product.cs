using System.ComponentModel.DataAnnotations;

namespace GetBaked.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        //parent reference to category 
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Photo { get; set; }

        //child references
        public List<CartItem>? CartItems { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
