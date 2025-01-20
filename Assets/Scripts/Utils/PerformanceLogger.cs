using UnityEngine;

using System.Collections;
using System.IO;
using System;

public class PerformanceLogger : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private bool enableLogging = false;
    private string logPath;
    private float accumulatedFPS = 0;
    private float accumulatedPoolUsage = 0;
    private int sampleCount = 0;
    private const int SAMPLES_BEFORE_WRITE = 20;
    private CellPool cellPool;
    private StatManager statManager;
    #endregion

    #region Lifecycle Methods
    private void Start()
    {
        if (!enableLogging)
        {
            return;
        }

        cellPool = FindFirstObjectByType<CellPool>();
        statManager = StatManager.Instance;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logPath = Path.Combine(Application.dataPath, "Logs", $"performance_{timestamp}.txt");

        string directory = Path.GetDirectoryName(logPath);
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(logPath, "Time(s),AvgFPS,MinFPS,MaxFPS,PoolUsage%,ActiveCells,TotalPoolSize\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating file: {e.Message}");
        }

        StartCoroutine(LogStats());
    }
    #endregion

    #region Private Methods
    private IEnumerator LogStats()
    {
        if (!enableLogging)
        {
            yield break;
        }

        float startTime = Time.time;

        while (true)
        {
            accumulatedFPS += statManager.CurrentFPS;
            accumulatedPoolUsage += cellPool.PoolUsagePercent;
            sampleCount++;

            if (sampleCount >= SAMPLES_BEFORE_WRITE)
            {
                try
                {
                    float avgFPS = accumulatedFPS / sampleCount;
                    float avgPoolUsage = accumulatedPoolUsage / sampleCount;
                    float elapsedTime = Time.time - startTime;

                    string logLine = string.Format(
                        "{0:F1},{1:F1},{2:F1},{3:F1},{4:F1},{5},{6}\n",
                        elapsedTime,
                        avgFPS,
                        statManager.MinFPS,
                        statManager.MaxFPS,
                        avgPoolUsage,
                        cellPool.ActiveCount,
                        cellPool.TotalCount
                    );

                    File.AppendAllText(logPath, logLine);

                    accumulatedFPS = 0;
                    accumulatedPoolUsage = 0;
                    sampleCount = 0;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error writing to file: {e.Message}");
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    #endregion
}