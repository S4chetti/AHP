using AHP.DTOs;
using AHP.DTOs.CoreApiProject.DTOs;
using AHP.Models;
using AHP.Models.CoreApiProject.Models;
using AHP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AHP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SurveysController : ControllerBase
    {
        private readonly IGenericRepository<Survey> _repository;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public SurveysController(IGenericRepository<Survey> repository, UserManager<AppUser> userManager, AppDbContext context)
        {
            _repository = repository;
            _userManager = userManager;
            _context = context;
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

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return NotFound();

            var result = new
            {
                survey.Id,
                survey.Title,
                survey.Description,
                Questions = survey.Questions.Select(q => new
                {
                    q.Id,
                    q.Text,
                    Options = q.Options.Select(o => new { o.Id, o.Text }).ToList()
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateSurveyDto dto)
        {
            var userName = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userName);
            }

            if (user == null)
            {
                return Unauthorized("Kullanıcı oturumu bulunamadı.");
            }

            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                AppUserId = user.Id,
                CategoryId = dto.CategoryId,
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

            return Ok(new { Message = "Anket başarıyla eklendi." });
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

        [HttpPost("SubmitAnswers")]
        public async Task<IActionResult> SubmitAnswers(List<SubmitAnswerDto> dtos)
        {
            try
            {
                var userName = User.Identity.Name;
                var user = await _userManager.FindByNameAsync(userName);

                if (user == null) return Unauthorized();

                foreach (var dto in dtos)
                {
                    var answer = new Answer
                    {
                        AppUserId = user.Id,
                        QuestionId = dto.QuestionId,
                        OptionId = dto.OptionId > 0 ? dto.OptionId : null,
                        TextResponse = dto.TextResponse
                    };
                    await _context.Answers.AddAsync(answer);
                }
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Cevaplar başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}