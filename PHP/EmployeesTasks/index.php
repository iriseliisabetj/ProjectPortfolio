<?php

ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

require_once 'ex8/vendor/autoload.php';
require_once 'EmployeeRepository.php';
require_once 'TaskRepository.php';

$latte = new Latte\Engine;

$employeeRepo = new EmployeeRepository();
$taskRepo = new TaskRepository();

$command = $_GET['command'] ?? 'dashboard';

function redirectToFormWithErrors(string $type, array $errors, array $fields, ?int $id = null): void
{
    $queryParams = http_build_query([
        'errors' => urlencode(serialize($errors)),
        'fields' => array_map('urlencode', $fields),
        'id' => $id
    ]);

    header("Location: index.php?command={$type}&{$queryParams}");
    exit();
}

switch ($command) {
    case 'handle_employee':
    case 'handle_task':
        handleRequest($command);
        break;

    case 'delete_employee':
        $employeeId = (int)$_GET['id'];
        if ($employeeRepo->delete($employeeId)) {
            header("Location: index.php?command=employee_list&success=" . urlencode("Employee deleted successfully!"));
        } else {
            header("Location: index.php?command=employee_list&error=" . urlencode("Failed to delete employee."));
        }
        exit();

    case 'delete_task':
        $taskId = (int)$_GET['id'];
        if ($taskRepo->delete($taskId)) {
            header("Location: index.php?command=task_list&success=" . urlencode("Task deleted successfully!"));
        } else {
            header("Location: index.php?command=task_list&error=" . urlencode("Failed to delete task."));
        }
        exit();

    case 'employee_list':
        $employees = $employeeRepo->getAll();
        $successMessage = $_GET['success'] ?? null;
        $latte->render('employee-list.latte', [
            'title' => 'Employees',
            'pageId' => 'employee-list-page',
            'employees' => $employees,
            'successMessage' => $successMessage
        ]);
        break;

    case 'employee_form':
        $employee = isset($_GET['id']) ? $employeeRepo->findById((int)$_GET['id']) : new Employee();
        $title = $employee->id ? 'Edit Employee' : 'Add Employee';
        $pageId = 'employee-form-page';

        if (!$employee->id && isset($_GET['id'])) {
            $latte->render('employee-form.latte', [
                'title' => 'Edit Employee',
                'pageId' => $pageId,
                'employee' => new Employee(),
                'errors' => ['Employee not found.']
            ]);
            break;
        }

        $errors = isset($_GET['errors']) ? unserialize(urldecode($_GET['errors'])) : [];
        $fields = isset($_GET['fields']) ? array_map('urldecode', $_GET['fields']) : [];

        if ($fields) {
            $employee->firstName = $fields['firstName'] ?? $employee->firstName;
            $employee->lastName = $fields['lastName'] ?? $employee->lastName;
            $employee->picture = $fields['picture'] ?? $employee->picture;
        }

        $latte->render('employee-form.latte', [
            'title' => $title,
            'pageId' => $pageId,
            'employee' => $employee,
            'errors' => $errors
        ]);
        break;

    case 'task_list':
        $tasks = $taskRepo->getAll();
        $successMessage = $_GET['success'] ?? null;
        $latte->render('task-list.latte', [
            'title' => 'Task List',
            'pageId' => 'task-list-page',
            'tasks' => $tasks,
            'successMessage' => $successMessage
        ]);
        break;

    case 'task_form':
        $task = isset($_GET['id']) ? $taskRepo->findById((int)$_GET['id']) : new Task();
        $title = $task->id ? 'Edit Task' : 'Add Task';
        $pageId = 'task-form-page';

        if (!$task->id && isset($_GET['id'])) {
            header("Location: index.php?command=task_list&success=" . urlencode("Task not found."));
            exit();
        }

        $errors = isset($_GET['errors']) ? unserialize(urldecode($_GET['errors'])) : [];
        $fields = isset($_GET['fields']) ? array_map('urldecode', $_GET['fields']) : [];

        if ($fields) {
            $task->description = $fields['description'] ?? $task->description;
            $task->estimate = $fields['estimate'] ?? $task->estimate;
            $task->employeeId = $fields['employeeId'] ?? $task->employeeId;
            $task->isCompleted = $fields['isCompleted'] ?? $task->isCompleted;
        }

        $employees = $employeeRepo->getAll();

        $latte->render('task-form.latte', [
            'title' => $title,
            'pageId' => $pageId,
            'task' => $task,
            'employees' => $employees,
            'errors' => $errors
        ]);
        break;

    case 'dashboard':
    default:
        $employees = $employeeRepo->getAll();
        $tasks = $taskRepo->getAll();
        $successMessage = $_GET['success'] ?? null;
        $latte->render('dashboard.latte', [
            'title' => 'Dashboard',
            'pageId' => 'dashboard-page',
            'employees' => $employees,
            'tasks' => $tasks,
            'successMessage' => $successMessage
        ]);
        break;
}

