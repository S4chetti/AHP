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
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _context.Surveys.Include(s => s.Category).ToListAsync();
            var dtos = surveys.Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate,
                AppUserId = s.AppUserId,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : "Genel"
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
                    else if (q.Options != null)
                    {
                        foreach (var optText in q.Options)
                        {
                            question.Options.Add(new Option { Text = optText });
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
            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();
            var user = await _userManager.FindByNameAsync(userName) ?? await _userManager.FindByEmailAsync(userName);
            if (user == null) return Unauthorized();

            var submissionTime = DateTime.Now;

            foreach (var dto in dtos)
            {
                var answer = new Answer
                {
                    AppUserId = user.Id,
                    QuestionId = dto.QuestionId,
                    OptionId = dto.OptionId > 0 ? dto.OptionId : null,
                    TextResponse = string.IsNullOrWhiteSpace(dto.TextResponse) ? null : dto.TextResponse,
                    SubmissionDate = submissionTime
                };
                await _context.Answers.AddAsync(answer);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("MySurveys")]
        public async Task<IActionResult> GetMySurveys()
        {
            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userName) ?? await _userManager.FindByEmailAsync(userName);
            if (user == null) return Unauthorized();

            var surveys = await _context.Surveys
                .Include(s => s.Category)
                .Where(s => s.AppUserId == user.Id)
                .ToListAsync();

            var dtos = surveys.Select(s => new SurveyDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                CreatedDate = s.CreatedDate,
                CategoryId = s.CategoryId,
                CategoryName = s.Category?.Name
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}/Results")]
        public async Task<IActionResult> GetSurveyResults(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return NotFound();

            var answers = await _context.Answers
                .Include(a => a.AppUser)
                .Where(a => survey.Questions.Select(q => q.Id).Contains(a.QuestionId))
                .ToListAsync();

            var result = answers.GroupBy(a => new { a.AppUserId, a.SubmissionDate, a.AppUser })
                .Select(g => new
                {
                    UserName = g.Key.AppUser.FullName ?? g.Key.AppUser.UserName,
                    Email = g.Key.AppUser.Email,
                    Date = g.Key.SubmissionDate.ToString("dd.MM.yyyy HH:mm"),
                    Responses = g.Select(a => new
                    {
                        QuestionId = a.QuestionId,
                        AnswerText = a.OptionId != null
                            ? survey.Questions.FirstOrDefault(q => q.Id == a.QuestionId)?.Options.FirstOrDefault(o => o.Id == a.OptionId)?.Text
                            : a.TextResponse
                    }).ToList()
                }).ToList();

            return Ok(new
            {
                survey.Title,
                Questions = survey.Questions.Select(q => new { q.Id, q.Text }).ToList(),
                Participants = result
            });
        }

        [HttpGet("{id}/Answers")]
        public async Task<IActionResult> GetAnswers(int id)
        {
            var survey = await _context.Surveys.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == id);
            if (survey == null) return NotFound();

            var questionIds = survey.Questions.Select(q => q.Id).ToList();
            var answers = await _context.Answers
                .Include(a => a.AppUser)
                .Include(a => a.Question)
                .ThenInclude(q => q.Options)
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();

            var result = answers.GroupBy(a => new { a.AppUser, a.SubmissionDate })
                .Select(g => new {
                    UserId = g.Key.AppUser.Id,
                    UserName = g.Key.AppUser.FullName ?? g.Key.AppUser.UserName,
                    Email = g.Key.AppUser.Email,
                    Date = g.Key.SubmissionDate.ToString("dd.MM.yyyy HH:mm"),
                    Responses = g.Select(a => new {
                        QuestionText = a.Question.Text,
                        AnswerText = a.OptionId != null ? a.Question.Options.FirstOrDefault(o => o.Id == a.OptionId)?.Text : a.TextResponse
                    }).ToList()
                }).ToList();

            return Ok(result);
        }
    }
}