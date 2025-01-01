using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Data;

namespace Nop.Plugin.HerGunEnUcuz.ProductSearch.Data
{
    [NopMigration("2025-01-01 12:00:00", "HerGunEnUcuz.ProductSearch ProductSearchMigration", MigrationProcessType.Update)]
    public class ProductSearchMigration : MigrationBase
    {
        public override void Up()
        {
            // Check if database is installed and if the database is MySQL
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            var dataSettings = DataSettingsManager.LoadSettings();
            if (dataSettings.DataProvider != DataProviderType.MySql)
                return;

            // FluentMigrator way to execute SQL command
            
            //Execute.Sql("ALTER TABLE product CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci;");
        }

        public override void Down()
        {
            // Add logic here to revert the above changes if necessary.
            // Example:
            // Execute.Sql("ALTER TABLE product CONVERT TO CHARACTER SET utf8 COLLATE utf8_general_ci;");
        }
    }
}
