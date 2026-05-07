using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;    
using Shopping_Mobile.Interfaces; 

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound("Không tìm thấy sản phẩm");
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO productDto)
        {
            var result = await _productService.AddProductAsync(productDto);
            return Ok(result);
        }


        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromProduct(int productId)
        {
           var result = await _productService.RemoveFromProduct(productId);
            if (!result) return NotFound("Không tìm thấy sản phẩm");
            return Ok(new { message = "Đã xóa sản phẩm thành công" });
        }
 
        [HttpGet("search")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Tên tìm kiếm không được để trống");
            }

            var products = await _productService.SearchProductsByNameAsync(name);

            if (products == null || !products.Any())
            {
                return NotFound($"Không tìm thấy sản phẩm nào có tên chứa: {name}");
            }

            return Ok(products);
        }
    }
}