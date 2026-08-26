using FluentMigrator;

namespace EvenSteven.Infrastructure.Storage.Migrations
{
    [Migration(2)]
    public class AddRoomTitle : Migration
    {
        public override void Down()
        {
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
        }
    }
}
