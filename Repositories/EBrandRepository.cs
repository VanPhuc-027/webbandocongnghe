using _2280613193_webdocongnghe.Models;
using Microsoft.EntityFrameworkCore;

namespace _2280613193_webdocongnghe.Repositories
{
    public class EBrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;
        public EBrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync();
            return await _context.Brands
        .Include(c => c.Products)
        .ToListAsync();
        }
        public async Task<Brand> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);
            // lấy thông tin kèm theo category
            return await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task AddAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (brand == null) return;

            // Đặt lại CategoryId của các Product thành null
            foreach (var product in brand.Products)
            {
                product.BrandId = null;
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }

    }
}

