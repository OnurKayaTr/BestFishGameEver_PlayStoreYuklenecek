using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GIGA.PixelCableRenderer
{
    [ExecuteInEditMode]
    public class CableRenderer : MonoBehaviour
    {
        // Constants
        public const string VERSION = "1.0.0";
        public const float MIN_VERTICAL_SCALE = 0.2f;
        protected const float SCALE_COMPRESSION = 100f;                 // Value used to compress scale into rg color channels in the shader.
        public const int MAX_PIXEL_SIZE = 24;                           // The maximum pixel size used for cable thickness.
        public const int CANVAS_RESIZE_FACTOR = 10;                   // Resizing multiplier used for canvas renderers
        public static int CANVAS_SIZE = 64;                          // This is the resolution of the "canvas" where the cable will be drawn onto. You can tweak this
                                                                        // parameter if cable pixels don't match your game's pixel size.

        public enum ColorMode
        {
            SingleColor = 0,
            LinearGradient = 1,
            GradientFromCenter = 2,
            AlternatePixels = 3,
            IndexedColor = 4,
            PixelTexture = 5,
            FullTexture = 6,
            RepeatedTexture = 7,
        }

        [Header("Cable Properties")]
        // Cable Properties
        [Range(0f,1f)]
        public float softness = 0.2f;
        [Range(0f, 1f)]
        public float shapeModifier = 0.5f;                // bell position from left to right
        [Range(1, MAX_PIXEL_SIZE)]
        public int pixelSize = 2;
        [Range(0f, 1f)]
        public float edgeSmoothness = 0.0f;               // Smoothing effect on the edges

        [Header("Wind Settings")]
        [Range(0f, 1f)]
        public float windAmount = 0;
        [Range(0f, 1f)]
        public float windSpeed = 0;
        public bool windRippleEffect;
        [Range(0f, 1f)]
        public float rippleModifier = 0.1f;               // ripple modifier

        [Header("Color Settings")]
        public ColorMode colorMode = ColorMode.SingleColor;
        public Color color1 = Color.black, color2 = Color.black;
        public Sprite texture;
        public int colorIndex;                          // used for IndexedColor coloring mode
        public CableRendererIndexedColors indexedColorsData;
        private static bool indexedColorsSet;
        
        [Header("Misc. Settings")]
        public bool staticParameters;
        public bool simulateMovementInEditor;

        // Shared "canvas" texture
        private static Texture2D canvasTexture;
        private static Sprite canvasTextureSprite;
        
        // Renderer components
        protected SpriteRenderer spriteRenderer = null;
        protected Image canvasRenderer = null;
        protected RectTransform rectTransform;

        // Misc getters
        public Transform ParentContainer => this.GetParentContainer();

        // Internal properties
        protected Material cableMaterial;
        private static Material sharedTextureMaterial;                      // For textures a shader variant is needed, so a new material is istantiated
        private static Dictionary<int, Material> sharedCanvasMaterials;     // Materials dictionary used for canvas batching
        public Material defaultCableMaterial;
        [SerializeField]
        protected bool useBatching;                       // Enables batching for canvas renderers
        [SerializeField]
        protected int batchingGroup;                      // Canvas renderers of the same group will share the material
        private float currentWindSpeed;
        
        #region Initialization
        void Awake()
        {
            this.Init();
            // Applying initial parameters
            this.ApplyShaderProperties();
        }

        public virtual void Init()
		{
            // Creating texture if null
            if (canvasTexture == null || canvasTexture.width != CANVAS_SIZE)
            {
                canvasTexture = new Texture2D(CANVAS_SIZE, CANVAS_SIZE);
                canvasTexture.filterMode = FilterMode.Point;
                canvasTexture.name = "CableRenderer Canvas";
                canvasTexture.Apply();
            }
            if (canvasTextureSprite == null || canvasTextureSprite.pixelsPerUnit != CANVAS_SIZE)
                canvasTextureSprite = Sprite.Create(canvasTexture, new Rect(0, 0, canvasTexture.width, canvasTexture.height), new Vector2(0.5f, 0.5f), CANVAS_SIZE);

            this.spriteRenderer = this.GetComponent<SpriteRenderer>();
            this.canvasRenderer = this.GetComponent<Image>();

            if (spriteRenderer != null)
            {
                this.spriteRenderer.sprite = canvasTextureSprite;
                this.cableMaterial = spriteRenderer.sharedMaterial;
            }
            else if (canvasRenderer != null)
            {
                this.rectTransform = this.GetComponent<RectTransform>();
                this.canvasRenderer.sprite = canvasTextureSprite;
                SetCanvasMaterialInstancing();
            }
            else
            {
                Debug.LogError("CableRenderer is missing either SpriteRenderer or UI.Image");
                return;
            }

            // Setting shader variant
            this.SetShaderTextureVariant(this.IsUsingTexture());

            // Setting general shader properties
            if (cableMaterial.GetFloat("_InverseCanvasSize") != 1 / (float)CANVAS_SIZE)
                cableMaterial.SetFloat("_InverseCanvasSize", 1 / (float)CANVAS_SIZE);
        }

		#endregion

        #region Update

		protected virtual void Update()
        {
            // Updating rectTransform, if any
            if (this.rectTransform != null)
                this.UpdateRectTransform();

            // Applying shader properties
            if ((!Application.isPlaying && this.simulateMovementInEditor) || !this.staticParameters)
                this.ApplyShaderProperties();
            
            // If static, disabling after first run
            if (Application.isPlaying && this.staticParameters && this.rectTransform == null)
                this.enabled = false;
        }

        protected void UpdateRectTransform()
        {
            // Forcing size to 1
            if(this.rectTransform.sizeDelta != Vector2.one)
                this.rectTransform.sizeDelta = Vector2.one;
            // Forcing pivots and anchors
            if(this.rectTransform.pivot.x != 0.5f || this.rectTransform.pivot.y != 0.5f)
                this.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            if (this.rectTransform.anchorMin.x != 0.5f || this.rectTransform.anchorMin.y != 0.5f)
                this.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            if (this.rectTransform.anchorMax.x != 0.5f || this.rectTransform.anchorMax.y != 0.5f)
                this.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);


        }

        #endregion

        #region Shader Settings

        /// <summary>
        /// Applies shader parameters to update the renderer.
        /// </summary>
        public virtual void ApplyShaderProperties()
        {
            // Shared calculations
            this.currentWindSpeed = Mathf.Lerp(this.currentWindSpeed, this.windSpeed, Time.deltaTime*10);
            float bellParam3 = 20f + this.rippleModifier * 20;
            if (this.spriteRenderer != null)
            {
                var props = new MaterialPropertyBlock();
				props.SetFloat("_CableWindAmount", this.windAmount);
				props.SetFloat("_CableWindSpeed", this.currentWindSpeed * 4);
				props.SetInt("_RippleToggle", this.windRippleEffect ? 1 : 0);
				this.spriteRenderer.color = new Color(this.transform.localScale.x / SCALE_COMPRESSION, this.transform.localScale.y / SCALE_COMPRESSION, this.softness,this.colorIndex / (float)this.indexedColorsData.indexedColors.Length);
                props.SetColor("_Color1", this.color1);
                props.SetColor("_Color2", this.color2);
                props.SetInt("_ColorMode", Mathf.Min(4,(int)this.colorMode));
                props.SetInt("_PixelSize", Mathf.Clamp(this.pixelSize, 1, MAX_PIXEL_SIZE));
                props.SetFloat("_Smoothness",this.edgeSmoothness);
                if (this.texture != null)
                    props.SetTexture("_CableTexture", this.texture.texture);
                props.SetFloat("_BellParam1", this.windRippleEffect ? 0.5f : this.shapeModifier);
                props.SetFloat("_BellParam3", bellParam3);

#if UNITY_EDITOR
                if (Application.isPlaying || this.simulateMovementInEditor && Selection.activeGameObject == this.gameObject)
                {
                    props.SetInt("_TimeEnabled", 1);
                }
                else
                    props.SetInt("_TimeEnabled", 0);
#else
                props.SetInt("_TimeEnabled", 1);
#endif

                this.spriteRenderer.SetPropertyBlock(props);
            }
            else if (this.canvasRenderer != null)
            {
                cableMaterial.SetFloat("_CableWindAmount", this.windAmount);
                cableMaterial.SetFloat("_CableWindSpeed", this.currentWindSpeed * 4);
                cableMaterial.SetInt("_RippleToggle", this.windRippleEffect ? 1 : 0);
                cableMaterial.SetColor("_Color1", this.color1);
                cableMaterial.SetColor("_Color2", this.color2);
                cableMaterial.SetInt("_ColorMode", Mathf.Min(4, (int)this.colorMode)); 
                cableMaterial.SetInt("_PixelSize", Mathf.Clamp(this.pixelSize, 1, MAX_PIXEL_SIZE) * CANVAS_RESIZE_FACTOR);
                cableMaterial.SetFloat("_Smoothness", this.edgeSmoothness);
                if (this.texture != null)
                    cableMaterial.SetTexture("_CableTexture", this.texture.texture);
                cableMaterial.SetFloat("_BellParam1", this.windRippleEffect ? 0.5f : this.shapeModifier);
                cableMaterial.SetFloat("_BellParam3", bellParam3);

#if UNITY_EDITOR
                if (Application.isPlaying || this.simulateMovementInEditor && Selection.activeGameObject == this.gameObject)
                {
                    cableMaterial.SetInt("_TimeEnabled", 1);
                }
                else
                    cableMaterial.SetInt("_TimeEnabled", 0);
#else
                cableMaterial.SetInt("_TimeEnabled", 1);
#endif

                this.canvasRenderer.color = new Color(this.transform.localScale.x / SCALE_COMPRESSION, this.transform.localScale.y / SCALE_COMPRESSION, this.softness, this.colorIndex / (float)this.indexedColorsData.indexedColors.Length);
            }

            // Setting indexed colors if never done before
            if (this.colorMode == ColorMode.IndexedColor && !indexedColorsSet)
                this.cableMaterial.SetColorArray("_IndexedColors", this.indexedColorsData.indexedColors);

#if UNITY_EDITOR
            // In the editor, updating the shader texture variant
            if (this.cableMaterial.IsKeywordEnabled("TEXTURE_ON") && !this.IsUsingTexture() || this.cableMaterial.IsKeywordEnabled("TEXTURE_OFF") && this.IsUsingTexture())
                this.SetShaderTextureVariant(this.IsUsingTexture());
#endif
            // Adjusting Y scale based on softness
            float adjustedScale = this.VerticalScaleFromSoftness();
            if (adjustedScale != this.transform.localScale.y)
                this.transform.localScale = new Vector3(this.transform.localScale.x, adjustedScale, this.transform.localScale.z);
        }

        protected virtual void SetShaderTextureVariant(bool on)
        {
            this.ExecuteShaderTextureVariant(on, ref CableRenderer.sharedTextureMaterial,ref CableRenderer.sharedCanvasMaterials);
        }

        protected void ExecuteShaderTextureVariant(bool on,ref Material sharedStaticMaterial, ref Dictionary<int, Material> canvasMaterialsDictionary)
        {
            if (on)
            {
                // Instantiating the shared texture material if it doesn't exist yet
                if (sharedStaticMaterial == null)
                {
                    sharedStaticMaterial = Material.Instantiate(this.cableMaterial);
                    sharedStaticMaterial.EnableKeyword("TEXTURE_ON");
                    sharedStaticMaterial.DisableKeyword("TEXTURE_OFF");

                }
                this.cableMaterial = sharedStaticMaterial;
            }
            else
            {
                if (this.spriteRenderer != null)
                    this.cableMaterial = defaultCableMaterial;
                else
                    ExecuteCanvasMaterialInstancing(ref canvasMaterialsDictionary);
            }

            if (this.spriteRenderer != null)
                this.spriteRenderer.material = this.cableMaterial;
            else if (this.canvasRenderer != null)
                this.canvasRenderer.material = this.cableMaterial;
        }

        protected virtual void SetCanvasMaterialInstancing()
        {
            this.ExecuteCanvasMaterialInstancing(ref CableRenderer.sharedCanvasMaterials);
        }

        protected void ExecuteCanvasMaterialInstancing(ref Dictionary<int, Material> canvasMaterialsDictionary)
        {
            if (!this.useBatching)
            {
                // If not using batching, just instantiate a new material
                this.cableMaterial = Material.Instantiate(this.defaultCableMaterial);
            }
            else
            {
                // If using batching instantiante a new material, or assigning an existing one of the same group
                if (canvasMaterialsDictionary == null)
                    canvasMaterialsDictionary = new Dictionary<int, Material>();

                // Textured materials use negative id
                if (this.batchingGroup > 0)
                {
                    int batchGroupToUse = Mathf.Abs(this.batchingGroup);
                    if (this.IsUsingTexture())
                        batchGroupToUse = -batchGroupToUse;

                    if (canvasMaterialsDictionary.ContainsKey(batchGroupToUse) && canvasMaterialsDictionary[batchGroupToUse] != null)
                    {
                        this.cableMaterial = canvasMaterialsDictionary[batchGroupToUse];
                    }
                    else
                    {
                        Material mat = Material.Instantiate(this.defaultCableMaterial);
                        if (this.IsUsingTexture())
                        {
                            mat.EnableKeyword("TEXTURE_ON");
                            mat.DisableKeyword("TEXTURE_OFF");
                        }

                        canvasMaterialsDictionary[batchGroupToUse] = mat;
                        this.cableMaterial = canvasMaterialsDictionary[batchGroupToUse];
                    }
                }
                else
                    Debug.LogError("Negative and zero batch groups are reserved");

            }

            this.canvasRenderer.material = this.cableMaterial;

        }

        protected bool IsUsingTexture()
        {
            return this.colorMode == ColorMode.PixelTexture || this.colorMode == ColorMode.FullTexture || this.colorMode == ColorMode.RepeatedTexture;
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Calculates the vertical scale of the renderer, based on softness & wind amount
        /// </summary>
        /// <returns></returns>
        public float VerticalScaleFromSoftness()
        {
            return  this.transform.localScale.x * (MIN_VERTICAL_SCALE + this.softness * (1.0f + this.windAmount));
        }

        /// <summary>
        /// Calculates the softness, starting from the cable vertical scale
        /// </summary>
        /// <returns></returns>
        public float SoftnessFromVerticalScale(float verticalScale)
        {
            if (this.transform.localScale.x > 0)
                return (verticalScale / this.transform.localScale.x - MIN_VERTICAL_SCALE) / (1.0f + this.windAmount);
            else
                return 0;
        }

        #endregion

        #region Misc Functions

        private Transform GetParentContainer()
        {
            if (this.canvasRenderer == null)
                return this.transform.parent;
            else
            {
                if (this.transform.parent != null && this.transform.parent.GetComponent<Canvas>() == null)
                    return this.transform.parent;
                else
                    return null;
            }
        }

		#endregion

		#region Editor Functions

		protected void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying )
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        #endregion
    }


}
