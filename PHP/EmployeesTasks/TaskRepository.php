<?php

require_once 'ex6/connection.php';
require_once 'Task.php';

class TaskRepository
{

    public function getAll(): array
    {
        $db = getConnection();
        $stmt = $db->prepare("
            SELECT t.id, t.description, t.estimate, t.employeeId, t.isCompleted
            FROM tasks t
        ");

        try {
            $stmt->execute();
            $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
            $tasks = [];
            foreach ($results as $taskData) {
                $tasks[] = new Task(
                    $taskData['id'],
                    $taskData['description'],
                    $taskData['estimate'],
                    $taskData['employeeId'],
                    (bool)$taskData['isCompleted']
                );
            }
            return $tasks;
        } catch (PDOException $e) {
            error_log("Error fetching all tasks: " . $e->getMessage());
            return [];
        }
    }

    public function findById(int $id): ?Task
    {
        $db = getConnection();
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = :id");
        $stmt->bindValue(':id', $id, PDO::PARAM_INT);

        try {
            $stmt->execute();
            $task = $stmt->fetch(PDO::FETCH_ASSOC);

            if ($task) {
                return new Task(
                    $task['id'],
                    $task['description'],
                    $task['estimate'],
                    $task['employeeId'],
                    (bool)$task['isCompleted']
                );
            }
            return null;
        } catch (PDOException $e) {
            error_log("Error finding task by ID: " . $e->getMessage());
            return null;
        }
    }

    public function save(Task $task): array
    {
        $errors = $task->validate();
        if (!empty($errors)) {
            return $errors;
        }

        if ($task->employeeId !== null && !$this->doesEmployeeExist($task->employeeId)) {
            $task->employeeId = null;
        }

        try {
            $db = getConnection();
            $stmt = $db->prepare("
                INSERT INTO tasks (description, estimate, employeeId, isCompleted) 
                VALUES (:description, :estimate, :employeeId, :isCompleted)
            ");
            $stmt->bindValue(':description', $task->description);
            $stmt->bindValue(':estimate', (int)$task->estimate, PDO::PARAM_INT);
            $stmt->bindValue(':employeeId', $task->employeeId !== null ? (int)$task->employeeId : null, $task->employeeId !== null ? PDO::PARAM_INT : PDO::PARAM_NULL);
            $stmt->bindValue(':isCompleted', $task->isCompleted ? 1 : 0, PDO::PARAM_INT);
            $stmt->execute();
            $task->id = (int)$db->lastInsertId();
            return [];
        } catch (PDOException $e) {
            error_log("Error saving task: " . $e->getMessage());
            return ["Unable to save task at this time."];
        }
    }

    public function update(Task $task): array
    {
        if (is_null($task->id)) {
            return ["Task ID is missing."];
        }

        $errors = $task->validate();
        if (!empty($errors)) {
            return $errors;
        }

        if ($task->employeeId !== null && !$this->doesEmployeeExist($task->employeeId)) {
            $task->employeeId = null;
        }

        try {
            $db = getConnection();
            $stmt = $db->prepare("
                UPDATE tasks 
                SET description = :description, estimate = :estimate, employeeId = :employeeId, isCompleted = :isCompleted 
                WHERE id = :id
            ");
            $stmt->bindValue(':description', $task->description);
            $stmt->bindValue(':estimate', (int)$task->estimate, PDO::PARAM_INT);
            $stmt->bindValue(':employeeId', $task->employeeId !== null ? (int)$task->employeeId : null, $task->employeeId !== null ? PDO::PARAM_INT : PDO::PARAM_NULL);
            $stmt->bindValue(':isCompleted', $task->isCompleted ? 1 : 0, PDO::PARAM_INT);
            $stmt->bindValue(':id', $task->id, PDO::PARAM_INT);
            $stmt->execute();
            return [];
        } catch (PDOException $e) {
            error_log("Error updating task: " . $e->getMessage());
            return ["Unable to update task at this time."];
        }
    }

    public function delete(int $id): ?array
    {
        $db = getConnection();
        $stmt = $db->prepare("DELETE FROM tasks WHERE id = :id");
        $stmt->bindValue(':id', $id, PDO::PARAM_INT);

        try {
            $stmt->execute();
            return null;
        } catch (PDOException $e) {
            error_log("Error deleting task: " . $e->getMessage());
            return ["Unable to delete task at this time."];
        }
    }

    private function doesEmployeeExist($employeeId): bool
    {
        $db = getConnection();
        $stmt = $db->prepare("SELECT COUNT(*) FROM employees WHERE id = :id");
        $stmt->bindValue(':id', (int)$employeeId, PDO::PARAM_INT);
        $stmt->execute();
        return $stmt->fetchColumn() > 0;
    }
}
