-- DockSaaS Database Initialization Script
-- This script is automatically executed when PostgreSQL container starts

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Set timezone
SET timezone = 'UTC';

-- Create database if not exists (this is usually already done by POSTGRES_DB)
-- SELECT 'CREATE DATABASE docksaasdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'docksaasdb');

-- Connect to the database
\c docksaasdb;

-- Create any additional setup needed
-- Tables will be created by Entity Framework migrations

-- Add any seed data if needed
-- INSERT INTO ... if needed

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE docksaasdb TO postgres;

-- Output success message
SELECT 'DockSaaS database initialized successfully!' as message;