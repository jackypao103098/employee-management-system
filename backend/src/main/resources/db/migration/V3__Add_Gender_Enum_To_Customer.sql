ALTER TABLE customer
ADD COLUMN gender TEXT;

UPDATE customer SET gender = 'MALE' WHERE gender IS NULL;

ALTER TABLE customer
ALTER COLUMN gender SET NOT NULL;