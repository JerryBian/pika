PRAGMA journal_mode = 'wal';

-- task
CREATE TABLE IF NOT EXISTS task
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

CREATE INDEX IF NOT EXISTS IX_task_is_temp ON task(is_temp ASC);
CREATE INDEX IF NOT EXISTS IX_task_created_at ON task(created_at ASC);

-- task_run
CREATE TABLE IF NOT EXISTS task_run
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    task_id INTEGER NOT NULL,
    status INTEGER NOT NULL,
    script TEXT NOT NULL COLLATE BINARY,
    shell_name TEXT NOT NULL COLLATE BINARY,
    shell_option TEXT COLLATE BINARY,
    shell_ext TEXT NOT NULL COLLATE BINARY,
    created_at INTEGER NOT NULL,
    started_at INTEGER NOT NULL,
    completed_at INTEGER NOT NULL,
    FOREIGN KEY(task_id) REFERENCES task(id)
);

CREATE INDEX IF NOT EXISTS IX_task_run_status ON task_run(status ASC);
CREATE INDEX IF NOT EXISTS IX_task_run_created_at ON task_run(created_at DESC);
CREATE INDEX IF NOT EXISTS IX_task_run_completed_at ON task_run(completed_at DESC);

-- task_run_output
CREATE TABLE IF NOT EXISTS task_run_output
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    message TEXT NOT NULL COLLATE BINARY,
    is_error INTEGER DEFAULT 0,
    created_at INTEGER NOT NULL,
    FOREIGN KEY(run_id) REFERENCES task_run(id)
);

CREATE INDEX IF NOT EXISTS IX_task_run_output_created_at ON task_run_output(created_at ASC);

-- task_run_output
CREATE TABLE IF NOT EXISTS app
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL COLLATE BINARY,
    description TEXT COLLATE BINARY,
    start_script TEXT COLLATE BINARY,
    start_script_path TEXT COLLATE BINARY,
    stop_script TEXT COLLATE BINARY,
    stop_script_path TEXT COLLATE BINARY,
    shell_name TEXT NOT NULL COLLATE BINARY,
    shell_option TEXT COLLATE BINARY,
    shell_ext TEXT NOT NULL COLLATE BINARY,
    created_at INTEGER NOT NULL,
    last_modified_at INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_app_created_at ON app(created_at ASC);

-- setting
CREATE TABLE IF NOT EXISTS setting
(
    key TEXT NOT NULL PRIMARY KEY COLLATE BINARY,
    value TEXT COLLATE BINARY,
    last_modified_at INTEGER NOT NULL
);