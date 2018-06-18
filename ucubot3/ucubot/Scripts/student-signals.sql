USE ucubot;
CREATE VIEW student_signals AS SELECT student.first_name, student.last_name,
CASE
WHEN lesson_signal.signal_type = -1 then 'Simple'
WHEN lesson_signal.signal_type = 0 then 'Normal'
WHEN lesson_signal.signal_type = 1 then 'Hard'
END AS SignalType,
COUNT(lesson_signal.student_id) AS Count FROM student 
INNER JOIN lesson_signal ON lesson_signal.student_id = student.id GROUP BY lesson_signal.signal_type, student.user_id; 
