namespace AHP.DTOs
{
    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AppUserId { get; set; }
        public string CategoryName { get; set; }
    }
}