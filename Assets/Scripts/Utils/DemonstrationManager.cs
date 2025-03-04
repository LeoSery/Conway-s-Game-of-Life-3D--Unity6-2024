using System.Collections.Generic;
using System.Collections;

using Unity.Mathematics;

using UnityEngine;

public class DemonstrationManager : MonoBehaviour
{
    #region Fields
    [Header("Settings")]
    public bool isActive = true;

    [Header("Camera Actions")]
    [Header("Sequence 1")]
    public Transform cameraStartPointSeq1;
    [Header("Sequence 2-4")]
    public Transform cameraStartPointSeq2to4;
    public float orthographicSize = 5f;
    [Header("Sequence 5")]
    public Transform cameraStartPointSeq5;
    public Transform cameraEndPointSeq5;

    [Header("Color Actions")]
    public Material NextAliveMaterial;
    public Material NextDeadMaterial;

    private Camera mainCamera;
    private bool isSequenceRunning = false;
    #endregion

    #region UnityLifecycle
    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("SequenceManager started");
    }

    void Update()
    {
        if (!isActive)
        {
            return;
        }

        if (!isSequenceRunning)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(Sequence1());
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(Sequence2());
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartCoroutine(Sequence3());
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartCoroutine(Sequence4());
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartCoroutine(Sequence5());
            }
        }
    }
    #endregion

    #region Sequences
    /// <summary>
    /// Sequence 1: grid of 1x1x1 single cell in the center of the grid, showing living/dead state
    /// </summary>
    IEnumerator Sequence1()
    {
        isSequenceRunning = true;
        Debug.Log("Sequence 1 started");

        GameManager.Instance.ResetGrid();
        GameManager.Instance.ResizeGrid(1);

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;

        mainCamera.transform.SetPositionAndRotation(cameraStartPointSeq1.position, cameraStartPointSeq1.rotation);

        yield return new WaitForSecondsRealtime(2.0f);

        int3[] cellPosition = new int3[] { new(0, 0, 0) };
        yield return StartCoroutine(CellAction(cellPosition, true, 1.0f));

        isSequenceRunning = false;
        Debug.Log("Sequence 1 completed");
    }

    /// <summary>
    /// Sequence 2: 3x3x3 grid single cell in the center of the grid, showing that the cell can have neighbors
    /// </summary>
    IEnumerator Sequence2()
    {
        isSequenceRunning = true;
        Debug.Log("Sequence 2 started");

        GameManager.Instance.ResetGrid();
        GameManager.Instance.ResizeGrid(3);

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;
        mainCamera.transform.SetPositionAndRotation(cameraStartPointSeq2to4.position, cameraStartPointSeq2to4.rotation);

        int3[] centerPosition = new int3[] { new(1, 1, 1) };
        yield return StartCoroutine(CellAction(centerPosition, true, 0.5f));

        isSequenceRunning = false;
        Debug.Log("Sequence 2 completed");
    }

    /// <summary>
    /// Sequence 3: grid from 3x3x3 to 11x1x11 single cell in the center of the grid, we'll show that it's cyclic (alternating 2 states that could really happen). 
    /// </summary>
    IEnumerator Sequence3()
    {
        isSequenceRunning = true;
        Debug.Log("Sequence 3 started");

        GameManager.Instance.ResetGrid();
        Coroutine Seq3GridResizeCoroutine = StartCoroutine(ResizeGridAction(11, 2.0f));

        mainCamera.orthographic = true;
        Coroutine Seq3OrthographiqueCoroutine = StartCoroutine(OrthographicSizeAction(mainCamera.orthographicSize, 10f, 2f));

        yield return Seq3GridResizeCoroutine;
        yield return Seq3OrthographiqueCoroutine;

        mainCamera.transform.SetPositionAndRotation(cameraStartPointSeq2to4.position, cameraStartPointSeq2to4.rotation);

        yield return new WaitForSecondsRealtime(1f);

        int center = GameManager.Instance.gridSize / 2;

        int3[] pattern1 = GenerateBoxPattern(center, center, 5, true);
        yield return StartCoroutine(CellAction(pattern1, true));
        yield return new WaitForSecondsRealtime(2f);

        GameManager.Instance.ResetGrid();
        int3[] pattern2 = GenerateBoxWithRoundedCorners(center, center, 7);
        yield return StartCoroutine(CellAction(pattern2, true));
        yield return new WaitForSecondsRealtime(2f);

        GameManager.Instance.ResetGrid();
        int3[] pattern3 = GenerateCrossWithBlocks(center, center);
        yield return StartCoroutine(CellAction(pattern3, true));
        yield return new WaitForSecondsRealtime(2f);

        GameManager.Instance.ResetGrid();
        int3[] pattern4 = GenerateCheckerboardPattern(center, center);
        yield return StartCoroutine(CellAction(pattern4, true));
        yield return new WaitForSecondsRealtime(2f);

        isSequenceRunning = false;
        Debug.Log("Sequence 3 completed");
    }

    /// <summary>
    /// Sequence 4: 11x11x11 grid, we show the rules of the game by linking several paterns and coloring the cells.
    /// </summary>
    IEnumerator Sequence4()
    {
        isSequenceRunning = true;
        Debug.Log("Sequence 4 started");

        GameManager.Instance.ResetGrid();
        yield return StartCoroutine(ResizeGridAction(11, 0.1f));

        mainCamera.orthographic = true;
        yield return StartCoroutine(OrthographicSizeAction(mainCamera.orthographicSize, 10f, 2f));
        mainCamera.transform.SetPositionAndRotation(cameraStartPointSeq2to4.position, cameraStartPointSeq2to4.rotation);

        yield return new WaitForSecondsRealtime(1f);

        int center = GameManager.Instance.gridSize / 2;

        // Rule 1 > 3 Cells alive create a new cell
        int3[] cell1Position = new int3[] { new(center - 1, center, center) };
        int3[] cell2Position = new int3[] { new(center, center, center) };
        int3[] cell3Position = new int3[] { new(center + 1, center, center) };
        int3[] cell4Position = new int3[] { new(center, center, center + 1) };

        yield return StartCoroutine(CellAction(cell1Position, true));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(cell2Position, true));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(cell3Position, true));
        yield return new WaitForSecondsRealtime(3f);

        yield return StartCoroutine(CellAction(cell4Position, true));
        yield return StartCoroutine(ColorCellAction(cell4Position[0], NextAliveMaterial));
        yield return new WaitForSecondsRealtime(5f);

        GameManager.Instance.ResetGrid();
        yield return new WaitForSecondsRealtime(3f);

        // Rule 2 > 3 Cells alive keep the cell alive
        int3[] newCell1Position = new int3[] { new(center, center, center + 1) };
        int3[] newCell2Position = new int3[] { new(center, center, center) };
        int3[] newCell3Position = new int3[] { new(center + 1, center, center) };
        int3[] newCell4Position = new int3[] { new(center, center, center - 1) };

        yield return StartCoroutine(CellAction(newCell1Position, true));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(newCell2Position, true));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(newCell3Position, true));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(newCell4Position, true));
        yield return new WaitForSecondsRealtime(3f);
        
        yield return StartCoroutine(ColorCellAction(newCell3Position[0], NextAliveMaterial));
        yield return new WaitForSecondsRealtime(3f);

        int3[] Cell5Position = new int3[] { new(center + 1, center, center + 1) };
        yield return StartCoroutine(CellAction(Cell5Position, true));

        yield return new WaitForSecondsRealtime(3f);

        yield return StartCoroutine(ColorCellAction(newCell3Position[0], NextDeadMaterial));

        yield return new WaitForSecondsRealtime(5f);

        Debug.Log("Suppression des cellules 5, 1 et 2");
        yield return StartCoroutine(CellAction(Cell5Position, false));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(newCell1Position, false));
        yield return new WaitForSecondsRealtime(0.3f);

        yield return StartCoroutine(CellAction(newCell2Position, false));
        yield return new WaitForSecondsRealtime(3f);

        yield return StartCoroutine(CellAction(newCell4Position, false));
        yield return StartCoroutine(CellAction(newCell3Position, false));

        isSequenceRunning = false;
        Debug.Log("Sequence 4 completed");
    }

    /// <summary>
    /// Sequence 5: 11x1x11 grid, single cell in the center of the grid, change from orthographic to perspective view and move the camera.
    /// </summary>
    IEnumerator Sequence5()
    {
        isSequenceRunning = true;
        Debug.Log("Sequence 5 started");

        yield return StartCoroutine(OrthographicSizeAction(mainCamera.orthographicSize, 12f, 2f));

        GameManager.Instance.ResetGrid();
        yield return StartCoroutine(ResizeGridAction(11, 0.1f));

        int center = GameManager.Instance.gridSize / 2;
        int3[] centerPosition = new int3[] { new(center, center, center) };
        yield return StartCoroutine(CellAction(centerPosition, true, 0.5f));

        yield return new WaitForSecondsRealtime(2.0f);

        mainCamera.orthographic = false;

        Coroutine cameraCoroutine = StartCoroutine(CameraTravelingAction(cameraStartPointSeq5, cameraEndPointSeq5, 5f));
        Coroutine layerCoroutine = StartCoroutine(LayerAction(10, 0, 5.0f));

        yield return cameraCoroutine;
        yield return layerCoroutine;

        isSequenceRunning = false;
        Debug.Log("Sequence 5 completed");
    }
    #endregion

    #region Actions
    /// <summary>
    /// Moves the camera from one point to another
    /// </summary>
    IEnumerator CameraTravelingAction(Transform start, Transform end, float duration)
    {
        Debug.Log($"CameraSequence: Démarrage de {start.name} vers {end.name} en {duration}s");

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            mainCamera.transform.position = Vector3.Lerp(start.position, end.position, t);
            mainCamera.transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);

            yield return null;
        }

        mainCamera.transform.position = end.position;
        mainCamera.transform.rotation = end.rotation;

        Debug.Log("CameraSequence: Terminé");
    }

    /// <summary>
    /// Gradually changes the camera's orthographicSize value
    /// </summary>
    /// <param name="fromSize">Taille orthographique de départ</param>
    /// <param name="toSize">Taille orthographique d'arrivée</param>
    /// <param name="duration">Durée totale de la transition en secondes</param>
    IEnumerator OrthographicSizeAction(float fromSize, float toSize, float duration)
    {
        Debug.Log($"OrthographicSizeAction: Transition de {fromSize} vers {toSize} en {duration}s");

        // Assurons-nous que la caméra est en mode orthographique
        if (!mainCamera.orthographic)
        {
            mainCamera.orthographic = true;
            Debug.Log("OrthographicSizeAction: Caméra passée en mode orthographique");
        }

        // Définir la valeur initiale
        mainCamera.orthographicSize = fromSize;

        float elapsedTime = 0;

        // Animation progressive
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Interpolation linéaire entre les valeurs
            mainCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, t);

            yield return null;
        }

        // S'assurer que la valeur finale est exactement celle demandée
        mainCamera.orthographicSize = toSize;

        Debug.Log($"OrthographicSizeAction: Transition terminée. Valeur finale: {mainCamera.orthographicSize}");
    }

    /// <summary>
    /// Gradually changes the current visible layer from one value to another
    /// </summary>
    /// <param name="fromLayer">Couche de départ</param>
    /// <param name="toLayer">Couche d'arrivée</param>
    /// <param name="duration">Durée totale de la transition</param>
    IEnumerator LayerAction(int fromLayer, int toLayer, float duration)
    {
        VisualGrid visualGrid = GameManager.Instance.visualGrid;
        CellInteractionController controller = GameManager.Instance.cellInteractionController;

        int minLayer = 0;
        int maxLayer = GameManager.Instance.gridSize - 1;

        fromLayer = Mathf.Clamp(fromLayer, minLayer, maxLayer);
        toLayer = Mathf.Clamp(toLayer, minLayer, maxLayer);

        Debug.Log($"LayerSequence: Démarrage de la couche {fromLayer} vers la couche {toLayer} en {duration}s");

        int currentLayer = visualGrid.CurrentVisibleLayer;

        while (currentLayer != fromLayer)
        {
            if (currentLayer < fromLayer)
            {
                controller.ShowLayer();
            }
            else
            {
                controller.HideLayer();
            }

            currentLayer = visualGrid.CurrentVisibleLayer;
            yield return null;

            if ((currentLayer == minLayer && fromLayer < minLayer) || (currentLayer == maxLayer && fromLayer > maxLayer))
            {
                break;
            }
        }

        if (fromLayer == toLayer || visualGrid.CurrentVisibleLayer == toLayer)
        {
            Debug.Log($"LayerSequence: Aucun changement nécessaire (actuel: {visualGrid.CurrentVisibleLayer}, cible: {toLayer})");
            yield break;
        }

        int layersToChange = Mathf.Abs(toLayer - visualGrid.CurrentVisibleLayer);
        float timePerLayer = duration / layersToChange;
        bool increasing = toLayer > visualGrid.CurrentVisibleLayer;

        Debug.Log($"LayerSequence: Phase principale - Changement de {layersToChange} couches, direction: {(increasing ? "+" : "-")}");

        for (int i = 0; i < layersToChange; i++)
        {
            int beforeChange = visualGrid.CurrentVisibleLayer;

            if (increasing)
            {
                controller.ShowLayer();
            }
            else
            {
                controller.HideLayer();
            }

            if (beforeChange == visualGrid.CurrentVisibleLayer)
            {
                Debug.Log($"LayerSequence: Limite atteinte à la couche {visualGrid.CurrentVisibleLayer}");
                break;
            }

            Debug.Log($"LayerSequence: {(increasing ? "Avancer" : "Reculer")} couche {i + 1}/{layersToChange} (maintenant: {visualGrid.CurrentVisibleLayer})");
            yield return new WaitForSecondsRealtime(timePerLayer);
        }

        Debug.Log($"LayerSequence: Terminé - Couche finale: {visualGrid.CurrentVisibleLayer} (objectif: {toLayer})");
    }

    /// <summary>
    /// Creates or destroys a cell at a specific position
    /// </summary>
    /// <param name="position">Position de la cellule dans la grille</param>
    /// <param name="create">True pour créer, False pour détruire</param>
    /// <param name="delay">Délai avant d'exécuter l'action (en secondes)</param>
    IEnumerator CellAction(int3[] positions, bool create, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);

        for (int i = 0; i < positions.Length; i++)
        {
            if (create)
            {
                GameManager.Instance.CreateCell(positions[i]);
                Debug.Log($"CellSequence: Cellule créée à ({positions[i].x}, {positions[i].y}, {positions[i].z})");
            }
            else
            {
                GameManager.Instance.DestroyCell(positions[i]);
                Debug.Log($"CellSequence: Cellule détruite à ({positions[i].x}, {positions[i].y}, {positions[i].z})");
            }
        }

    }

    /// <summary>
    /// Change the material of an existing cell
    /// </summary>
    /// <param name="position">Position de la cellule dans la grille</param>
    /// <param name="material">Nouveau material à appliquer</param>
    /// <param name="duration">Durée pendant laquelle le material reste appliqué (0 pour permanent)</param>
    /// <param name="delay">Délai avant d'exécuter l'action (en secondes)</param>
    IEnumerator ColorCellAction(int3 position, Material material, float duration = 0, float delay = 0)
    {
        if (delay > 0)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        GameObject cellObject = GameManager.Instance.GetCellObjectAt(position);

        if (cellObject != null)
        {
            MeshRenderer renderer = cellObject.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                Material originalMaterial = renderer.material;
                renderer.material = material;
                Debug.Log($"ColorCellSequence: Material changé pour la cellule à ({position.x}, {position.y}, {position.z})");
            }
            else
            {
                Debug.LogWarning($"ColorCellSequence: La cellule à ({position.x}, {position.y}, {position.z}) n'a pas de MeshRenderer");
            }
        }
        else
        {
            Debug.LogWarning($"ColorCellSequence: Aucune cellule trouvée à ({position.x}, {position.y}, {position.z})");
        }
    }

    /// <summary>
    /// Gradually resizes the grid from one size to another over a specified period of time
    /// </summary>
    /// <param name="targetSize">Taille cible de la grille</param>
    /// <param name="duration">Durée de la transition en secondes</param>
    /// <returns></returns>
    IEnumerator ResizeGridAction(int targetSize, float duration)
    {
        int startSize = GameManager.Instance.gridSize;

        if (startSize == targetSize || duration <= 0f)
        {
            GameManager.Instance.ResizeGrid(targetSize);
            Debug.Log($"ResizeGridAction: Redimensionnement immédiat de {startSize} à {targetSize}");
            yield break;
        }

        Debug.Log($"ResizeGridAction: Début du redimensionnement de {startSize} à {targetSize} en {duration}s");

        float elapsedTime = 0f;
        int prevSize = startSize;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            int currentSize = Mathf.RoundToInt(Mathf.Lerp(startSize, targetSize, t));

            if (currentSize != prevSize)
            {
                GameManager.Instance.ResizeGrid(currentSize);
                Debug.Log($"ResizeGridAction: Taille intermédiaire {currentSize}");
                prevSize = currentSize;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (GameManager.Instance.gridSize != targetSize)
        {
            GameManager.Instance.ResizeGrid(targetSize);
            Debug.Log($"ResizeGridAction: Taille finale ajustée à {targetSize}");
        }

        Debug.Log($"ResizeGridAction: Redimensionnement terminé ({startSize} → {targetSize})");
    }
#endregion

    #region PaternForSequence4
    private int3[] GenerateBoxPattern(int centerX, int centerY, int size, bool withInnerBox = false)
    {
        List<int3> positions = new List<int3>();
        int halfSize = size / 2;

        for (int x = centerX - halfSize; x <= centerX + halfSize; x++)
        {
            for (int z = centerY - halfSize; z <= centerY + halfSize; z++)
            {
                if (x == centerX - halfSize || x == centerX + halfSize ||
                    z == centerY - halfSize || z == centerY + halfSize)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        if (withInnerBox)
        {
            int innerHalfSize = halfSize / 2;
            for (int x = centerX - innerHalfSize; x <= centerX + innerHalfSize; x++)
            {
                for (int z = centerY - innerHalfSize; z <= centerY + innerHalfSize; z++)
                {
                    if (x == centerX - innerHalfSize || x == centerX + innerHalfSize ||
                        z == centerY - innerHalfSize || z == centerY + innerHalfSize)
                    {
                        positions.Add(new int3(x, centerY, z));
                    }
                }
            }
        }

        return positions.ToArray();
    }

    private int3[] GenerateBoxWithRoundedCorners(int centerX, int centerY, int size)
    {
        List<int3> positions = new List<int3>();
        int halfSize = size / 2;
        int innerHalfSize = halfSize / 2;

        for (int x = centerX - halfSize - 1; x <= centerX + halfSize + 1; x++)
        {
            for (int z = centerY - halfSize - 1; z <= centerY + halfSize + 1; z++)
            {
                if ((x >= centerX - halfSize && x <= centerX + halfSize &&
                    (z == centerY - halfSize - 1 || z == centerY + halfSize + 1)) ||
                    (z >= centerY - halfSize && z <= centerY + halfSize &&
                    (x == centerX - halfSize - 1 || x == centerX + halfSize + 1)))
                {
                    if (!((x == centerX - halfSize - 1 && (z == centerY - halfSize - 1 || z == centerY + halfSize + 1)) ||
                         (x == centerX + halfSize + 1 && (z == centerY - halfSize - 1 || z == centerY + halfSize + 1))))
                    {
                        positions.Add(new int3(x, centerY, z));
                    }
                }

                if ((x == centerX - halfSize && (z == centerY - halfSize || z == centerY + halfSize)) ||
                    (x == centerX + halfSize && (z == centerY - halfSize || z == centerY + halfSize)))
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX - innerHalfSize; x <= centerX + innerHalfSize; x++)
        {
            for (int z = centerY - innerHalfSize; z <= centerY + innerHalfSize; z++)
            {
                if (x == centerX - innerHalfSize || x == centerX + innerHalfSize ||
                    z == centerY - innerHalfSize || z == centerY + innerHalfSize)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        return positions.ToArray();
    }

    private int3[] GenerateCrossWithBlocks(int centerX, int centerY)
    {
        List<int3> positions = new List<int3>();

        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int z = centerY - 1; z <= centerY + 1; z++)
            {
                if (!(x == centerX && z == centerY))
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int z = centerY - 4; z <= centerY - 2; z++)
            {
                positions.Add(new int3(x, centerY, z));
            }
        }

        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int z = centerY + 2; z <= centerY + 4; z++)
            {
                positions.Add(new int3(x, centerY, z));
            }
        }

        for (int x = centerX - 4; x <= centerX - 2; x++)
        {
            for (int z = centerY - 1; z <= centerY + 1; z++)
            {
                positions.Add(new int3(x, centerY, z));
            }
        }

        for (int x = centerX + 2; x <= centerX + 4; x++)
        {
            for (int z = centerY - 1; z <= centerY + 1; z++)
            {
                positions.Add(new int3(x, centerY, z));
            }
        }

        return positions.ToArray();
    }

    private int3[] GenerateCheckerboardPattern(int centerX, int centerY)
    {
        List<int3> positions = new List<int3>();

        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int z = centerY - 2; z <= centerY + 2; z++)
            {
                if ((x + z) % 2 == 0)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int z = centerY - 5; z <= centerY - 3; z++)
            {
                if ((x + z) % 2 == 0)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int z = centerY + 3; z <= centerY + 5; z++)
            {
                if ((x + z) % 2 == 0)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX - 5; x <= centerX - 3; x++)
        {
            for (int z = centerY - 2; z <= centerY + 2; z++)
            {
                if ((x + z) % 2 == 0)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }

        for (int x = centerX + 3; x <= centerX + 5; x++)
        {
            for (int z = centerY - 2; z <= centerY + 2; z++)
            {
                if ((x + z) % 2 == 0)
                {
                    positions.Add(new int3(x, centerY, z));
                }
            }
        }
        return positions.ToArray();
    }
    #endregion
}