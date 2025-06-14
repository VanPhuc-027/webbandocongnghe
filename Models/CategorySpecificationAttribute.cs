namespace _2280613193_webdocongnghe.Models
{
	public class CategorySpecificationAttribute
	{
		public int Id { get; set; }
		public int CategoryId { get; set; }
		public int SpecificationAttributeId { get; set; }

		public Category Category { get; set; }
		public SpecificationAttribute SpecificationAttribute { get; set; }
	}
}
