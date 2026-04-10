ALTER TABLE customer ADD COLUMN IF NOT EXISTS password TEXT;
UPDATE customer SET password = '$2a$10$placeholder.hash.for.existing.rows.only' WHERE password IS NULL;
ALTER TABLE customer ALTER COLUMN password SET NOT NULL;