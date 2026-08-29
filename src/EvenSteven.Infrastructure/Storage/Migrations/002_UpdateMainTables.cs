using FluentMigrator;

namespace EvenSteven.Infrastructure.Storage.Migrations
{
    [Migration(2)]
    public class UpdateMainTables : Migration
    {
        public override void Down()
        {
            Delete.Column("InviteCode")
                .FromTable("Rooms");

            Delete.Column("PayerId")
                .FromTable("Expenses");

            Rename.Column("ExpenseId")
                .OnTable("ExpenseEntries")
                .To("EventId");

            Rename.Table("ExpenseEntries")
                .To("EventEntries");

            Rename.Table("Expenses")
                .To("Events");

            Delete.Column("Title")
                .FromTable("Rooms");
        }

        public override void Up()
        {
            Alter.Table("Rooms")
                .AddColumn("Title").AsString().NotNullable();

            Rename.Table("Events")
                .To("Expenses");

            Rename.Table("EventEntries")
                .To("ExpenseEntries");

            Rename.Column("EventId")
                .OnTable("ExpenseEntries")
                .To("ExpenseId");

            Create.Column("PayerId")
                .OnTable("Expenses")
                .AsGuid()
                .NotNullable();

            Create.Column("InviteCode")
                .OnTable("Rooms")
                .AsString()
                .NotNullable();

            Create.UniqueConstraint("UC_Rooms_InviteCode")
                .OnTable("Rooms")
                .Column("InviteCode");
        }
    }
}
