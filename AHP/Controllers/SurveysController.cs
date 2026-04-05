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
            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userName) ?? await _userManager.FindByEmailAsync(userName);

            if (user == null) return Unauthorized();

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

            return Ok();
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
                var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userName)) return Unauthorized();

                var user = await _userManager.FindByNameAsync(userName) ?? await _userManager.FindByEmailAsync(userName);

                if (user == null) return Unauthorized();

                foreach (var dto in dtos)
                {
                    var answer = new Answer
                    {
                        AppUserId = user.Id,
                        QuestionId = dto.QuestionId,
                        OptionId = dto.OptionId > 0 ? dto.OptionId : null,
                        TextResponse = string.IsNullOrWhiteSpace(dto.TextResponse) ? null : dto.TextResponse
                    };
                    await _context.Answers.AddAsync(answer);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        [HttpGet("{id}/Answers")]
        public async Task<IActionResult> GetAnswers(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return NotFound();

            var questionIds = survey.Questions.Select(q => q.Id).ToList();

            var answers = await _context.Answers
                .Include(a => a.AppUser)
                .Include(a => a.Question)
                .ThenInclude(q => q.Options)
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();

            var result = answers.GroupBy(a => a.AppUser)
                .Select(g => new {
                    UserId = g.Key.Id,
                    UserName = g.Key.FullName ?? g.Key.UserName,
                    Email = g.Key.Email,
                    Responses = g.Select(a => new {
                        QuestionText = a.Question.Text,
                        AnswerText = a.OptionId != null ? a.Question.Options.FirstOrDefault(o => o.Id == a.OptionId)?.Text : a.TextResponse
                    }).ToList()
                }).ToList();

            return Ok(result);
        }
    }
}