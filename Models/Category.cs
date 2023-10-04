namespace GetBaked.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        //child reference to Products (1 cat/ many prod)
        public List<Product>? Products { get; set; }

    }
}
