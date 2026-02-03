namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions
{
    public static class CollectionSyncExtensions
    {
        public static void SyncByKey<TExisting, TUpdated, TKey>(
            this IList<TExisting> existing,
            IEnumerable<TUpdated> updated,
            Func<TExisting, TKey> existingKey,
            Func<TUpdated, TKey> updatedKey,
            Action<TExisting, TUpdated> updateExisting,
            Func<TUpdated, TExisting> createNew)
        {
            var updatedLookup = updated.ToDictionary(updatedKey);

            // Remove items not present in updated
            for (var i = existing.Count - 1; i >= 0; i--)
            {
                var key = existingKey(existing[i]);
                if (!updatedLookup.ContainsKey(key))
                    existing.RemoveAt(i);
            }

            // Add or update
            foreach (var updatedItem in updated)
            {
                var key = updatedKey(updatedItem);
                var existingItem = existing.FirstOrDefault(e => existingKey(e).Equals(key));

                if (existingItem == null)
                {
                    existing.Add(createNew(updatedItem));
                }
                else
                {
                    updateExisting(existingItem, updatedItem);
                }
            }
        }
    }    
}
