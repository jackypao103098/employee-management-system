ALTER TABLE customer ADD COLUMN IF NOT EXISTS profile_image_id VARCHAR(36);
ALTER TABLE customer DROP CONSTRAINT IF EXISTS profile_image_id_unique;
ALTER TABLE customer ADD CONSTRAINT profile_image_id_unique UNIQUE (profile_image_id);
