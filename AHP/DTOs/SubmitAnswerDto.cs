namespace AHP.DTOs
{
    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string? TextResponse { get; set; }
    }
}