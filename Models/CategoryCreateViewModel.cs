namespace _2280613193_webdocongnghe.Models
{
	public class CategoryCreateViewModel
	{
		public string Name { get; set; }

		// Các thông số được chọn khi tạo danh mục
		public List<int> SelectedSpecificationAttributeIds { get; set; } = new List<int>();

		// Dùng để hiển thị checkbox
		public List<SpecificationAttribute> AllSpecificationAttributes { get; set; } = new List<SpecificationAttribute>();
	}
}
