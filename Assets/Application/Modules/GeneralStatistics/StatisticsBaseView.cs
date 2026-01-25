using UnityEngine;

public abstract class StatisticsBaseView
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    protected string statisticsId;
    
    // runtime
    protected GameObject viewPrefab;
    protected StatisticsViewOutlet outlet;

    protected StatisticsBaseView(GameObject prefab, Transform parent, string statisticsId)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.statisticsId = statisticsId;

        Generate();
    }

    private void Generate()
    {
        viewPrefab = Object.Instantiate(prefab, parent);
        outlet = viewPrefab.GetComponent<StatisticsViewOutlet>();
        UpdateContent();
    }

    public abstract void UpdateContent();

    public virtual void Cleanup()
    {
        if (viewPrefab != null)
        {
            Object.Destroy(viewPrefab.gameObject);
        }
    }
    
}
