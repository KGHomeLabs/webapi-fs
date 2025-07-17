using FluentMigrator;


namespace WebApi.Migrations.Migrations
{     
      /// <summary>
      /// Migration to create the Users table with initial test data.
      /// </summary>
      /// <remarks>
      /// This migration creates the Users table with columns for UserId, IsAdmin, IsRoot, UserName, and IsLockedOut.
      /// It also inserts some initial test data into the Users table.
      /// </remarks>

    [Migration(20250714001)]
    public class CreateUsersTable : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("UserId").AsString(255).NotNullable().WithDefaultValue("")
                .WithColumn("IsAdmin").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsRoot").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("UserName").AsString(255).NotNullable().WithDefaultValue("")
                .WithColumn("IsLockedOut").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}