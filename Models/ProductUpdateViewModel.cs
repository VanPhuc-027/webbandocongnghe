using System.ComponentModel.DataAnnotations;

namespace _2280613193_webdocongnghe.Models
{
    public class ProductUpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public int BrandId { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        public List<SpecificationInputModel> Specifications { get; set; } = new();
    }

}
