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
    public float cameraSpeed;

    public List<Blob> inactiveBlobs { get; private set; } = new List<Blob>();
    public List<Blob> blobs { get; private set; } = new List<Blob>();
    public bool displayHelp { get; private set; } = false;

    private Vector2 targetCameraPosition;
    private float targetCameraOrthographicSize;
    private bool autoZoomOut = false;
    private bool enableCollision = true;
    private Resolution blobResolution = Resolution.high;

    // Start is called before the first frame update
    void Start()
    {
        targetCameraOrthographicSize = camera.orthographicSize;
        CreateBlobs();
    }

    // Update is called once per frame
    void Update()
    {
        float screenRatio = Screen.width / Screen.height;
        Vector2 mousePositionOnScreen = Input.mousePosition;
        Vector2 mouseRelativePositionOnScreen = mousePositionOnScreen - new Vector2(Screen.width, Screen.height) / 2f;
        Vector2 mouseRelativePositionOnScreenNormalized = mouseRelativePositionOnScreen / new Vector2(Screen.width, Screen.height);

        targetCameraPosition = new Vector2((maxCameraOrthographicSize - targetCameraOrthographicSize) * screenRatio, maxCameraOrthographicSize - targetCameraOrthographicSize) * mouseRelativePositionOnScreenNormalized;

        Vector3 cameraPosition = Vector2.Lerp(camera.transform.position, targetCameraPosition, cameraSpeed * Time.deltaTime);
        cameraPosition.z = -10;
        camera.transform.position = cameraPosition;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            displayHelp = !displayHelp;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) && MouseMove())
        {
            Vector2 mousePositionOnWorld = camera.ScreenToWorldPoint(mousePositionOnScreen);
            Vector2 randomOffest = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

            ActivateRandomBlob(mousePositionOnWorld + randomOffest);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            autoZoomOut = !autoZoomOut;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            enableCollision = !enableCollision;
            blobs.ForEach(b => b.SetColliderEnabled(enableCollision));
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            blobs.ForEach(b => b.goToFinalPosition = true);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            while (inactiveBlobs.Count > 0)
            {
                Vector2 position = new Vector2(Random.Range(0f, image.width) - image.width / 2f, Random.Range(0f, image.height) - image.height / 2f);

                ActivateRandomBlob(position);
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            while (inactiveBlobs.Count > 0)
            {
                ActivateRandomBlob(Vector2.zero);
            }

            blobs.ForEach(b =>
            {
                b.transform.position = b.finalPosition;
                b.goToFinalPosition = true;
            });
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            targetCameraOrthographicSize -= Input.mouseScrollDelta.y;

            if (targetCameraOrthographicSize < 10)
                ChangeBlobResolution(Resolution.high);
            else if (targetCameraOrthographicSize < 30)
                ChangeBlobResolution(Resolution.medium);
            else
                ChangeBlobResolution(Resolution.low);
        }
        targetCameraOrthographicSize = Mathf.Clamp(targetCameraOrthographicSize, minCameraOrthographicSize, maxCameraOrthographicSize);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetCameraOrthographicSize, zoomSpeed * Time.deltaTime);

        if (autoZoomOut == true)
        {
            targetCameraOrthographicSize = Mathf.MoveTowards(targetCameraOrthographicSize, maxCameraOrthographicSize, Time.deltaTime);

            if (targetCameraOrthographicSize >= maxCameraOrthographicSize)
                autoZoomOut = false;
        }
    }

    private bool MouseMove()
    {
        return Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
    }

    private void ChangeBlobResolution(Resolution resolution)
    {
        if (blobResolution == resolution)
            return;

        blobResolution = resolution;

        blobs.ForEach(b => b.SetResolution(blobResolution));
    }

    private Blob ActivateRandomBlob(Vector2 position)
    {
        if (inactiveBlobs.Count == 0)
            return null;

        Blob blob = inactiveBlobs[Random.Range(0, inactiveBlobs.Count)];

        return ActivateBlob(position, blob);
    }

    private Blob ActivateBlob(Vector2 position, Blob blob)
    {
        int inactiveBlobCount = inactiveBlobs.Count;

        inactiveBlobs.Remove(blob);

        if (inactiveBlobCount > 0 && inactiveBlobs.Count == 0) // when the last blob is activate
        {
            autoZoomOut = true;
            blobs.ForEach(b => b.goToFinalPosition = true);
        }

        blob.transform.position = position;
        blob.gameObject.SetActive(true);


        return blob;
    }

    private void CreateBlobs()
    {
        for (int i = 0; i < image.width; i++)
            for (int j = 0; j < image.height; j++)
            {
                Color color = image.GetPixel(i, j);

                if (color.a <= 0.2f)
                    continue;

                Vector2 position = new Vector2(i - image.width / 2f, j - image.height / 2f);

                Blob blob = Instantiate(blobPrefab);
                blob.color = color;
                blob.finalPosition = position;
                blob.SetResolution(blobResolution);
                blob.gameObject.SetActive(false);

                blobs.Add(blob);
                inactiveBlobs.Add(blob);
            }
    }
}
