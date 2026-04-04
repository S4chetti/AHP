namespace AHP.DTOs
{
    using System;

    namespace CoreApiProject.DTOs
    {
        public class SurveyDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedDate { get; set; }
            public string AppUserId { get; set; }
        }
    }
}
