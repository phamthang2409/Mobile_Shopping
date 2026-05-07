using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> AddProductAsync(ProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            product.CreatedAt = DateTime.Now;


            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<bool> RemoveFromProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Product>> SearchProductsByNameAsync(string name)
        {
            return await _context.Products
        .Where(p => p.ProductName.Contains(name))
        .ToListAsync();
        }
    }
}