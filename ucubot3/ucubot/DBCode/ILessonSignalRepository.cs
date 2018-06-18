using System.Collections.Generic;

using ucubot.Model;


namespace ucubot.DBCode{

    public interface ILessonSignalRepository
    
    {
    
        IEnumerable<LessonSignalDto> ShowSignalsDB();
        
        LessonSignalDto ShowSignalDB(long id);
        
        bool CreateSignalDB(SlackMessage message);
        
        void RemoveSignalDB(long id);
        
    }
    
}
