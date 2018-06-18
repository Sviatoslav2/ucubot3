using System;

namespace ucubot.Model
{
    public class LessonSignalDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public LessonSignalType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}