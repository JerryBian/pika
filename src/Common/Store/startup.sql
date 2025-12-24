PRAGMA journal_mode = 'wal';

-- script
CREATE TABLE IF NOT EXISTS script
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT COLLATE BINARY,
    description TEXT COLLATE BINARY,
    script TEXT NOT NULL COLLATE BINARY,
    shell_name TEXT NOT NULL COLLATE BINARY,
    shell_option TEXT COLLATE BINARY,
    shell_ext TEXT NOT NULL COLLATE BINARY,
    is_temp INTEGER DEFAULT 0,
    created_at INTEGER NOT NULL,
    last_modified_at INTEGER NOT NULL
);


-- script_run
CREATE TABLE IF NOT EXISTS script_run
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    script_id INTEGER NOT NULL,
    status INTEGER NOT NULL,
    script TEXT NOT NULL COLLATE BINARY,
    shell_name TEXT NOT NULL COLLATE BINARY,
    shell_option TEXT COLLATE BINARY,
    shell_ext TEXT NOT NULL COLLATE BINARY,
    created_at INTEGER NOT NULL,
    started_at INTEGER NOT NULL,
    completed_at INTEGER NOT NULL,
    FOREIGN KEY(script_id) REFERENCES script(id)
);

-- script_run_output
CREATE TABLE IF NOT EXISTS script_run_output
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    message TEXT NOT NULL COLLATE BINARY,
    is_error INTEGER DEFAULT 0,
    created_at INTEGER NOT NULL,
    FOREIGN KEY(run_id) REFERENCES script_run(id)
);

--CREATE INDEX IF NOT EXISTS IX_task_run_output_created_at ON task_run_output(created_at ASC);

-- task_run_output
CREATE TABLE IF NOT EXISTS app
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL COLLATE BINARY,
    description TEXT COLLATE BINARY,
    init_script TEXT COLLATE BINARY,
    init_script_path TEXT COLLATE BINARY,
    start_script TEXT COLLATE BINARY,
    start_script_path TEXT COLLATE BINARY,
    stop_script TEXT COLLATE BINARY,
    stop_script_path TEXT COLLATE BINARY,
    state_script TEXT COLLATE BINARY,
    state_script_path TEXT COLLATE BINARY,
    running_state TEXT COLLATE BINARY,
    port INTEGER DEFAULT 80,
    shell_name TEXT NOT NULL COLLATE BINARY,
    shell_option TEXT COLLATE BINARY,
    shell_ext TEXT NOT NULL COLLATE BINARY,
    created_at INTEGER NOT NULL,
    last_modified_at INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_app_created_at ON app(created_at ASC);

CREATE TABLE IF NOT EXISTS drive
(
    id TEXT NOT NULL COLLATE BINARY PRIMARY KEY,
    name TEXT NOT NULL COLLATE BINARY,
    path TEXT NOT NULL COLLATE BINARY,
    size TEXT NOT NULL COLLATE BINARY,
    type TEXT NOT NULL COLLATE BINARY,
    model TEXT NOT NULL COLLATE BINARY,
    serial TEXT NOT NULL COLLATE BINARY,
    tran TEXT NOT NULL COLLATE BINARY,
    created_at INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS drive_partition
(
    drive_id TEXT NOT NULL COLLATE BINARY,
    name TEXT NOT NULL COLLATE BINARY,
    path TEXT COLLATE BINARY,
    size TEXT COLLATE BINARY,
    type TEXT COLLATE BINARY,
    mount_point TEXT COLLATE BINARY,
    label TEXT COLLATE BINARY,
    created_at INTEGER NOT NULL,
    uuid TEXT COLLATE BINARY
);

CREATE TABLE IF NOT EXISTS drive_smartctl
(
    drive_id TEXT NOT NULL COLLATE BINARY,
    passed INTEGER NOT NULL,
    reallocated_sector_ct INTEGER,
    current_pending_sector INTEGER,
    offline_uncorrectable INTEGER,
    reported_uncorrect INTEGER,
    start_stop_count INTEGER,
    power_on_hours INTEGER,
    created_at INTEGER NOT NULL,
    power_cycle_count INTEGER,
    command_timeout INTEGER,
    power_off_restart_count INTEGER,
    load_cycle_count INTEGER,
    udma_crc_error_count INTEGER
);

-- setting
CREATE TABLE IF NOT EXISTS setting
(
    key TEXT NOT NULL PRIMARY KEY COLLATE BINARY,
    value TEXT COLLATE BINARY,
    last_modified_at INTEGER NOT NULL
);