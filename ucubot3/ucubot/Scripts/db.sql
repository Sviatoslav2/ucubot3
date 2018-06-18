CREATE DATABASE ucubot;
USE ucubot;
create user 'ucu_user'@'%' identified by 'rootPW1';
GRANT ALL PRIVILEGES ON ucubot.* TO 'ucu_user'@'%';
FLUSH PRIVILEGES;
