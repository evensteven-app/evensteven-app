namespace EvenSteven.Infrastructure.Storage.Utils
{
    public class ExpenseUtils
    {
        public static Dictionary<Guid, long> SplitAmount(List<Guid> participantIds, long amount)
        {
            if (participantIds.Count == 0)
            {
                return [];
            }

            var sortedParticipantIds = participantIds.ToList();
            sortedParticipantIds.Sort();

            var baseShare = amount / sortedParticipantIds.Count;
            var reminder = amount % sortedParticipantIds.Count;

            var distributions = new Dictionary<Guid, long>();
            for(int i = 0; i < sortedParticipantIds.Count; i++)
            {
                var id = sortedParticipantIds[i];
                distributions.Add(id, baseShare + (i < reminder ? 1 : 0));
            }

            return distributions;
        }
    }
}
