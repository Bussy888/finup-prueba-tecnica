namespace Claims.Domain;

public static class BusinessDayCalculator
{
    public static DateTime AddBusinessDays(DateTime start, int businessDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(businessDays);

        var current = start;
        var remaining = businessDays;
        while (remaining > 0)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current))
            {
                remaining--;
            }
        }

        return current;
    }

    public static bool IsBusinessDay(DateTime date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
}
