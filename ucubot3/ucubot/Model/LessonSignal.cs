using System;

namespace ucubot.Model
{
    public enum LessonSignalType
    {
        BoringSimple = -1,
        Interesting = 0,
        BoringHard = 1
    }

    public static class SignalTypeUtils
    {
        public static LessonSignalType ConvertSlackMessageToSignalType(this string message)
        {
            switch (message)
            {
                case "simple":
                    return LessonSignalType.BoringSimple;
                case "interesting":
                    return LessonSignalType.Interesting;
                case "hard":
                    return LessonSignalType.BoringHard;
                default:
                    throw new CanNotParseSlackCommandException(message);
            }
        }
    }
}