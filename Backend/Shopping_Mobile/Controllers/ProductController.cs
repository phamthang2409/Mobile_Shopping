using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Sử dụng Primary Constructor giúp inject dependency gọn gàng, loại bỏ constructor cũ
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Logic check null và quăng KeyNotFoundException đã nằm trong Service
            var product = await productService.GetProductByIdAsync(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDTO productDto)
        {
            await productService.AddProductAsync(productDto);
            return Ok(new { message = "Thêm sản phẩm thành công!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDTO productDto)
        {
            await productService.UpdateProductAsync(id, productDto);
            return Ok(new { message = "Cập nhật sản phẩm thành công!" });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromProduct(int productId)
        {
            // Đổi tên hàm gọi sang Service thành DeleteProductAsync cho khớp với ProductService
            await productService.DeleteProductAsync(productId);
            return Ok(new { message = "Đã xóa sản phẩm thành công." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Tên tìm kiếm không được để trống.");
            }

            // Gọi đúng hàm SearchProductsAsync đã map qua DTO trong ProductService
            var products = await productService.SearchProductsAsync(name);
            return Ok(products);
        }
    }
}