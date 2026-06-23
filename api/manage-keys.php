<?php
// 1. CHANGE THESE TO MATCH YOUR WEBSITE DATABASE DETAILS
$db_host = "localhost";$db_user = "YOUR_DATABASE_USERNAME";$db_pass = "YOUR_DATABASE_PASSWORD";$db_name = "YOUR_DATABASE_NAME";

// Connect to database
$conn = new mysqli($db_host, $db_user, $db_pass, $db_name);
if ($conn->connect_error) {
    die("Database connection failed: " . $conn->connect_error);
}

// Track login state or error messages
$searched_key = isset($_POST['donor_key']) ? trim($_POST['donor_key']) : (isset($_GET['key']) ? trim($_GET['key']) : '');
$message = "";

// 2. HANDLE REMOVE (DEACTIVATE) BUTTON CLICKS
if (isset($_POST['action']) && $_POST['action'] == 'deactivate') {
    $key_to_clean = trim($_POST['donor_key']);
    $guid_to_remove = trim($_POST['machine_guid']);
    
    if (!empty($key_to_clean) && !empty($guid_to_remove)) {
        $stmt = $conn->prepare("DELETE FROM simtools_activations WHERE donor_key = ? AND machine_guid = ?");
        $stmt->bind_param("ss", $key_to_clean, $guid_to_remove);
        if ($stmt->execute()) {
            $message = "<div class='alert alert-success'>Machine slot successfully freed up!</div>";
        } else {
            $message = "<div class='alert alert-danger'>Failed to clear the slot. Please try again.</div>";
        }
        $stmt->close();
    }
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SimTools — Donor Dashboard</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
    <style>
        body { background-color: #0f0f14; color: #e1e1e6; font-family: sans-serif; }
        .card { background-color: #16161f; border: 1px solid #232330; color: #ffffff; }
        .form-control { background-color: #232330; border: 1px solid #323245; color: #ffffff; }
        .form-control:focus { background-color: #2a2a3d; color: #ffffff; border-color: #80a093; box-shadow: none; }
        .btn-primary { background-color: #80a093; border-color: #80a093; }
        .btn-primary:hover { background-color: #6b8a7d; border-color: #6b8a7d; }
        .list-group-item { background-color: #1c1c27; border: 1px solid #2d2d3d; color: #e1e1e6; }
    </style>
</head>
<body>

<div class="container py-5">
    <div class="row justify-content-center">
        <div class="col-md-7">
            
            <div class="text-center mb-4">
                <h2>SimTools Personalization Portal</h2>
                <p class="text-light-50">Manage your active machine slots and license activations.</p>
            </div>

            <?php echo $message; ?>

            <div class="card p-4 mb-4 shadow-sm">
                <form method="POST" action="manage-keys.php">
                    <label for="donor_key" class="form-label fw-bold">Enter Your Donor Key</label>
                    <div class="input-group">
                        <input type="text" class="form-control" id="donor_key" name="donor_key" value="<?php echo htmlspecialchars($searched_key); ?>" placeholder="Paste your personalization key here..." required>
                        <button class="btn btn-primary" type="submit">Check Activations</button>
                    </div>
                </form>
            </div>

            <?php if (!empty($searched_key)): ?>
                <div class="card p-4 shadow-sm">
                    
                    <?php
                    // Fetch slots currently written in the DB table
                    $stmt = $conn->prepare("SELECT machine_guid, machine_name, activated_at FROM simtools_activations WHERE donor_key = ?");
                    $stmt->bind_param("s", $searched_key);
                    $stmt->execute();
                    $result = $stmt->get_result();

                    // Calculate usage numbers dynamically
                    $current_slots_used = $result->num_rows;
                    $max_slots_allowed = 5;
                    $slots_remaining = $max_slots_allowed - $current_slots_used;
                    ?>

                    <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-3">
                        <h5 class="m-0">Active Machine Registrations</h5>
                        <span class="badge bg-primary fs-6 py-2 px-3">
                            Slot Usage: <?php echo $current_slots_used; ?> / <?php echo $max_slots_allowed; ?>
                        </span>
                    </div>
                    
                    <?php
                    if ($current_slots_used == 0) {
                        echo "<p class='text-success m-0'>🎉 This key is wide open! No active machines are currently registered (0 out of 5 slots used).</p>";
                    } else {
                        echo "<p class='text-light small mb-3'>You have <strong>" . $slots_remaining . "</strong> machine slot(s) remaining before hitting your limit.</p>";
                        echo "<ul class='list-group'>";
                        
                        while ($row = $result->fetch_assoc()) {
                            $pcName = !empty($row['machine_name']) ? $row['machine_name'] : "Unknown Computer Setup";
                            $date = date("F j, Y", strtotime($row['activated_at']));
                            
                            echo "<li class='list-group-item d-flex justify-content-between align-items-center py-3'>";
                            echo "<div>";
                            echo "  <strong class='d-block text-white'>" . htmlspecialchars($pcName) . "</strong>";
                            echo "  <small class='text-muted'>Activated on " . $date . " </small>";
                            echo "</div>";
                            
                            // Form for the specific button to target this loop's row unique Machine GUID
                            echo "<form method='POST' action='manage-keys.php' onsubmit='return confirm(\"Are you sure you want to remove this computer slot?\");'>";
                            echo "  <input type='hidden' name='donor_key' value='" . htmlspecialchars($searched_key) . "'>";
                            echo "  <input type='hidden' name='machine_guid' value='" . htmlspecialchars($row['machine_guid']) . "'>";
                            echo "  <input type='hidden' name='action' value='deactivate'>";
                            echo "  <button type='submit' class='btn btn-outline-danger btn-sm'>Revoke Slot</button>";
                            echo "</form>";
                            echo "</li>";
                        }
                        
                        echo "</ul>";
                    }
                    $stmt->close();
                    ?>
                </div>
            <?php endif; ?>

        </div>
    </div>
</div>

</body>
</html>
<?php $conn->close(); ?>