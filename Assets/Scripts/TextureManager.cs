using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    private const int gridSize = 8;

    private Texture2D sourceTexture;

    private readonly List<GameObject> cubes = new ();
    private readonly List<Material> materials = new ();
    private readonly List<Texture2D> textureCopies = new ();

    private string currentMode = "No textures";

    private void Start()
    {
        //Use one source texture for both tests.
        sourceTexture = Resources.Load<Texture2D>("TestTexture");

        if (sourceTexture == null)
        {
            Debug.LogError("TestTexture was not found in Resources.");
            return;
        }

        CreateTestCubes();
    }

    private void CreateTestCubes()
    {
        Shader shader = Shader.Find("Standard");

        if (shader == null)
        {
            Debug.LogError("Standard shader was not found.");
            return;
        }

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject cube = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                cube.name = "TextureCube";
                cube.transform.SetParent(transform);

                cube.transform.position = new Vector3(
                    (col - 4) * 2,
                    1,
                    (row * 2) + 3
                );

                cube.transform.localScale =
                    new Vector3(1, 1, 1);

                //Keep the same material objects for both tests.
                Material material = new(shader)
                {
                    mainTexture = null
                };

                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterial = material;

                cubes.Add(cube);
                materials.Add(material);
            }
        }

        Debug.Log(cubes.Count + " texture test cubes created.");
    }

    private void UseSharedTexture()
    {
        if (sourceTexture == null)
            return;

        ClearTextures();

        //All cubes reference the same loaded texture.
        foreach (Material material in materials)
        {
            material.mainTexture = sourceTexture;
        }

        currentMode = "Shared texture";

        Debug.Log("Shared texture test: 64 cubes, 1 texture.");
    }

    private void UseSeparateCopies()
    {
        if (sourceTexture == null)
            return;

        ClearTextures();

        //Create a separate texture allocation for each cube.
        foreach (Material material in materials)
        {
            Texture2D copy = Instantiate(sourceTexture);
            textureCopies.Add(copy);

            material.mainTexture = copy;
        }

        currentMode = "Separate copies";

        Debug.Log("Separate texture test: "
            + textureCopies.Count + " texture copies created.");
    }

    private void ClearTextures()
    {
        //Remove material references before destroying texture copies.
        foreach (Material material in materials)
        {
            material.mainTexture = null;
        }

        foreach (Texture2D texture in textureCopies)
        {
            Destroy(texture);
        }

        textureCopies.Clear();
        currentMode = "No textures";
    }

    private void OnDestroy()
    {
        //Clean up the objects created for this test.
        foreach (Texture2D texture in textureCopies)
        {
            if (texture != null)
                Destroy(texture);
        }

        foreach (Material material in materials)
        {
            if (material != null)
                Destroy(material);
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(290, 10, 260, 200),
            "Texture Management Test");

        GUI.Label(new Rect(300, 40, 240, 20),
            "Objects: " + cubes.Count);

        GUI.Label(new Rect(300, 65, 240, 20),
            "Mode: " + currentMode);

        GUI.enabled = sourceTexture != null && materials.Count > 0;

        if (GUI.Button(new Rect(300, 95, 240, 30),
            "Shared Texture"))
        {
            UseSharedTexture();
        }

        if (GUI.Button(new Rect(300, 130, 240, 30),
            "Separate Copies"))
        {
            UseSeparateCopies();
        }

        if (GUI.Button(new Rect(300, 165, 240, 30),
            "Clear Textures"))
        {
            ClearTextures();
        }

        GUI.enabled = true;
    }
}