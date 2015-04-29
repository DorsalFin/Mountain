using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFadeInOut : MonoBehaviour {

    public float fadeSpeed = 1.5f; // Speed that the screen fades to and from black.
    public Image image;
    private bool _fadingToClear = false;
    private bool _fadingToBlack = false;

    public void FadeToClear()
    {
        _fadingToBlack = false;
        _fadingToClear = true;
    }
    public void FadeToBlack()
    {
        _fadingToClear = false;
        _fadingToBlack = true;
    }

    void ToClear()
    {
        // Lerp the colour of the texture between itself and transparent.
        image.color = Color.Lerp(image.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    void ToBlack()
    {
        // Lerp the colour of the texture between itself and black.
        image.color = Color.Lerp(image.color, Color.black, fadeSpeed * Time.deltaTime);
    }

    void Update()
    {
        if (_fadingToClear)
        {
            ToClear();
            // If the texture is almost clear...
            if (image.color.a <= 0.05f)
            {
                // ... set the colour to clear and disable the GUITexture.
                image.color = Color.clear;
                image.enabled = false;
                _fadingToClear = false;
            }
        }

        if (_fadingToBlack)
        {
            ToBlack();
            // If the texture is almost clear...
            if (image.color.a >= 0.95f)
            {
                // ... set the colour to clear and disable the GUITexture.
                image.color = Color.clear;
                image.enabled = false;
                _fadingToBlack = false;
            }
        }
    }
}
