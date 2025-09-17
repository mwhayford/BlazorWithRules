-- Database initialization script for BlazorWithRules
-- This script runs when the SQL Server container starts

USE master;
GO

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BlazorWithRules')
BEGIN
    CREATE DATABASE [BlazorWithRules];
    PRINT 'Database BlazorWithRules created successfully.';
END
ELSE
BEGIN
    PRINT 'Database BlazorWithRules already exists.';
END
GO

-- Switch to the application database
USE [BlazorWithRules];
GO

-- Create a dedicated user for the application (optional, for better security)
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'BlazorAppUser')
BEGIN
    CREATE LOGIN [BlazorAppUser] WITH PASSWORD = 'BlazorApp@Passw0rd123';
    PRINT 'Login BlazorAppUser created successfully.';
END
ELSE
BEGIN
    PRINT 'Login BlazorAppUser already exists.';
END
GO

-- Create user in the database
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'BlazorAppUser')
BEGIN
    CREATE USER [BlazorAppUser] FOR LOGIN [BlazorAppUser];
    PRINT 'User BlazorAppUser created in database.';
END
ELSE
BEGIN
    PRINT 'User BlazorAppUser already exists in database.';
END
GO

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER [BlazorAppUser];
ALTER ROLE db_datawriter ADD MEMBER [BlazorAppUser];
ALTER ROLE db_ddladmin ADD MEMBER [BlazorAppUser];
PRINT 'Permissions granted to BlazorAppUser.';
GO

-- Create some sample data (optional)
-- This will be handled by Entity Framework migrations, but we can add some initial data here if needed

PRINT 'Database initialization completed successfully.';
GO
