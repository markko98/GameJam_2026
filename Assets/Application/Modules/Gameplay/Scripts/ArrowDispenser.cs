using UnityEngine;

public class ArrowDispenser : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform spawnPoint;
    public float interval = 2f;
    public float tileSize = 2f;
    public int maxTiles = 2;

    private float currentTime;
    private bool isPaused;
    private bool isStopped;

    private void Update()
    {
        if (isPaused || isStopped) return;

        currentTime += Time.deltaTime;

        if (currentTime >= interval)
        {
            currentTime -= interval;
            Shoot();
        }
    }

    private void Shoot()
    {
        var go = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
        var arrow = go.GetComponent<Arrow>();
        if (arrow == null)
            arrow = go.AddComponent<Arrow>();

        arrow.Initialize(spawnPoint.forward, tileSize, maxTiles);
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void Stop()
    {
        isStopped = true;
        currentTime = 0f;
    }

    public void Restart()
    {
        isStopped = false;
        isPaused = false;
        currentTime = 0f;
    }
}