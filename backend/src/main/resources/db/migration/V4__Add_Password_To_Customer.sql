ALTER TABLE employee ADD COLUMN IF NOT EXISTS password TEXT;
UPDATE employee SET password = '$2a$10$placeholder.hash.for.existing.rows.only' WHERE password IS NULL;
ALTER TABLE employee ALTER COLUMN password SET NOT NULL;
