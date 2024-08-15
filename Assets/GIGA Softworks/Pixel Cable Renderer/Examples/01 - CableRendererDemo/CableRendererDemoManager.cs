using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GIGA.PixelCableRenderer.Demo
{
    public class CableRendererDemoManager : MonoBehaviour
    {
        private const float BASE_INCREMENT = 0.1f;

        public CableRenderer cableRenderer;
        
        // GUI styles
        public Font font,fontBold;
        private GUIStyle infoLabel;
        private GUIStyle titleLabel;

        public Sprite pixelTexture;
        public Sprite fullTexture;

        private FieldInfo[] fields;
        private StringBuilder sbInfo = new StringBuilder();
        private StringBuilder sbTips = new StringBuilder();

        private string[] fieldsFilter = {
            "softness",
            "shapeModifier",
            "rippleModifier",
            "pixelSize",
            "edgeSmoothness",
            "windAmount",
            "windSpeed",
            "windRippleEffect",
            "colorMode"};

        private int selectedFieldIndex;

        private float valueIncrement = BASE_INCREMENT;
        private float keyPressMultiplier = 0;
        private int incrementDirection = 0;
        private bool lockValueInput;
        private int selectedColorModeInt;
        private bool helpEnabled = true;
        private bool welcomeText = true;
        
        void Start()
        {
            this.fields = CableRendererReflectionHelper.GetFields<CableRenderer>(this.cableRenderer,fieldsFilter);


        }

        void Update()
        {
            // Keyboard input
            if (Input.GetKeyDown(KeyCode.DownArrow) && this.selectedFieldIndex < this.fields.Length - 1)
            {
                this.selectedFieldIndex++;
                welcomeText = false;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) && this.selectedFieldIndex > 0)
            { 
                this.selectedFieldIndex--;
                welcomeText = false;
            }

            float minValue = 0;
            float maxValue = 1;
            bool lockInputAfterFirst = false;

            // Custom min/max values
            if (this.fields[this.selectedFieldIndex].Name == "pixelSize")
            {
                minValue = 1;
                maxValue = 24;
                lockInputAfterFirst = true;
            }
            else if (this.fields[this.selectedFieldIndex].Name == "colorMode")
            {
                minValue = 0;
                maxValue = 23;
                lockInputAfterFirst = true;
            }
            else if (this.fields[this.selectedFieldIndex].FieldType == typeof(bool))
            {
                lockInputAfterFirst = true;
            }

            if (!lockValueInput)
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    welcomeText = false;
                    if (this.fields[this.selectedFieldIndex].Name == "colorMode")
                    {
                        if(this.selectedColorModeInt < maxValue)
                            this.selectedColorModeInt++;
                        this.SetColorModeFromInt(this.selectedColorModeInt);
                        this.fields[this.selectedFieldIndex].SetValue(this.cableRenderer, this.cableRenderer.colorMode);
                    }
                    else
                        CableRendererReflectionHelper.IncreaseValue<CableRenderer>(this.fields[this.selectedFieldIndex], this.cableRenderer, this.valueIncrement * Time.deltaTime, minValue, maxValue);
                    
                    if (this.incrementDirection == 1)
                    {
                        this.valueIncrement += BASE_INCREMENT * Time.deltaTime * keyPressMultiplier;
                        this.keyPressMultiplier += 40 * Time.deltaTime;
                    }
                    else
                    {
                        this.valueIncrement = BASE_INCREMENT;
                        this.keyPressMultiplier = 0;
                    }
                    this.incrementDirection = 1;
                    this.lockValueInput = lockInputAfterFirst;
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    welcomeText = false;
                    if (this.fields[this.selectedFieldIndex].Name == "colorMode")
                    {
                        if (this.selectedColorModeInt > 0)
                            this.selectedColorModeInt--;
                        this.SetColorModeFromInt(this.selectedColorModeInt);
                        this.fields[this.selectedFieldIndex].SetValue(this.cableRenderer, this.cableRenderer.colorMode);
                    }
                    else
                        CableRendererReflectionHelper.IncreaseValue<CableRenderer>(this.fields[this.selectedFieldIndex], this.cableRenderer, this.valueIncrement * Time.deltaTime, minValue, maxValue, true);
                    
                    if (this.incrementDirection == -1)
                    {
                        this.valueIncrement += BASE_INCREMENT * Time.deltaTime * keyPressMultiplier;
                        this.keyPressMultiplier += 40 * Time.deltaTime;
                    }
                    else
                    {
                        this.valueIncrement = BASE_INCREMENT;
                        this.keyPressMultiplier = 0;
                    }
                    this.incrementDirection = -1;
                    this.lockValueInput = lockInputAfterFirst;
                }
                else
                {
                    this.valueIncrement = BASE_INCREMENT;
                    this.keyPressMultiplier = 0;
                    this.incrementDirection = 0;
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                    this.lockValueInput = false;
            }

            if (Input.GetKeyDown(KeyCode.F1))
                this.helpEnabled = !helpEnabled;


        }

        private void OnGUI()
		{
            // Setting up fonts
            if (this.infoLabel == null)
            {
                infoLabel = new GUIStyle(GUI.skin.label);
                infoLabel.fontSize = Mathf.RoundToInt(GUI.skin.label.fontSize * 0.55f);
                infoLabel.alignment = TextAnchor.UpperLeft;
                infoLabel.font = this.font;
            }
            if (this.titleLabel == null)
            {
                titleLabel = new GUIStyle(infoLabel);
                titleLabel.font = this.fontBold;
                titleLabel.fontStyle = FontStyle.Bold;
            }

            Font exFont = GUI.skin.font;
            GUI.skin.font = this.font;
            Color exColor = GUI.color;
            
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            for(int k=0;k<this.fields.Length;k++)
            {
                GUI.color = k == this.selectedFieldIndex ? new Color(0.9f,0.786f,0,1) : Color.white;

                string valueString = "";
                if (fields[k].FieldType == typeof(float))
                    valueString = ((float)fields[k].GetValue(this.cableRenderer)).ToString("0.00");
                else if (fields[k].FieldType == typeof(int))
                    valueString = ((int)fields[k].GetValue(this.cableRenderer)).ToString();
                else if (fields[k].FieldType == typeof(bool))
                    valueString = (bool)fields[k].GetValue(this.cableRenderer) ? "True" : "False";
                else if (fields[k].Name == "colorMode")
                {
                    valueString = NormalizeFieldName((cableRenderer.colorMode).ToString());
                    if (cableRenderer.colorMode == CableRenderer.ColorMode.IndexedColor)
                        valueString += " " + cableRenderer.colorIndex;
                }

                GUILayout.Space(-6);
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label($"{NormalizeFieldName(fields[k].Name)}",GUILayout.MaxWidth(200), GUILayout.MinHeight(24));
                GUILayout.Label($"{valueString}", GUILayout.MaxWidth(200), GUILayout.MinHeight(24));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUIStyle rightcentederLabel = new GUIStyle(GUI.skin.label);
            rightcentederLabel.fontStyle = FontStyle.Italic;
            rightcentederLabel.fontSize = Mathf.RoundToInt(GUI.skin.label.fontSize * 0.55f);
            rightcentederLabel.alignment = TextAnchor.UpperRight;
            GUI.color = new Color(0.9f, 0.786f, 0, 1);
            GUI.Label(new Rect(Screen.width - 600, Screen.height - 35, 600, 80), "GIGA Softworks - Soft Pixel Cables LITE ver. " + CableRenderer.VERSION + " ("+this.cableRenderer.GetType().Name+")  ", rightcentederLabel);

            GUI.color = exColor;
            GUI.skin.font = exFont;

            if(helpEnabled)
                this.UpdateInfoText();

        }

        private string NormalizeFieldName(string input)
        {
            string normalizedText = Regex.Replace(input, "(\\B[A-Z])", " $1");
            normalizedText = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalizedText.ToLower());
            return normalizedText;
        }

        private void UpdateInfoText()
        {



            Color exColor = GUI.color;
            GUI.color = Color.white;

            sbInfo.Clear();
            sbTips.Clear();

            if (welcomeText)
            {
                sbInfo.AppendLine("");
                sbInfo.AppendLine("Welcome to the Cable Renderer Demo!".ToUpper());
                GUI.Label(new Rect(10, Screen.height - 300, 400, 50), sbInfo.ToString(), this.titleLabel);
                sbInfo.Clear();
                sbInfo.AppendLine("F1 toggles help on/off");
                sbInfo.AppendLine("Use the arrow keys to navigate the parameters.");
                sbInfo.AppendLine("Drag the white handles to move the cable around.");
                GUI.Label(new Rect(10, Screen.height - 250, 400, 200), sbInfo.ToString(), this.infoLabel);
            }
            else
            {
                // Info text and tips
                switch (this.fields[this.selectedFieldIndex].Name)
                {
                    case "softness":    
                        sbInfo.AppendLine("The softness of the cable.");
                        sbTips.AppendLine("Recommended values are between 0.2 and 0.8");
                        break;
                    case "shapeModifier":
                        sbInfo.AppendLine("Changes the overall shape of the cable.\nDisabled if wind ripple effect is on.");
                        sbTips.AppendLine("If the cable is not horizontal use this to adapt the shape to the gravity point.");
                        break;
                    case "pixelSize":
                        sbInfo.AppendLine("Base size of the cable pixels.");
                        sbTips.AppendLine("If you want to scale the cable and mantain pixel size, set it as a child of another GameObject and scale the parent");
                        break;
                    case "edgeSmoothness":
                        sbInfo.AppendLine("Applies a fading effect to the edges.");
                        sbTips.AppendLine("Use this to blend the cable with background images.");
                        break;
                    case "windAmount":
                        sbInfo.AppendLine("Amount of the wind animation effect.");
                        sbTips.AppendLine("For high values, decrease the cable softness for better results.");
                        break;
                    case "windSpeed":
                        sbInfo.AppendLine("Speed of the wind animation effect.");
                        sbTips.AppendLine("Use high values to find the animation you like and then scale down for a more realistic effect.");
                        break;
                    case "windRippleEffect":
                        sbInfo.AppendLine("Adds a linear ripple effect to the wind animation.");
                        sbTips.AppendLine("Works best with long and soft cables.");
                        break;
                    case "rippleModifier":
                        sbInfo.AppendLine("Changes the width of the ripple effect.");
                        break;
                    case "colorMode":
                        sbInfo.AppendLine("Cycles between different rendering modes.\n\nTextures are not supported in LITE package.");
                        break;
                }

                // param info
                GUI.Label(new Rect(10, Screen.height - 340, 400, 40), this.NormalizeFieldName(this.fields[this.selectedFieldIndex].Name), this.titleLabel);
                GUI.Label(new Rect(10, Screen.height - 300, 400, 100), sbInfo.ToString(), this.infoLabel);

                // tips
                if (sbTips.Length > 0)
                {
                    GUI.Label(new Rect(10, Screen.height - 240, 400, 40), "TIPS:", this.titleLabel);
                    GUI.Label(new Rect(10, Screen.height - 200, 400, 100), sbTips.ToString(), this.infoLabel);
                }

            }
            GUI.color = exColor;

        }

        private int GetColorModeInt()
        {
            switch (this.cableRenderer.colorMode)
            {
                case CableRenderer.ColorMode.SingleColor:
                    return 0;
                case CableRenderer.ColorMode.LinearGradient:
                    return 1;
                case CableRenderer.ColorMode.GradientFromCenter:
                    return 2;
                case CableRenderer.ColorMode.AlternatePixels:
                    return 3;
                case CableRenderer.ColorMode.PixelTexture:
                    return 4;
                case CableRenderer.ColorMode.FullTexture:
                    return 5;
                case CableRenderer.ColorMode.RepeatedTexture:
                    return 6;
                case CableRenderer.ColorMode.IndexedColor:
                    return 7 + cableRenderer.colorIndex;
                default:
                    return 0;
            }
        }

        private void SetColorModeFromInt(int intValue)
        {
            switch (intValue)
            {
                case 0:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.SingleColor;
                    break;
                case 1:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.LinearGradient;
                    break;
                case 2:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.GradientFromCenter;
                    break;
                case 3:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.AlternatePixels;
                    break;
                case 4:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.PixelTexture;
                    this.cableRenderer.texture = this.pixelTexture;
                    break;
                case 5:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.FullTexture;
                    this.cableRenderer.texture = this.fullTexture;
                    break;
                case 6:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.RepeatedTexture;
                    this.cableRenderer.texture = this.fullTexture;
                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                    this.cableRenderer.colorMode = CableRenderer.ColorMode.IndexedColor;
                    this.cableRenderer.colorIndex = intValue - 7;
                    break;

            }

        }
    }
}
