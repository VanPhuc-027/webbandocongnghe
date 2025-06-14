using System.ComponentModel.DataAnnotations;

namespace _2280613193_webdocongnghe.Models
{
    public class SpecificationAttribute
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } 

        public List<ProductSpecification>? ProductSpecifications { get; set; }
    }
}
