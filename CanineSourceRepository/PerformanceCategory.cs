namespace CanineSourceRepository;

public enum PerformanceCategory
{
  WorldClass,
  Excellent,
  Good,
  Average,
  BelowAverage,
  Bad
}

public static class PerformanceColors
{
  public static string GetColor(this PerformanceCategory category)
  {
    return category switch
    {
      PerformanceCategory.WorldClass => "#B2E2B1",   // Pastel Green
      PerformanceCategory.Excellent => "#D9E2B2",    // Pastel Light Green
      PerformanceCategory.Good => "#F2E2B2",         // Pastel Yellow
      PerformanceCategory.Average => "#F9C5B2",       // Pastel Peach
      PerformanceCategory.BelowAverage => "#F1B2B2",  // Pastel Red
      PerformanceCategory.Bad => "#E8668B",           // Pastel Pink
      _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };
  }
}