DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'geometry_index') THEN
        DROP INDEX geometry_index;
    END IF;
END $$;

CREATE INDEX geometry_index
    ON geo_data
    USING GIST (geom);