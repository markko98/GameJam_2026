public abstract class StatisticsServiceBase : IStatisticsService
{
    public StatisticsData data { get; protected set; } = new();
    public StatisticsCatalogue catalogue { get; protected set; }

    protected StatisticsServiceBase(StatisticsCatalogue catalogue)
    {
        this.catalogue = catalogue;
        UEventBusAny.Register(OnEvent);
    }
    
    public void Cleanup()
    {
        UEventBusAny.Deregister(OnEvent);
    }

    ~StatisticsServiceBase()
    {
        Cleanup();
    }

    public abstract void OnEvent(IEvent e);

    public abstract void Save();
    public abstract void Load();
    public abstract string GetStatisticsName(string statisticsId);
    public abstract string GetStatisticsFormattedValue(string statisticsId);
}