using AHP.Models.CoreApiProject.Models;

namespace AHP.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int? OptionId { get; set; }
        public Option Option { get; set; }
        public string TextResponse { get; set; }
    }
}