using System.Collections.Generic;

namespace AHP.DTOs
{
    public class CreateSurveyDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<CreateQuestionDto> Questions { get; set; }
    }

    public class CreateQuestionDto
    {
        public string Text { get; set; }
        public bool IsRating { get; set; }
    }
}