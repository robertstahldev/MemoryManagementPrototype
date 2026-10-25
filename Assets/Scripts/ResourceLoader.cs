
using System.Collections;
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    private GameObject loadedPrefab;
    private GameObject spawnedObject;

    private Texture2D loadedTexture;
    private Material originalMaterial;
    private Material runtimeMaterial;

    private AudioClip loadedSound;
    private AudioSource audioSource;

    private bool isBusy;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private IEnumerator LoadModel()
    {
        if (isBusy || spawnedObject != null)
            yield break;

        isBusy = true;

        //Load the prefab when needed instead of keeping it in the scene.
        ResourceRequest request =
            Resources.LoadAsync<GameObject>("TestCube");

        yield return request;
        loadedPrefab = request.asset as GameObject;

        if (loadedPrefab != null)
        {
            spawnedObject = Instantiate(
                loadedPrefab,
                new Vector3(0f, 1f, 0f),
                Quaternion.identity
            );

            Renderer renderer = spawnedObject.GetComponent<Renderer>();

            if (renderer != null)
                originalMaterial = renderer.sharedMaterial;

            //Apply the texture if it was loaded before the model.
            ApplyTexture();

            Debug.Log("Model loaded.");
        }
        else
        {
            Debug.LogError("Could not load TestCube.");
        }

        isBusy = false;
    }

    private IEnumerator UnloadModel()
    {
        if (isBusy || spawnedObject == null)
            yield break;

        isBusy = true;

        //Remove the material instance created for the texture.
        RemoveRuntimeMaterial();

        Destroy(spawnedObject);
        spawnedObject = null;
        loadedPrefab = null;
        originalMaterial = null;

        yield return StartCoroutine(CleanupUnusedAssets());

        Debug.Log("Model unloaded.");
        isBusy = false;
    }

    private IEnumerator LoadTexture()
    {
        if (isBusy || loadedTexture != null)
            yield break;

        isBusy = true;

        ResourceRequest request =
            Resources.LoadAsync<Texture2D>("TestTexture");

        yield return request;
        loadedTexture = request.asset as Texture2D;

        if (loadedTexture != null)
        {
            ApplyTexture();
            Debug.Log("Texture loaded.");
        }
        else
        {
            Debug.LogError("Could not load TestTexture.");
        }

        isBusy = false;
    }

    private void ApplyTexture()
    {
        if (loadedTexture == null || spawnedObject == null)
            return;

        Renderer renderer = spawnedObject.GetComponent<Renderer>();

        if (renderer == null)
            return;

        if (runtimeMaterial == null)
        {
            //Use a separate material so the original prefab material isn't changed.
            runtimeMaterial = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = runtimeMaterial;
        }

        runtimeMaterial.mainTexture = loadedTexture;
    }

    private IEnumerator UnloadTexture()
    {
        if (isBusy || loadedTexture == null)
            yield break;

        isBusy = true;

        //Detach the texture before releasing its asset reference.
        RemoveRuntimeMaterial();
        loadedTexture = null;

        yield return StartCoroutine(CleanupUnusedAssets());

        Debug.Log("Texture unloaded.");
        isBusy = false;
    }

    private void RemoveRuntimeMaterial()
    {
        if (runtimeMaterial == null)
            return;

        if (spawnedObject != null)
        {
            Renderer renderer = spawnedObject.GetComponent<Renderer>();

            if (renderer != null)
                renderer.sharedMaterial = originalMaterial;
        }

        //Runtime-created materials must be destroyed separately.
        Destroy(runtimeMaterial);
        runtimeMaterial = null;
    }

    private IEnumerator LoadSound()
    {
        if (isBusy || loadedSound != null)
            yield break;

        isBusy = true;

        ResourceRequest request =
            Resources.LoadAsync<AudioClip>("TestSound");

        yield return request;
        loadedSound = request.asset as AudioClip;

        if (loadedSound != null)
        {
            audioSource.clip = loadedSound;
            Debug.Log("Sound loaded.");
        }
        else
        {
            Debug.LogError("Could not load TestSound.");
        }

        isBusy = false;
    }

    private void PlaySound()
    {
        if (isBusy || loadedSound == null)
            return;

        audioSource.Play();
    }

    private IEnumerator UnloadSound()
    {
        if (isBusy || loadedSound == null)
            yield break;

        isBusy = true;

        audioSource.Stop();

        //AudioSource must release its clip reference.
        audioSource.clip = null;
        loadedSound = null;

        yield return StartCoroutine(CleanupUnusedAssets());

        Debug.Log("Sound unloaded.");
        isBusy = false;
    }

    private IEnumerator CleanupUnusedAssets()
    {
        //Wait for Destroy operations to finish.
        yield return null;

        //Unload assets no longer being used.
        yield return Resources.UnloadUnusedAssets();
    }

    //Simple buttons for testing the resource loading system.
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 260, 320),
            "Resource Loading Test");

        GUI.Label(new Rect(20, 35, 230, 20),
            "Model: " + (spawnedObject != null ? "Loaded" : "Unloaded"));

        GUI.enabled = !isBusy && spawnedObject == null;

        if (GUI.Button(new Rect(20, 60, 110, 30), "Load Model"))
            StartCoroutine(LoadModel());

        GUI.enabled = !isBusy && spawnedObject != null;

        if (GUI.Button(new Rect(140, 60, 110, 30), "Unload Model"))
            StartCoroutine(UnloadModel());

        GUI.enabled = true;

        GUI.Label(new Rect(20, 105, 230, 20),
            "Texture: " + (loadedTexture != null ? "Loaded" : "Unloaded"));

        GUI.enabled = !isBusy && loadedTexture == null;

        if (GUI.Button(new Rect(20, 130, 110, 30), "Load Texture"))
            StartCoroutine(LoadTexture());

        GUI.enabled = !isBusy && loadedTexture != null;

        if (GUI.Button(new Rect(140, 130, 110, 30), "Unload Texture"))
            StartCoroutine(UnloadTexture());

        GUI.enabled = true;

        GUI.Label(new Rect(20, 175, 230, 20),
            "Sound: " + (loadedSound != null ? "Loaded" : "Unloaded"));

        GUI.enabled = !isBusy && loadedSound == null;

        if (GUI.Button(new Rect(20, 200, 110, 30), "Load Sound"))
            StartCoroutine(LoadSound());

        GUI.enabled = !isBusy && loadedSound != null;

        if (GUI.Button(new Rect(140, 200, 110, 30), "Unload Sound"))
            StartCoroutine(UnloadSound());

        GUI.enabled = !isBusy && loadedSound != null;

        if (GUI.Button(new Rect(20, 250, 230, 35), "Play Sound"))
            PlaySound();

        GUI.enabled = true;

        if (isBusy)
            GUI.Label(new Rect(20, 295, 230, 20),
                "Processing...");
    }
}
