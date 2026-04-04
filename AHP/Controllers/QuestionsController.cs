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
    public class QuestionsController : ControllerBase
    {
        private readonly IGenericRepository<Question> _repository;

        public QuestionsController(IGenericRepository<Question> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questions = await _repository.GetAllAsync();
            var dtos = questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                SurveyId = q.SurveyId
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var question = await _repository.GetByIdAsync(id);
            if (question == null) return NotFound();

            var dto = new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                SurveyId = question.SurveyId
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Add(QuestionDto dto)
        {
            var question = new Question
            {
                Text = dto.Text,
                SurveyId = dto.SurveyId
            };
            await _repository.AddAsync(question);
            await _repository.SaveAsync();
            return Ok(question);
        }

        [HttpPut]
        public async Task<IActionResult> Update(QuestionDto dto)
        {
            var question = await _repository.GetByIdAsync(dto.Id);
            if (question == null) return NotFound();

            question.Text = dto.Text;

            _repository.Update(question);
            await _repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _repository.GetByIdAsync(id);
            if (question == null) return NotFound();

            _repository.Remove(question);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}