function handleRequest(string $command)
{
    global $employeeRepo, $taskRepo;

    try {
        if ($command === 'handle_employee') {
            handleEmployeeRequest($employeeRepo);
        } elseif ($command === 'handle_task') {
            handleTaskRequest($taskRepo);
        }
    } catch (Exception $e) {
        error_log($e->getMessage());
        header("Location: index.php?command=dashboard&error=" . urlencode("An error occurred while processing your request."));
        exit();
    }
}

function handleEmployeeRequest($employeeRepo)
{
    $employeeId = isset($_POST['id']) ? (int)$_POST['id'] : null;

    if (isset($_POST['deleteButton']) && $_POST['deleteButton'] === 'delete_employee' && $employeeId) {
        $errors = $employeeRepo->delete($employeeId);
        if (empty($errors)) {
            header("Location: index.php?command=employee_list&success=" . urlencode("Employee deleted successfully!"));
        } else {
            header("Location: index.php?command=employee_form&id=" . $employeeId . "&error=" . urlencode("Failed to delete employee: " . implode(", ", $errors)));
        }
        exit();
    }

    $firstName = $_POST['firstName'] ?? '';
    $lastName = $_POST['lastName'] ?? '';
    $picture = 'default.jpg';

    $employee = new Employee($employeeId, $firstName, $lastName, $picture);

    if (isset($_FILES['picture']) && $_FILES['picture']['error'] === UPLOAD_ERR_OK) {
        $employee->picture = uploadPicture($_FILES['picture']);
    }

    $errors = $employeeId ? $employeeRepo->update($employee) : $employeeRepo->save($employee);

    if (!empty($errors)) {
        redirectToFormWithErrors('employee_form', $errors, [
            'firstName' => $firstName,
            'lastName' => $lastName,
            'picture' => $employee->picture
        ], $employeeId);
    } else {
        header("Location: index.php?command=employee_list&success=" . urlencode($employeeId ? "Employee updated successfully!" : "Employee added successfully!"));
        exit();
    }
}

function handleTaskRequest($taskRepo)
{
    $taskId = isset($_POST['id']) ? (int)$_POST['id'] : null;

    if (isset($_POST['deleteButton']) && $_POST['deleteButton'] === 'delete_task' && $taskId) {
        $errors = $taskRepo->delete($taskId);
        if (empty($errors)) {
            header("Location: index.php?command=task_list&success=" . urlencode("Task deleted successfully!"));
        } else {
            header("Location: index.php?command=task_form&id=" . $taskId . "&error=" . urlencode("Failed to delete task: " . implode(", ", $errors)));
        }
        exit();
    }

    $description = $_POST['description'] ?? '';
    $estimate = (int)($_POST['estimate'] ?? 0);
    $employeeId = getEmployeeId($_POST['employeeId'] ?? null);
    $isCompleted = isset($_POST['isCompleted']);

    $task = new Task($taskId, $description, $estimate, $employeeId, $isCompleted);

    $errors = $taskId ? $taskRepo->update($task) : $taskRepo->save($task);

    if (!empty($errors)) {
        redirectToFormWithErrors('task_form', $errors, [
            'description' => $description,
            'estimate' => $estimate,
            'employeeId' => $employeeId,
            'isCompleted' => $isCompleted
        ], $taskId);
    } else {
        header("Location: index.php?command=task_list&success=" . urlencode($taskId ? "Task updated successfully!" : "Task added successfully!"));
        exit();
    }
}

function getEmployeeId($employeeIdInput): ?int
{
    return ($employeeIdInput !== '' && $employeeIdInput !== 'Select an employee') ? (int)$employeeIdInput : null;
}

function uploadPicture(array $file): string
{
    $targetDir = "uploads/";
    if (!is_dir($targetDir)) {
        mkdir($targetDir, 0755, true);
    }

    $filename = basename($file["name"]);
    $uniqueFilename = uniqid() . "_" . preg_replace("/[^a-zA-Z0-9.]/", "_", $filename);
    $targetFile = $targetDir . $uniqueFilename;
    $imageFileType = strtolower(pathinfo($targetFile, PATHINFO_EXTENSION));

    $check = getimagesize($file["tmp_name"]);
    if ($check === false) {
        return 'default.jpg';
    }

    $allowedTypes = ['jpg', 'png', 'jpeg', 'gif'];
    if (!in_array($imageFileType, $allowedTypes)) {
        return 'default.jpg';
    }

    if (move_uploaded_file($file["tmp_name"], $targetFile)) {
        return $targetFile;
    } else {
        return 'default.jpg';
    }
}
