USE ucubot;
CREATE TABLE lesson_signal (
    id int NOT NULL AUTO_INCREMENT primary key,
    time_stamp DATETIME,
    signal_type INT,
    user_id VARCHAR(255)
);