
CREATE TABLE employees (
    id INT AUTO_INCREMENT PRIMARY KEY,
    firstName VARCHAR(50) NOT NULL,
    lastName VARCHAR(50) NOT NULL,
    picture VARCHAR(255) DEFAULT 'default.jpg'
);

INSERT INTO employees VALUES (1, 'Mari', 'Tamm', 'profile1.jpg');
INSERT INTO employees VALUES (2, 'Mart', 'Kask', 'profile2.jpg');

CREATE TABLE tasks (
   id INT AUTO_INCREMENT PRIMARY KEY,
   description TEXT NOT NULL,
   estimate INT DEFAULT 0 CHECK (estimate BETWEEN 0 AND 5),
   employeeId INT DEFAULT NULL,
   isCompleted BOOLEAN DEFAULT FALSE,
   FOREIGN KEY (employeeId) REFERENCES employees(id) ON DELETE SET NULL
);

INSERT INTO tasks VALUES (1, 'Redesign website', 3, null, false);
INSERT INTO tasks VALUES (2, 'Meet with client', 2, null, false);
INSERT INTO tasks VALUES (3, 'Hire new manager', 5, null, false);
