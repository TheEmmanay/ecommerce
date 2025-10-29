CREATE DATABASE IF NOT EXISTS authdb;
USE authdb;

CREATE TABLE IF NOT EXISTS Users (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  FullName VARCHAR(100) NOT NULL,
  Email VARCHAR(100) NOT NULL UNIQUE,
  PasswordHash VARCHAR(255) NOT NULL,
  Role VARCHAR(50) NOT NULL,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Usuario inicial (admin)
INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt)
VALUES (
  'Administrador',
  'admin@example.com',
  '$2a$11$Wq58CyOPmYUz2nFqVsa8L.9ePXR6XfXH8nZQvDFFgbN.7hAqxS9Si', -- hash de "admin123"
  'Admin',
  NOW()
);
