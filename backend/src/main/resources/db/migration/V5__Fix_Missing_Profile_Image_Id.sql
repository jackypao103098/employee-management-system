ALTER TABLE employee ADD COLUMN IF NOT EXISTS profile_image_id VARCHAR(36);
ALTER TABLE employee DROP CONSTRAINT IF EXISTS profile_image_id_unique;
ALTER TABLE employee ADD CONSTRAINT profile_image_id_unique UNIQUE (profile_image_id);
