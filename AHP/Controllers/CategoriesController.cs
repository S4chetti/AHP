using AHP.DTOs;
using AHP.Models;
using AHP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AHP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IGenericRepository<Category> _repository;

        public CategoriesController(IGenericRepository<Category> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repository.GetAllAsync();
            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(CategoryDto dto)
        {
            var category = new Category { Name = dto.Name };
            await _repository.AddAsync(category);
            await _repository.SaveAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound();

            _repository.Remove(category);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}