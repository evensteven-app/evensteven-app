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

            participantIds.Sort();
            var baseShare = amount / participantIds.Count;
            var reminder = amount % participantIds.Count;

            var distributions = new Dictionary<Guid, long>();
            for(int i = 0; i < participantIds.Count; i++)
            {
                var id = participantIds[i];
                distributions.Add(id, baseShare + (i < reminder ? 1 : 0));
            }

            return distributions;
        }
    }
}
