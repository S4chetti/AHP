namespace AHP.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; }
        public bool IsRating { get; set; }
        public List<string> Options { get; set; }
    }
}