using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    public Color backgroundColor = new Color(0.8f, 0.9f, 1f, 1f);
    public bool useGradient = false;
    public Color gradientTopColor = new Color(0.5f, 0.7f, 1f, 1f);
    public Color gradientBottomColor = new Color(0.8f, 0.9f, 1f, 1f);
    public bool autoFitCamera = true;
    
    [Header("Parallax Background")]
    public bool enableParallax = false;
    public Transform[] backgroundLayers;
    public float[] parallaxSpeed;
    
    [Header("Tile Background")]
    public bool tileBackground = false;
    public float tileSize = 20f;
    
    private Camera mainCamera;
    private Vector3 previousCameraPosition;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }
        
        previousCameraPosition = mainCamera.transform.position;
        
        // Setup background
        SetupBackground();
        
        // Auto fit camera to background
        if (autoFitCamera)
        {
            FitCameraToBackground();
        }
        
        Debug.Log("BackgroundManager initialized");
    }
    
    void Update()
    {
        if (enableParallax && backgroundLayers != null && backgroundLayers.Length > 0)
        {
            UpdateParallax();
        }
    }
    
    void SetupBackground()
    {
        // Create background sprite if it doesn't exist
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set background color
        spriteRenderer.color = backgroundColor;
        
        // Create a simple white sprite if none assigned
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = CreateWhiteSprite();
        }
        
        // Setup tiling
        if (tileBackground)
        {
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = new Vector2(tileSize, tileSize);
        }
        
        // Set sorting order to background
        spriteRenderer.sortingOrder = -10;
    }
    
    void UpdateParallax()
    {
        if (mainCamera == null) return;
        
        Vector3 deltaCameraPosition = mainCamera.transform.position - previousCameraPosition;
        
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] != null)
            {
                Vector3 parallaxOffset = deltaCameraPosition * parallaxSpeed[i];
                backgroundLayers[i].position += parallaxOffset;
            }
        }
        
        previousCameraPosition = mainCamera.transform.position;
    }
    
    Sprite CreateWhiteSprite()
    {
        // Create a simple white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
    
    void FitCameraToBackground()
    {
        if (mainCamera == null) return;
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("No sprite renderer or sprite found for camera fitting");
            return;
        }
        
        // Ensure camera is orthographic
        if (!mainCamera.orthographic)
        {
            mainCamera.orthographic = true;
            Debug.Log("Camera set to orthographic mode");
        }
        
        // Calculate the required orthographic size
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        
        // Get screen aspect ratio
        float screenAspect = (float)Screen.width / Screen.height;
        
        // Calculate the orthographic size needed to fit the sprite
        float orthographicSize = spriteHeight / 2f;
        
        // If sprite is wider than screen aspect ratio, adjust for width
        if (spriteWidth > spriteHeight * screenAspect)
        {
            orthographicSize = spriteWidth / (2f * screenAspect);
        }
        
        mainCamera.orthographicSize = orthographicSize;
        
        // Center camera on background
        Vector3 backgroundPosition = transform.position;
        backgroundPosition.z = mainCamera.transform.position.z;
        mainCamera.transform.position = backgroundPosition;
        
        Debug.Log($"Camera fitted to background. Orthographic size: {orthographicSize}");
    }
    
    void OnValidate()
    {
        // Validate parallax arrays
        if (backgroundLayers != null && parallaxSpeed != null)
        {
            if (backgroundLayers.Length != parallaxSpeed.Length)
            {
                System.Array.Resize(ref parallaxSpeed, backgroundLayers.Length);
            }
        }
        
        // Ensure parallax speeds are reasonable
        if (parallaxSpeed != null)
        {
            for (int i = 0; i < parallaxSpeed.Length; i++)
            {
                parallaxSpeed[i] = Mathf.Clamp01(parallaxSpeed[i]);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (backgroundLayers != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                if (backgroundLayers[i] != null)
                {
                    Gizmos.DrawWireSphere(backgroundLayers[i].position, 0.5f);
                }
            }
        }
    }
}
