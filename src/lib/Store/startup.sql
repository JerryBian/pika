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
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    last_modified_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime'))
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
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    started_at TEXT COLLATE BINARY,
    completed_at TEXT COLLATE BINARY,
    FOREIGN KEY(task_id) REFERENCES task(id)
);

CREATE INDEX IF NOT EXISTS IX_task_run_status ON task(status ASC);
CREATE INDEX IF NOT EXISTS IX_task_run_created_at ON task(created_at DESC);
CREATE INDEX IF NOT EXISTS IX_task_run_completed_at ON task(completed_at DESC);

-- task_run_output
CREATE TABLE IF NOT EXISTS task_run_output
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    run_id INTEGER NOT NULL,
    message TEXT NOT NULL COLLATE BINARY,
    is_error INTEGER DEFAULT 0,
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    FOREIGN KEY(run_id) REFERENCES task_run(id)
);

CREATE INDEX IF NOT EXISTS IX_task_run_output_created_at ON task(created_at ASC);

-- setting
CREATE TABLE IF NOT EXISTS setting
(
    key TEXT NOT NULL PRIMARY KEY COLLATE BINARY,
    value TEXT COLLATE BINARY,
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    last_modified_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime'))
);