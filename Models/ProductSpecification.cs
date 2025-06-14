using System.ComponentModel.DataAnnotations;

namespace _2280613193_webdocongnghe.Models
{
    public class ProductSpecification
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int SpecificationAttributeId { get; set; }
        public SpecificationAttribute SpecificationAttribute { get; set; }

        [Required, StringLength(200)]
        public string Value { get; set; } 
    }
}
