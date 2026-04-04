namespace AHP.Models
{
    using System;
    using System.Collections.Generic;

    namespace CoreApiProject.Models
    {
        public class Survey
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedDate { get; set; }
            public string AppUserId { get; set; }
            public AppUser AppUser { get; set; }
            public ICollection<Question> Questions { get; set; }
        }
    }
}
