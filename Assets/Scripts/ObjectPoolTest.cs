using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolTest : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;

    private const int objectCount = 100;
    private const int testCycles = 30;

    private readonly List<GameObject> pooledObjects = new ();
    private readonly List<GameObject> activeObjects = new ();

    private bool poolCreated;
    private bool isTesting;

    private double dynamicTime;
    private double pooledTime;

    private Vector3 GetSpawnPosition(int index)
    {
        int row = index / 10;
        int col = index % 10;

        return new Vector3(
            col-5,
            1,
            3 + row
        );
    }

    private void CreatePool()
    {
        if (poolCreated)
            return;

        //Allocate the pool before measuring runtime performance.
        for (int i = 0; i < objectCount; i++)
        {
            GameObject obj = Instantiate(objectPrefab, transform);
            obj.name = "PooledObject";
            obj.SetActive(false);

            pooledObjects.Add(obj);
        }

        poolCreated = true;
        Debug.Log("Object pool created: " + pooledObjects.Count);
    }

    private IEnumerator RunTest(bool usePooling)
    {
        if (isTesting || objectPrefab == null)
            yield break;

        isTesting = true;

        if (usePooling)
            CreatePool();

        //Measure object operations separately from the frames between cycles.
        double totalMilliseconds = 0;

        for (int cycle = 0; cycle < testCycles; cycle++)
        {
            long startTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            for (int i = 0; i < objectCount; i++)
            {
                GameObject obj;

                if (usePooling)
                {
                    obj = pooledObjects[i];
                    obj.transform.position = GetSpawnPosition(i);
                    obj.SetActive(true);
                }
                else
                {
                    //Create a new object every time instead of reusing one.
                    obj = Instantiate(
                        objectPrefab,
                        GetSpawnPosition(i),
                        Quaternion.identity,
                        transform
                    );
                }

                activeObjects.Add(obj);
            }

            long endTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            totalMilliseconds +=
                (endTicks - startTicks) * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;

            //Keep objects visible for a frame in both tests.
            yield return null;

            startTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            foreach (GameObject obj in activeObjects)
            {
                if (usePooling)
                {
                    //Return objects to the pool without destroying them.
                    obj.SetActive(false);
                }
                else
                {
                    Destroy(obj);
                }
            }

            activeObjects.Clear();

            endTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            totalMilliseconds +=
                (endTicks - startTicks) * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;

            //Allow scheduled destruction to finish before the next cycle.
            yield return null;
        }

        if (usePooling)
        {
            pooledTime = totalMilliseconds;
            Debug.Log("Pooled operations: " + pooledTime.ToString("F2") + " ms");
        }
        else
        {
            dynamicTime = totalMilliseconds;
            Debug.Log("Dynamic operations: " + dynamicTime.ToString("F2") + " ms");
        }

        isTesting = false;
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 350, 260, 205),
            "Object Pooling Test");

        GUI.Label(new Rect(20, 380, 230, 20),
            "100 objects x 30 cycles");

        GUI.enabled = !isTesting && objectPrefab != null;

        if (GUI.Button(new Rect(20, 410, 230, 30),
            "Run Dynamic Test"))
        {
            StartCoroutine(RunTest(false));
        }

        if (GUI.Button(new Rect(20, 445, 230, 30),
            "Run Pooling Test"))
        {
            StartCoroutine(RunTest(true));
        }

        GUI.enabled = true;

        GUI.Label(new Rect(20, 485, 230, 20),
            "Dynamic: " + dynamicTime.ToString("F2") + " ms");

        GUI.Label(new Rect(20, 510, 230, 20),
            "Pooled: " + pooledTime.ToString("F2") + " ms");

        if (isTesting)
            GUI.Label(new Rect(20, 535, 230, 20), "Running test...");
    }
}