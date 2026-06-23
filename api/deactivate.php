<?php
header("Content-Type: application/json");

// 1. DATABASE CREDENTIALS
$db_host = "localhost";$db_user = "YOUR_DATABASE_USERNAME";$db_pass = "YOUR_DATABASE_PASSWORD";$db_name = "YOUR_DATABASE_NAME";

$conn = new mysqli($db_host, $db_user, $db_pass, $db_name);
if ($conn->connect_error) {
    http_response_code(500);
    exit;
}

// 2. READ THE INCOMING DEACTIVATE ENVELOPE
$inputData = file_get_contents("php://input");
$request = json_decode($inputData, true);

$machine_guid = $request['machine_guid'] ?? '';

if (empty($machine_guid)) {
    http_response_code(400);
    echo json_encode(["error" => "Missing machine identification."]);
    exit;
}

// 3. DELETE THE REGISTRATION FOR THIS MACHINE
$stmt = $conn->prepare("DELETE FROM simtools_activations WHERE machine_guid = ?");
$stmt->bind_param("s", $machine_guid);

if ($stmt->execute()) {
    http_response_code(200);
    echo json_encode(["status" => "success", "message" => "Machine removed successfully."]);
} else {
    http_response_code(500);
    echo json_encode(["error" => "Failed to delete machine tracking slot."]);
}

$stmt->close();
$conn->close();
?>