using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("You can't have two GameManager singleton !");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    #endregion

    public new Camera camera;
    public Blob blobPrefab;

    [Space]

    public Texture2D image;
    public float minCameraOrthographicSize;
    public float maxCameraOrthographicSize;
    public float zoomSpeed;

    public List<Blob> inactiveBlobs { get; private set; } = new List<Blob>();
    public List<Blob> blobs { get; private set; } = new List<Blob>();
    public bool allBlobIsActive => inactiveBlobs.Count == 0;
    public bool disableCollision { get; private set; } = false;

    private float targetCameraOrthographicSize;

    // Start is called before the first frame update
    void Start()
    {
        targetCameraOrthographicSize = camera.orthographicSize;
        CreateBlobs();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePositionOnScreen = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector2 mousePositionOnWorld = camera.ScreenToWorldPoint(mousePositionOnScreen);
            Vector2 randomOffest = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

            int randomIndex = Random.Range(0, inactiveBlobs.Count);
            Blob blob = inactiveBlobs[randomIndex];
            inactiveBlobs.RemoveAt(randomIndex);

            blob.transform.position = mousePositionOnWorld + randomOffest;
            blob.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            disableCollision = !disableCollision;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            blobs.ForEach(b => b.goToFinalPosition = true);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (Blob blob in blobs)
            {
                Vector2 position = new Vector2(Random.Range(0f, 167f) - image.width / 2f, Random.Range(0f, 167f) - image.height / 2f);

                blob.transform.position = position;
                blob.gameObject.SetActive(true);

                inactiveBlobs.Clear();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (Blob blob in blobs)
            {
                blob.transform.position = blob.finalPosition;
                blob.goToFinalPosition = true;
                blob.gameObject.SetActive(true);

                inactiveBlobs.Clear();
            }
        }

        targetCameraOrthographicSize -= Input.mouseScrollDelta.y;
        targetCameraOrthographicSize = Mathf.Clamp(targetCameraOrthographicSize, minCameraOrthographicSize, maxCameraOrthographicSize);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetCameraOrthographicSize, zoomSpeed * Time.deltaTime);

        if (allBlobIsActive == true)
        {
            targetCameraOrthographicSize = Mathf.MoveTowards(targetCameraOrthographicSize, maxCameraOrthographicSize, Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (allBlobIsActive == true)
        {
            int randomIndex = Random.Range(0, blobs.Count);
            Blob blob = blobs[randomIndex];
            blobs.RemoveAt(randomIndex);

            blob.goToFinalPosition = true;
        }
    }

    private void CreateBlobs()
    {
        for (int i = 0; i < image.width; i++)
            for (int j = 0; j < image.height; j++)
            {
                Color color = image.GetPixel(i, j);

                if (color.a <= 0.1f)
                    continue;

                Vector2 position = new Vector2(i - image.width / 2f, j - image.height / 2f);

                Blob blob = Instantiate(blobPrefab);
                blob.color = color;
                blob.finalPosition = position;
                blob.gameObject.SetActive(false);

                blobs.Add(blob);
                inactiveBlobs.Add(blob);
            }
    }
}
