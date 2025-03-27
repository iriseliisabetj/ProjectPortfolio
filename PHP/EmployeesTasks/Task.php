<?php
require_once "ex6/connection.php";

class Task
{
    public ?int $id;
    public string $description;
    public int $estimate;
    public ?int $employeeId;
    public bool $isCompleted;

    public function __construct(?int $id = null, string $description = '', int $estimate = 0, ?int $employeeId = null, bool $isCompleted = false)
    {
        $this->id = $id;
        $this->description = $description;
        $this->estimate = $estimate;
        $this->employeeId = $employeeId;
        $this->isCompleted = $isCompleted;
    }

    public function getStatus(): string
    {
        if ($this->isCompleted) {
            return 'Closed';
        } elseif ($this->employeeId !== null) {
            return 'Pending';
        } else {
            return 'Open';
        }
    }

    public function validate(): array
    {
        $errors = [];
        if (strlen($this->description) < 5 || strlen($this->description) > 40) {
            $errors[] = "Task description must be between 5 and 40 characters.";
        }
        return $errors;
    }
}
