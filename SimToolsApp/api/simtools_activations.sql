CREATE TABLE IF NOT EXISTS simtools_activations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    donor_key VARCHAR(255) NOT NULL,
    machine_guid VARCHAR(255) NOT NULL,
    machine_name VARCHAR(100),
    email VARCHAR(255) NOT NULL,
    activated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_activation (donor_key, machine_guid)
);