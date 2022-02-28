PRAGMA journal_mode = 'wal';

-- task
CREATE TABLE IF NOT EXISTS task
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    name TEXT COLLATE BINARY,
    description TEXT COLLATE BINARY,
    script TEXT NOT NULL COLLATE BINARY,
    is_favorite INTEGER NOT NULL DEFAULT 0,
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    last_modified_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime'))
);

CREATE INDEX IF NOT EXISTS task_recent ON task(created_at DESC);

-- task_run
CREATE TABLE IF NOT EXISTS task_run
(
    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    task_id INTEGER NOT NULL,
    status INTEGER NOT NULL,
    script TEXT NOT NULL COLLATE BINARY,
    created_at TEXT COLLATE BINARY DEFAULT (DATETIME('now', 'localtime')),
    completed_at TEXT COLLATE BINARY,
    FOREIGN KEY(task_id) REFERENCES task(id)
);

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