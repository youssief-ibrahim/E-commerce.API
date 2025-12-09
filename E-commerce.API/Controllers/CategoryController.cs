using AutoMapper;
using E_commerce.Core.Basic;
using E_commerce.Core.DTO.Category;
using E_commerce.Core.DTO.Product;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IGenericReposatory<Category> GenCategory;
        private readonly IMapper Mapper;
        private readonly IStringLocalizer<Category> localizer;
        private readonly ResponseHandler responesHandler;

        public CategoryController(IGenericReposatory<Category> GenCategory, IMapper Mapper, IStringLocalizer<Category> localizer, ResponseHandler responesHandler)
        {
            this.GenCategory = GenCategory;
            this.Mapper = Mapper;
            this.localizer = localizer;
            this.responesHandler = responesHandler;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await GenCategory.GetAll();
            var data = Mapper.Map<List<AllCategoryDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await GenCategory.GetById(s => s.Id == id);
            if (res == null) return NotFound(responesHandler.NotFound<Category>($"Category with id {id} not found"));
            var data = Mapper.Map<AllCategoryDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("{name:alpha}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var res = await GenCategory.GetById(
               s => s.Name.ToLower().Trim() == name.ToLower().Trim()
            );
            if (res == null) return NotFound(responesHandler.NotFound<Category>($"Category with name {name} not found"));
            var data = Mapper.Map<AllCategoryDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string KeyWord)
        {
            if (string.IsNullOrEmpty(KeyWord)) return BadRequest(responesHandler.BadRequestt<Category>("KeyWord is required"));
            var serchitem = KeyWord.ToLower().Trim();
            var res = await GenCategory.GetAllwithsearch(
                s => s.Name.ToLower().Contains(serchitem)
                );
            var data = Mapper.Map<List<AllCategoryDTO>>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Product/{id}")]
        public async Task<IActionResult> GetCategoryByProductId(int id)
        {
            var res = await GenCategory.GetById(s => s.Products.Any(p => p.Id == id));
            if (res == null) return NotFound(responesHandler.NotFound<Category>($"Category with Product id {id} not found"));
            var data = Mapper.Map<AllCategoryDTO>(res);
            return Ok(responesHandler.Success(data));
        }
        [HttpGet("Product/{name:alpha}")]
        public async Task<IActionResult> GetCategoryByProductName(string name)
        {
            var res = await GenCategory.GetById(s => s.Products.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim()));
            if (res == null) return NotFound(responesHandler.NotFound<Category>($"Category with Product name {name} not found"));
            var data = Mapper.Map<AllCategoryDTO>(res);
            return Ok(responesHandler.Success(data));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDTO createCategoryDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var category = Mapper.Map<Category>(createCategoryDTO);
            await GenCategory.Create(category);
            GenCategory.Save();
            var data = Mapper.Map<AllCategoryDTO>(category);
            return Ok(responesHandler.Success(data));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] AllCategoryDTO allcategorydto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != allcategorydto.Id) return BadRequest(responesHandler.BadRequestt<Category>("shoud id Matched"));

            var category = await GenCategory.GetById(s => s.Id == id);
            if (category == null) return NotFound(responesHandler.NotFound<Category>($"Category with id {id} not found"));

            Mapper.Map(allcategorydto, category);
            GenCategory.update(category);
            GenCategory.Save();
            var data = Mapper.Map<AllCategoryDTO>(category);
            return Ok(responesHandler.Success(data));

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var category = await GenCategory.GetById(s => s.Id == id);
            if (category == null) return NotFound(responesHandler.NotFound<Category>($"category with id {id} not found"));
            GenCategory.delete(category);
            GenCategory.Save();
            var data = Mapper.Map<AllCategoryDTO>(category);
            return Ok(responesHandler.Deleted(data));
        }
    }
    
}
