<?php
header("Content-Type: application/json");

// 1. DATABASE CREDENTIALS
$db_host = "localhost";
$db_user = "YOUR_DATABASE_USERNAME";
$db_pass = "YOUR_DATABASE_PASSWORD";
$db_name = "YOUR_DATABASE_NAME";

$conn = new mysqli($db_host, $db_user, $db_pass, $db_name);
if ($conn->connect_error) {
    http_response_code(500);
    exit;
}

// 2. READ THE URL PARAMETERS
$donor_key    = $_GET['key'] ?? '';
$machine_guid = $_GET['machine'] ?? '';

if (empty($donor_key) || empty($machine_guid)) {
    http_response_code(400); // Bad request
    echo json_encode(["status" => "error", "message" => "Missing arguments."]);
    exit;
}

// 3. LOOK UP THE ROW (Optimized for Machine Guid only during audits)
if ($donor_key === "audit") {
    $stmt = $conn->prepare("SELECT id FROM simtools_activations WHERE machine_guid = ?");
    $stmt->bind_param("s", $machine_guid);
} else {
    $stmt = $conn->prepare("SELECT id FROM simtools_activations WHERE donor_key = ? AND machine_guid = ?");
    $stmt->bind_param("ss", $donor_key, $machine_guid);
}
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows > 0) {
    // Machine is present and valid!
    http_response_code(200);
    echo json_encode(["status" => "active"]);
} else {
    // Machine was removed from the web dashboard
    http_response_code(403); // Forbidden
    echo json_encode(["status" => "revoked"]);
}

$stmt->close();
$conn->close();
?>