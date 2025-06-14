using _2280613193_webdocongnghe.Models;

namespace _2280613193_webdocongnghe.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<List<Product>> GetAllWithCategoryAsync();
    }
}

