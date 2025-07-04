namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class ListExtensions
{
    public static bool AreSame<T,T2>(this List<T>? originalList, List<T2>? compareList) where T : IDomainEntity<T2>
    {
        if (originalList == null && compareList == null)
            return true;

        if (originalList == null || compareList == null)
            return false;

        if (originalList.Count != compareList.Count)
            return false;

        var comparePool = new List<T2>(compareList);// Create a copy of the comparison list so we can remove matched items

        foreach (var original in originalList)
        {
            bool matchFound = false;

            for (int i = 0; i < comparePool.Count; i++)
            {
                if (original.AreSame(comparePool[i]))
                {
                    comparePool.RemoveAt(i); // Prevent re-matching
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
                return false;
        }


        return true;
    }

    public static bool AreSame<T,T2>(this IReadOnlyCollection<T>? originalList, List<T2>? compareList) where T : IDomainEntity<T2>
    {
        if (originalList != null)
            return originalList.ToList().AreSame(compareList);

        if (compareList == null) // both lists are null
            return true;

        return false;
    }

    /// <summary>
    /// Converts a list of domain entities to a list of EF data access models, applying an optional modification action to each model.
    /// </summary>
    public static List<TModel> ToModels<TDomain, TModel>(this List<TDomain> list, Action<TModel>? modify = null) where TDomain : IDomainEntity<TModel>
    {
        return list
                .Select(x =>
                {
                    var model = x.GetModel();
                    modify?.Invoke(model);
                    return model;
                })
                .ToList();
    }

}