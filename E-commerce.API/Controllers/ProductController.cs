using System.Data;
using AutoMapper;
using E_commerce.Core.Basic;
using E_commerce.Core.DTO.Product;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IGenericReposatory<Product> GenProduct;
        private readonly IMapper Mapper;
        private readonly IStringLocalizer<Product> localizer;
        private readonly ResponseHandler responesHandler;

        public ProductController(IGenericReposatory<Product> GenProduct, IMapper Mapper, IStringLocalizer<Product> localizer, ResponseHandler responesHandler)
        {
            this.GenProduct = GenProduct;
            this.Mapper = Mapper;
            this.localizer = localizer;
            this.responesHandler = responesHandler;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res=await GenProduct.GetAll(s=>s.Category);
            var data = Mapper.Map<List<AllProductDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await GenProduct.GetById(s => s.Id == id, s => s.Category);
            if (res == null) return NotFound(responesHandler.NotFound<Product>($"Product with id {id} not found"));
            var data = Mapper.Map<AllProductDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{name:alpha}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var res = await GenProduct.GetById(
               s => s.Name.ToLower().Trim() == name.ToLower().Trim(),
               s => s.Category
            );
            if (res == null) return NotFound(responesHandler.NotFound<Product>($"Product with name {name} not found"));
            var data = Mapper.Map<AllProductDTO>(res);
            return Ok(responesHandler.Success(data));
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string KeyWord)
        {
            if (string.IsNullOrEmpty(KeyWord)) return BadRequest(responesHandler.BadRequestt<Product>("KeyWord is required"));
            var serchitem = KeyWord.ToLower().Trim();
            var res = await GenProduct.GetAllwithsearch(
                s => s.Name.ToLower().Contains(serchitem) || s.Description.ToLower().Contains(serchitem),
                s => s.Category
                );
            var data = Mapper.Map<List<AllProductDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Category/{id}")]

        public async Task<IActionResult> GetProductByCategoryId(int id)
        {
            var res = await GenProduct.FindAll(s => s.CategoryId == id, s => s.Category);
            if (res.Count==0) return NotFound(responesHandler.NotFound<Product>($"ther is no Product with CatogoryId {id}"));
            var data = Mapper.Map<List<AllProductDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Category/{name:alpha}")]
        public async Task<IActionResult> GetProductByCategoryName(string name)
        {
            var res = await GenProduct.FindAll(
              s => s.Category.Name.ToLower().Trim() == name.ToLower().Trim(),
             s => s.Category
            );
            if (res.Count==0) return NotFound(responesHandler.NotFound<Product>($"ther is no Product with Categoryname {name}"));
            var data = Mapper.Map<List<AllProductDTO>>(res);
            return Ok(responesHandler.Success(data));
        }

        [HttpPost]
        //[Authorize("Permission.Product.Create")]
        [Authorize(Roles="User")]
        [Authorize(Roles = "Honer")]
        public async Task<IActionResult> Createsdasd([FromForm] CreateProductDTO createProductDTO)
          {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var product=Mapper.Map<Product>(createProductDTO);
            await GenProduct.Create(product);
            GenProduct.Save();
            var data = Mapper.Map<AllProductDTO>(product);
            return Ok(responesHandler.Success(data));
          }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] AllProductDTO allproductdto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != allproductdto.Id) return BadRequest(responesHandler.BadRequestt<Product>("shoud id Matched"));
            var product = await GenProduct.GetById(s => s.Id == id, r => r.Category);
            var prductcategory = await GenProduct.GetById(s => s.CategoryId == allproductdto.CategoryId,r=>r.Category);
            if(product == null) return NotFound(responesHandler.NotFound<Product>($"Product with id {id} not found"));
            if (prductcategory == null) return NotFound(responesHandler.NotFound<Product>($"Category with id {allproductdto.CategoryId} not found"));

            Mapper.Map(allproductdto,product);
            GenProduct.update(product);
            GenProduct.Save();
            var data=Mapper.Map<AllProductDTO>(product);
            return Ok(responesHandler.Success(data));

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product = await GenProduct.GetById(s => s.Id == id, r => r.Category);
            if (product == null) return NotFound(responesHandler.NotFound<Product>($"Product with id {id} not found"));
            GenProduct.delete(product);
            GenProduct.Save();
            var data = Mapper.Map<AllProductDTO>(product);
            return Ok(responesHandler.Deleted(data));
        }
    }
}
