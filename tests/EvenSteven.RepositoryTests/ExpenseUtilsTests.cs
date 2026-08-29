using EvenSteven.Infrastructure.Storage.Utils;

namespace EvenSteven.RepositoryTests
{
    public class ExpenseUtilsTests
    {
        [Theory]
        [InlineData(1, 1000, new long[] { 1000 })]
        [InlineData(2, 1000, new long[] { 500, 500 })]
        [InlineData(3, 1000, new long[] { 334, 333, 333 })]
        [InlineData(7, 5000, new long[] { 715, 715, 714, 714, 714, 714, 714 })]
        public async Task SplitAmount_ReturnValidShareByParticipant(int participantsNumber, long amount, long[] expectedValues)
        {
            List<Guid> participantIds = [];
            for (int i = 0; i < participantsNumber; i++)
            {
                participantIds.Add(Guid.NewGuid());
            }

            var distribution = ExpenseUtils.SplitAmount(participantIds, amount);

            Assert.Equal(expectedValues, distribution.Select(d => d.Value).ToArray());
        }

        [Fact]
        public void SplitAmount_NoParticipants_ReturnEmptyDictionary()
        {
            var distribution = ExpenseUtils.SplitAmount([], 1000);

            Assert.Empty(distribution);
        }
    }
}
