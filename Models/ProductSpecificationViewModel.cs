using Microsoft.AspNetCore.Mvc;

namespace _2280613193_webdocongnghe.Models
{
    public class ProductSpecificationViewModel
    {
        public int ProductId { get; set; }

        public List<SpecificationInputModel> Specifications { get; set; } = new();
    }

    public class SpecificationInputModel
    {
        public int SpecificationAttributeId { get; set; } 
        public string Value { get; set; } = string.Empty;
    }

}
