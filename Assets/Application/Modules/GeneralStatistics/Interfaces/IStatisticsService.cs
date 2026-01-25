public interface IStatisticsService
{
    StatisticsData data { get; }
    StatisticsCatalogue catalogue { get; }
    void OnEvent(IEvent e);
    void Save();
    void Load();
    string GetStatisticsName(string statisticsId);
    string GetStatisticsFormattedValue(string statisticsId);
}