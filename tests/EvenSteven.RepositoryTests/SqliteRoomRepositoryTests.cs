using Dapper;
using EvenSteven.Shared.Models;

namespace EvenSteven.RepositoryTests
{
    public class SqliteRoomRepositoryTests(SqliteClassFixture fixture) : IAsyncLifetime, IClassFixture<SqliteClassFixture>
    {
        private List<Room> _mockRoom = [
                new(new Guid("00000000-0000-0000-0000-000000000001"), "Test room", new Guid("00000000-0000-0000-0000-000000000002"), "AABBCCDD", "TestPasswordHash", 1, DateTime.UtcNow),
                new(new Guid("00000000-0000-0000-0000-000000000003"), "Test room 2", new Guid("00000000-0000-0000-0000-000000000004"), "BBCCDDEE", "TestPasswordHash 2", 1, DateTime.UtcNow)
            ];

        public async ValueTask DisposeAsync()
        {
            await fixture.Database.ResetAsync();
        }

        public async ValueTask InitializeAsync()
        {
            await using var connection = await fixture.ConnectionFactory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

            string command = """
                             INSERT INTO Rooms (Id, Title, EditKey, InviteCode, PasswordHash, Version, CreatedAt)
                             VALUES (@Id, @Title, @EditKey, @InviteCode, @PasswordHash, @Version, @CreatedAt);
                             """;

            await connection.ExecuteAsync(command, _mockRoom);
        }

        [Fact]
        public async Task GetRoom_ExistingGuid_ReturnsRoom()
        {
            var room = await fixture.RoomRepository.GetRoomByIdAsync(_mockRoom[0].Id, TestContext.Current.CancellationToken);

            Assert.Equal(_mockRoom[0], room);
        }

        [Fact]
        public async Task GetRoom_NonExistingGuid_ReturnsNull()
        {
            var room = await fixture.RoomRepository.GetRoomByIdAsync(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), TestContext.Current.CancellationToken);

            Assert.Null(room);
        }

        [Fact]
        public async Task CreateRoom_ValidParameters_ReturnNewRoomId()
        {
            var newRoom = new Room(Guid.Empty, "New room", new Guid("00000000-0000-0000-0000-000000000005"), "AABBCCEE", "NewPasswordHash", 1, DateTime.UtcNow);
            var roomId = await fixture.RoomRepository.CreateRoomAsync(newRoom, TestContext.Current.CancellationToken);

            var room = await fixture.RoomRepository.GetRoomByIdAsync(roomId, TestContext.Current.CancellationToken);

            Assert.NotNull(room);
            Assert.Equal(newRoom with { Id = roomId, CreatedAt = room.CreatedAt }, room);
        }
    }
}
