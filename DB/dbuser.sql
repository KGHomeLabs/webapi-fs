-- Make sure that you logon as an entra user with the necessary permissions to create users and roles.
-- Create database user mapped to the App Service's Managed Identity
BEGIN TRY
    BEGIN TRANSACTION;
    -- Create user (not rollback-safe)
    CREATE USER [homelabuser-web-app] FROM EXTERNAL PROVIDER;
    -- These are rollback-safe
    ALTER ROLE db_datareader ADD MEMBER [homelabuser-web-app];
    ALTER ROLE db_datawriter ADD MEMBER [homelabuser-web-app];
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH