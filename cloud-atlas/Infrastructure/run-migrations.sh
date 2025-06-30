#!/bin/sh
echo "Migrations runner - waiting for DB to be ready"
sleep 20

echo "Migrations runner - creating database..."
# Create the database if it doesn't exist
/opt/mssql-tools/bin/sqlcmd -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -Q "IF DB_ID('cloud-atlas') IS NULL CREATE DATABASE [cloud-atlas];"

echo "Migrations runner - running migrations..."
# Run the migration script
/opt/mssql-tools/bin/sqlcmd -S "$SQL_HOST" -U "$SQL_USER" -P "$SQL_PASSWORD" -d cloud-atlas -i ./migrations/migrations.sql
