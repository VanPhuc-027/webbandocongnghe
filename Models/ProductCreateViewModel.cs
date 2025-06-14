using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace _2280613193_webdocongnghe.Models
{
    public class ProductSpecificationInput
    {
        public int SpecificationAttributeId { get; set; }
        public string Value { get; set; }
    }

    public class ProductCreateViewModel
    {
        // Product fields
        public string Name { get; set; }

        [Range(0.01, 10000000000)]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }

        public IFormFile? ImageFile { get; set; }

        // Specification fields
        public List<ProductSpecificationInput> Specifications { get; set; } = new();
    }
}
