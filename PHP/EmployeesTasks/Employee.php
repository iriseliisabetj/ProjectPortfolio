<?php

require_once "ex6/connection.php";

class Employee
{
    public ?int $id;
    public string $firstName;
    public string $lastName;
    public string $picture;
    public int $taskCount;

    public function __construct(?int $id = null, string $firstName = '', string $lastName = '', string $picture = 'default.jpg')
    {
        $this->id = $id;
        $this->firstName = $firstName;
        $this->lastName = $lastName;
        $this->picture = $picture;
        $this->taskCount = 0;
    }

    public function validate(): array
    {
        $errors = [];
        if (strlen($this->firstName) < 1 || strlen($this->firstName) > 21) {
            $errors[] = "First name must be between 1 and 21 characters.";
        }
        if (strlen($this->lastName) < 2 || strlen($this->lastName) > 22) {
            $errors[] = "Last name must be between 2 and 22 characters.";
        }
        return $errors;
    }
}
