using AHP.DTOs;
using AHP.DTOs.CoreApiProject.DTOs;
using AHP.Models;
using AHP.Models.CoreApiProject.Models;
using AHP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AHP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SurveysController : ControllerBase
    {
        private readonly IGenericRepository<Survey> _repository;
        private readonly UserManager<AppUser> _userManager;

        public SurveysController(IGenericRepository<Survey> repository, UserManager<AppUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _repository.GetAllAsync();
            var dtos = surveys.Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate,
                AppUserId = s.AppUserId
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var survey = await _repository.GetByIdAsync(id);
            if (survey == null) return NotFound();

            var dto = new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedDate = survey.CreatedDate,
                AppUserId = survey.AppUserId
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateSurveyDto dto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                AppUserId = user.Id,
                Questions = new List<Question>()
            };

            if (dto.Questions != null)
            {
                foreach (var q in dto.Questions)
                {
                    var question = new Question
                    {
                        Text = q.Text,
                        Options = new List<Option>()
                    };

                    if (q.IsRating)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            question.Options.Add(new Option { Text = i.ToString() });
                        }
                    }
                    survey.Questions.Add(question);
                }
            }

            await _repository.AddAsync(survey);
            await _repository.SaveAsync();
            return Ok(survey);
        }

        [HttpPut]
        public async Task<IActionResult> Update(SurveyDto dto)
        {
            var survey = await _repository.GetByIdAsync(dto.Id);
            if (survey == null) return NotFound();

            survey.Title = dto.Title;
            survey.Description = dto.Description;

            _repository.Update(survey);
            await _repository.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _repository.GetByIdAsync(id);
            if (survey == null) return NotFound();

            _repository.Remove(survey);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}