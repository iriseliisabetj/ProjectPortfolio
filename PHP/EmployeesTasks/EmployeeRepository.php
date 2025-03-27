<?php

require_once 'ex6/connection.php';
require_once 'Employee.php';

class EmployeeRepository
{

    public function save(Employee $employee)
    {
        try {
            $db = getConnection();
            $stmt = $db->prepare("INSERT INTO employees (firstName, lastName, picture) VALUES (:firstName, :lastName, :picture)");
            $stmt->bindValue(':firstName', $employee->firstName, PDO::PARAM_STR);
            $stmt->bindValue(':lastName', $employee->lastName, PDO::PARAM_STR);
            $stmt->bindValue(':picture', $employee->picture, PDO::PARAM_STR);
            $stmt->execute();
        } catch (PDOException $e) {
            error_log($e->getMessage());
            return ["Unable to save employee at this time."];
        }
        return [];
    }

    public function update(Employee $employee): array
    {
        if (is_null($employee->id)) {
            return ["Employee ID is missing."];
        }

        $errors = $employee->validate();
        if (!empty($errors)) {
            return $errors;
        }

        try {
            $db = getConnection();
            $stmt = $db->prepare("UPDATE employees SET firstName = :firstName, lastName = :lastName, picture = :picture WHERE id = :id");
            $stmt->bindValue(':firstName', $employee->firstName);
            $stmt->bindValue(':lastName', $employee->lastName);
            $stmt->bindValue(':picture', $employee->picture);
            $stmt->bindValue(':id', $employee->id, PDO::PARAM_INT);
            $stmt->execute();
            return [];
        } catch (PDOException $e) {
            error_log($e->getMessage());
            return ["Unable to update employee at this time."];
        }
    }

    public function delete(int $id): ?array
    {
        try {
            $db = getConnection();
            $stmt = $db->prepare("DELETE FROM employees WHERE id = :id");
            $stmt->bindValue(':id', $id, PDO::PARAM_INT);
            $stmt->execute();
            return null;
        } catch (PDOException $e) {
            error_log($e->getMessage());
            return ["Unable to delete employee at this time."];
        }
    }

    public function findById(int $id): ?Employee
    {
        $db = getConnection();
        $stmt = $db->prepare("SELECT e.id, e.firstName, e.lastName, e.picture, COUNT(t.id) AS taskCount
                                    FROM employees e
                                    LEFT JOIN tasks t ON e.id = t.employeeId
                                    WHERE e.id = :id
                                    GROUP BY e.id");
        $stmt->bindValue(':id', $id, PDO::PARAM_INT);
        $stmt->execute();
        $employee = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($employee) {
            $emp = new Employee(
                $employee['id'],
                $employee['firstName'],
                $employee['lastName'],
                $employee['picture']
            );
            $emp->taskCount = (int)$employee['taskCount'];
            return $emp;
        }
        return null;
    }

    public function getAll(): array
    {
        $db = getConnection();
        $stmt = $db->prepare("SELECT e.id, e.firstName, e.lastName, e.picture, COUNT(t.id) AS taskCount
                                    FROM employees e
                                    LEFT JOIN tasks t ON e.id = t.employeeId
                                    GROUP BY e.id");
        $stmt->execute();
        $results = $stmt->fetchAll(PDO::FETCH_ASSOC);

        $employees = [];
        foreach ($results as $row) {
            $emp = new Employee(
                $row['id'],
                $row['firstName'],
                $row['lastName'],
                $row['picture']
            );
            $emp->taskCount = (int)$row['taskCount'];
            $employees[] = $emp;
        }

        return $employees;
    }
}
