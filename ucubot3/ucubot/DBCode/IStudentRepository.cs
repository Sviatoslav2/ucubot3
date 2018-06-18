using System.Collections.Generic;

using ucubot.Model;


namespace ucubot.DBCode

{

    public interface IStudentRepository
    
    {
    
        IEnumerable<Student> ShowStudentsDB();
        
        Student ShowStudentDB(long id);
        
        bool CreateStudentDB(Student student);
        
        void UpdateStudentDB(Student stud);
        
        bool RemoveStudentDB(long id);
        
    }
    
}