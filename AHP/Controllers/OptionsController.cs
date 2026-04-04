using AHP.DTOs;
using AHP.DTOs.CoreApiProject.DTOs;
using AHP.Models;
using AHP.Models.CoreApiProject.Models;
using AHP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AHP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OptionsController : ControllerBase
    {
        private readonly IGenericRepository<Option> _repository;

        public OptionsController(IGenericRepository<Option> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var options = await _repository.GetAllAsync();
            var dtos = options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                QuestionId = o.QuestionId
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var option = await _repository.GetByIdAsync(id);
            if (option == null) return NotFound();

            var dto = new OptionDto
            {
                Id = option.Id,
                Text = option.Text,
                QuestionId = option.QuestionId
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Add(OptionDto dto)
        {
            var option = new Option
            {
                Text = dto.Text,
                QuestionId = dto.QuestionId
            };
            await _repository.AddAsync(option);
            await _repository.SaveAsync();
            return Ok(option);
        }

        [HttpPut]
        public async Task<IActionResult> Update(OptionDto dto)
        {
            var option = await _repository.GetByIdAsync(dto.Id);
            if (option == null) return NotFound();

            option.Text = dto.Text;

            _repository.Update(option);
            await _repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var option = await _repository.GetByIdAsync(id);
            if (option == null) return NotFound();

            _repository.Remove(option);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}