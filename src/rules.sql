CREATE RULE protect_column_parent_id AS
ON UPDATE TO position
WHERE OLD.parent_id IS DISTINCT FROM NEW.parent_id
DO raise exception 'Parent id cannot be changed directly, use specific function';
