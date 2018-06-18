USE ucubot;
ALTER TABLE lesson_signal DROP COLUMN user_id;
ALTER TABLE lesson_signal ADD student_id int NOT NULL;
ALTER TABLE lesson_signal ADD CONSTRAINT fk_student_id FOREIGN KEY (student_id) REFERENCES student(id) 
ON UPDATE RESTRICT ON DELETE RESTRICT;
