USE ucubot;
CREATE TABLE student (
    id int NOT NULL AUTO_INCREMENT primary key,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255) NOT NULL,
    user_id VARCHAR(255) NOT NULL UNIQUE
);