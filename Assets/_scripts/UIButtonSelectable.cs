using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Similar to UIButton, but handles multiple tween targets and can work similarly to radio button.
/// </summary>

public class UIButtonSelectable : MonoBehaviour
{
    /// <summary>
    /// Targets with a widget, renderer, or light that will have their color tweened.
    /// </summary>
    public GameObject[] tweenTargets;

    /// <summary>
    /// Color to apply on hover event (mouse only).
    /// </summary>
    public Color hover = new Color(0.75f, 0.75f, 0.75f, 1f);

    public Color hoverWhenSelected = new Color(0.85f, 0.85f, 0.85f, 1f);

    /// <summary>
    /// Color to apply on the pressed event.
    /// </summary>
    public Color pressed = Color.white;

    /// <summary>
    /// Color that will be applied when the button is disabled.
    /// </summary>
    public Color disabledColor = Color.gray;

    /// <summary>
    /// Additional target with a widget, renderer, or light that will have its color tweened.
    /// </summary>
    public GameObject additionalTweenTarget;

    /// <summary>
    /// Color to apply to additionalTweenTarget on hover event (mouse only).
    /// </summary>
    public Color secondaryHover = new Color(0.75f, 0.75f, 0.75f, 1f);

    public Color secondaryHoverWhenSelected = new Color(0.85f, 0.85f, 0.85f, 1f);

    /// <summary>
    /// Color to apply to additionalTweenTarget on the pressed event.
    /// </summary>
    public Color secondaryPressed = Color.black;

    /// <summary>
    /// Color that will be applied to additionalTweenTarget when the button is disabled.
    /// </summary>
    public Color secondaryDisabledColor = Color.gray;

    /// <summary>
    /// Duration of the tween process.
    /// </summary>
    public float duration = 0.05f;


    /// <summary>
    /// Additional target with a widget, renderer, or light that will have its scale tweened.
    /// </summary>
    public Transform scaleTweenTarget;
    bool bIsWaitingForScaleTween = false;

        /// <summary>
    /// Duration of the scale tween process.
    /// </summary>
    public float scaleDuration = 0.15f;
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 selectedScale = new Vector3(1.05f, 1.05f, 1.05f);
    Vector3 mScale;

    /// <summary>
    /// If the button is part of a selectable button group, specify the root object to use that all buttons are parented to. Note: It doesn't need to be the direct parent, use any object that is higher in hierarchy and common to all other buttons in the group.
    /// </summary>
    public Transform selectableButtonRoot;
    public Transform additionalButtonRoot; // use this ony if the button belongs to two groups

    /// <summary>
    /// Whether the button should stay selected.
    /// </summary>
    public bool isSelectable = true;

    // value will be overwritten by PlayerPrefs settings on start if settingsKey is set
    public bool isSelected = false; 

    /// <summary>
    /// Whether one of the buttons should always be selected
    /// </summary>
    public bool keepOneSelected = false;

    //public string settingsKey = ""; // set the key value used in PlayerPrefs settings for this button or button group (if there is any, otherwise leave it empty); Note: the game object's name will be used as a settings value

    [HideInInspector]
    public bool bForceDeselection = false; // fixes problem with deselecting the button remotely when keepOneSelected flag is set

    // by default, objects to enable/disable are enabled/disabled by component
    public bool enableDisableByGameObject = false;

    /// <summary>
    /// Add objects that should be enabled when selecting this button (if any)
    /// </summary>
    public GameObject[] objectsToEnableOnSelect;

    /// <summary>
    /// Add objects that should be disabled when selecting this button (if any)
    /// </summary>
    public GameObject[] objectsToDisableOnSelect;

    public UITable tableToNotify;

    protected Color[] mColors;
    protected Color mSecondaryColor;
    protected bool mInitDone = false;
    protected bool mStarted = false;
    protected bool mHighlighted = false;

	public List<EventDelegate> onClick = new List<EventDelegate>();
	


	void Awake()
	{
		if (scaleTweenTarget != null)
		{
			UISprite sprite = scaleTweenTarget.GetComponent<UISprite>();
			if (sprite != null)
				mScale = new Vector3(sprite.width, sprite.height, 1);
			else
				mScale = scaleTweenTarget.localScale;
		}
	}

    void Start()
    {
        mStarted = true;

        //if (isSelectable && settingsKey != "")
        //{
        //    if (PlayerPrefs.GetString(settingsKey) == gameObject.name)
        //    {
        //        isSelected = true;
        //    }
        //    else
        //    {
        //        isSelected = false;
        //    }
        //}

		if (isSelected)
		{
			OnPress(true);
			RefreshScale();
		}
    }

    void OnEnable()
    {
        if (isEnabled)
        {
            if (mHighlighted && mStarted)
                OnHover(UICamera.IsHighlighted(gameObject));
            else if (isSelected && mStarted)
			{
                OnPress(false);
				RefreshScale();
			}
            else
                UpdateColor(true, true);
        }
        else
            UpdateColor(true, true);
    }

    void OnDisable()
    {
        UpdateColor(false, true);

        // commented this so we can distinguish between just being disabled by a higher button and actual selection
        //isSelected = false;
    }

	protected void Init(bool saveTweenTargetsColours, bool saveAdditionalTweenTargerColour)
	{
		mInitDone = true;
		ResetDefaultColours(saveTweenTargetsColours, saveAdditionalTweenTargerColour);
	}

	public void ResetDefaultColours(bool saveTweenTargetsColours, bool saveAdditionalTweenTargerColour)
	{
        if (saveTweenTargetsColours)
        {
            mColors = new Color[tweenTargets.Length];

            for (int i = 0; i < tweenTargets.Length; i++)
            {
                UIWidget widget = tweenTargets[i].GetComponent<UIWidget>();

                if (widget != null)
                {
                    mColors[i] = widget.color;
                }
                else
                {
                    Renderer ren = tweenTargets[i].GetComponent<Renderer>();

                    if (ren != null)
                    {
                        mColors[i] = ren.material.color;
                    }
                    else
                    {
                        Light lt = tweenTargets[i].GetComponent<Light>();

                        if (lt != null)
                        {
                            mColors[i] = lt.color;
                        }
                        else
                        {
#if ME_DEBUG_ON
                            Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing for UIButtonSelectable to color", this);
#endif
                            mColors[i] = Color.black;
                            //enabled = false;
                        }
                    }
                }
            }
        }

        if (saveAdditionalTweenTargerColour)
        {
            if (additionalTweenTarget)
            {
                UIWidget widget = additionalTweenTarget.GetComponent<UIWidget>();

                if (widget != null)
                {
                    mSecondaryColor = widget.color;
                }
                else
                {
                    Renderer ren = additionalTweenTarget.GetComponent<Renderer>();

                    if (ren != null)
                    {
                        mSecondaryColor = ren.material.color;
                    }
                    else
                    {
                        Light lt = additionalTweenTarget.GetComponent<Light>();

                        if (lt != null)
                        {
                            mSecondaryColor = lt.color;
                        }
                        else
                        {
#if ME_DEBUG_ON
                            Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing additional for UIButtonSelectable to color", this);
#endif
                            mSecondaryColor = Color.black;
                            //enabled = false;
                        }
                    }
                }
            }
        }
    }

    public void RefreshTweenTargetColours()
    {
        Init(true, false);
    }

	public void OnHoverOver()
	{
		OnHover(true);
	}

	public void OnHoverOut()
	{
		OnHover(false);
	}

    void OnHover(bool isOver)
    {
        if (isEnabled)
        {
            if (enabled)
            {
                if (!mInitDone)
                    Init(true, true);
                for (int i = 0; i < tweenTargets.Length; i++)
                {
                    TweenColor.Begin(tweenTargets[i], duration, isOver ? (isSelected ? hoverWhenSelected : hover) : (isSelected ? pressed : mColors[i]));
                }
                if (additionalTweenTarget)
                {
                    TweenColor.Begin(additionalTweenTarget, duration, isOver ? (isSelected ? secondaryHoverWhenSelected : secondaryHover) : (isSelected ? secondaryPressed : mSecondaryColor));
                }
                if (scaleTweenTarget && !bIsWaitingForScaleTween)
                {
					UISprite sprite = scaleTweenTarget.GetComponent<UISprite>();
					if (sprite != null)
					{
						TweenHeight.Begin(sprite, scaleDuration, isSelected ? (int)(mScale.y * selectedScale.y) : (isOver ? (int)(mScale.y * hoverScale.y) : (int)mScale.y)).method = UITweener.Method.EaseInOut;
						TweenWidth.Begin(sprite, scaleDuration, isSelected ? (int)(mScale.x * selectedScale.x) : (isOver ? (int)(mScale.x * hoverScale.x) : (int)mScale.x)).method = UITweener.Method.EaseInOut;
					}
					else
                    	TweenScale.Begin(scaleTweenTarget.gameObject, scaleDuration, isSelected ? Vector3.Scale(mScale, selectedScale) : (isOver ? Vector3.Scale(mScale, hoverScale) : mScale)).method = UITweener.Method.EaseInOut;
                }

                mHighlighted = isOver;
            }
        }
    }

	public void OnPressed()
	{
		OnPress(true);
	}

	public void OnRelease()
	{
		OnPress(false);
	}

    void OnPress(bool isPressed)
    {
        if (isEnabled)
        {
            if (!mInitDone)
                Init(true, true);
            if (enabled)
            {
                bool bIsHighlighted = UICamera.IsHighlighted(gameObject);
                for (int i = 0; i < tweenTargets.Length; i++)
                {
                    TweenColor.Begin(tweenTargets[i], duration, isPressed ? pressed : (bIsHighlighted ? (isSelected ? hoverWhenSelected : hover) : (isSelected ? pressed : mColors[i])));
                }
                if (additionalTweenTarget)
                {
                    TweenColor.Begin(additionalTweenTarget, duration, isPressed ? secondaryPressed : (bIsHighlighted ? (isSelected ? secondaryHoverWhenSelected : secondaryHover) : (isSelected ? secondaryPressed : mSecondaryColor)));
                }
            }
        }
    }

    public void OnClick()
    {
        //if (!enabled)
        //{
			foreach (EventDelegate del in onClick)
				del.Execute();
            //return;
        //}

        if (!isSelected)
        {
            // unselect previous button if any selected
            if (selectableButtonRoot != null)
            {
                UIButtonSelectable[] aButtons = selectableButtonRoot.GetComponentsInChildren<UIButtonSelectable>(true);

                foreach (UIButtonSelectable button in aButtons)
                {
                    if (button.isSelected && (selectableButtonRoot == button.selectableButtonRoot || selectableButtonRoot == button.additionalButtonRoot))
                    {
                        if (keepOneSelected)
                            button.bForceDeselection = true;
						UIPlaySound buttonSound = button.gameObject.GetComponent<UIPlaySound>();
                        if (buttonSound)
                            buttonSound.enabled = false;
                        button.gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver); // this should also force the previously selected button to hide any sliding panels it uses
                        if (buttonSound)
                            buttonSound.enabled = true;
                    }
                }

                if (additionalButtonRoot)
                {
                    aButtons = additionalButtonRoot.GetComponentsInChildren<UIButtonSelectable>(true);

                    foreach (UIButtonSelectable button in aButtons)
                    {
                        if (button.isSelected && (additionalButtonRoot == button.selectableButtonRoot || additionalButtonRoot == button.additionalButtonRoot))
                        {
                            if (keepOneSelected)
                                button.bForceDeselection = true;
							UIPlaySound buttonSound = button.gameObject.GetComponent<UIPlaySound>();
                            if (buttonSound)
                                buttonSound.enabled = false;
                            button.gameObject.SendMessage("OnClick"); // this should also force the previously selected button to hide any sliding panels it uses
                            if (buttonSound)
                                buttonSound.enabled = true;
                        }
                    }
                }
            }

            if (isSelectable)
            {
                isSelected = true;
                OnPress(false);
            }
        }
        else
        {
            if (isSelectable && (!keepOneSelected || bForceDeselection))
            {
                ChangeSelection(false);
            }
        }

        if (isSelectable)
        {
            //if (!bForceDeselection)
            //{
                //if (settingsKey != "")
                //{
                //    // setting new value or clearing current one
                //    PlayerPrefs.SetString(settingsKey, isSelected ? gameObject.name : "");
                //    PlayerPrefs.Save();
                //}

                if (!(scaleTweenTarget != null && isSelected))
                    EnableOrDisableObjects();

            //}

            bForceDeselection = false;
        }

		RefreshScale();
    }

	void RefreshScale()
	{
		if (scaleTweenTarget)
		{
			UISprite sprite = scaleTweenTarget.GetComponent<UISprite>();
			if (sprite != null)
			{
				int newWidth = isSelected ? (int)(mScale.x * selectedScale.x) : (UICamera.IsHighlighted(gameObject) ? (int)(mScale.x * hoverScale.x) : (int)mScale.x);
				int newHeight = isSelected ? (int)(mScale.y * selectedScale.y) : (UICamera.IsHighlighted(gameObject) ? (int)(mScale.y * hoverScale.y) : (int)mScale.y);
				if (sprite.width != newWidth || sprite.height != newHeight)
				{
					TweenWidth.Begin(sprite, scaleDuration, newWidth).method = UITweener.Method.EaseInOut;
					TweenHeight scale = TweenHeight.Begin(sprite, scaleDuration, newHeight);
					scale.method = UITweener.Method.EaseInOut;
					EventDelegate.Add(scale.onFinished, ScaleFinished, true);
					bIsWaitingForScaleTween = true;
				}
			}
			else
			{
				Vector3 newScale = isSelected ? Vector3.Scale(mScale, selectedScale) : UICamera.IsHighlighted(gameObject) ? Vector3.Scale(mScale, hoverScale) : mScale;
				if (newScale != scaleTweenTarget.transform.localScale)
				{
					TweenScale scale = TweenScale.Begin(scaleTweenTarget.gameObject, scaleDuration, newScale);
					scale.method = UITweener.Method.EaseInOut;
					EventDelegate.Add(scale.onFinished, ScaleFinished, true);
					bIsWaitingForScaleTween = true;
				}
			}
		}
	}

    public void ScaleFinished()
    {
#if ME_DEBUG_ON
        //Debug.Log("Scale finished - notifying table " + tableToNotify + "   Button " + name);
#endif

        if (isSelected)
		{
            EnableOrDisableObjects();
		}
		else if (tableToNotify)
        {
            tableToNotify.Reposition();
			UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(gameObject);
			if (scrollView)
			{
				scrollView.InvalidateBounds();
				if (scrollView.shouldMoveVertically)
					scrollView.Scroll(0.0001f); // fake scroll to reposition panel
			}
        }

        bIsWaitingForScaleTween = false;
    }

    public void ChangeSelection(bool bIsSelected)
    {
        if (isSelected != bIsSelected)
        {
            isSelected = bIsSelected;
            OnPress(false);
			RefreshScale();
        }
    }

    public void EnableOrDisableObjects()
    {
        if (enableDisableByGameObject)
            EnableOrDisableObjectsByGameObject();
        else
            EnableOrDisableObjectsByComponent();

		if (tableToNotify)
		{
			tableToNotify.Reposition();
			UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(gameObject);
			if (scrollView)
			{
				scrollView.InvalidateBounds();
				if (scrollView.shouldMoveVertically)
					scrollView.Scroll(0.0001f); // fake scroll to reposition panel
			}
		}
    }

    void EnableOrDisableObjectsByGameObject()
    {
        foreach (GameObject target in objectsToDisableOnSelect)
        {
            target.SetActive(!isSelected);
        }

        foreach (GameObject target in objectsToEnableOnSelect)
        {
            target.SetActive(isSelected);
        }
    }

    void EnableOrDisableObjectsByComponent()
    {
        foreach (GameObject target in objectsToDisableOnSelect)
        {
            Collider[] aColliders = target.GetComponentsInChildren<Collider>();
            foreach (Collider c in aColliders)
            {
                c.enabled = !isSelected;
            }
            MonoBehaviour[] aComponents = target.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour c in aComponents)
            {
                c.enabled = !isSelected;
            }

            Renderer[] aRenderers = target.GetComponentsInChildren<Renderer>();
            foreach (Renderer c in aRenderers)
            {
                c.enabled = !isSelected;
            }
        }

        foreach (GameObject target in objectsToEnableOnSelect)
        {
            EnableOrDisableObjectByComponent(target.transform, true);
        }

    }

    void EnableOrDisableObjectByComponent(Transform target, bool firstTarget)
    {
        //UIFixNestedEnableState fnes = target.GetComponent<UIFixNestedEnableState>();
        //if (fnes != null)
        //{
        //    // set select state if this is the direct target of the button, and the button was hit directly (isn't being forced off)
        //    if (firstTarget && !bForceDeselection)
        //        fnes.isSelected = isSelected;

        //    // check whether enable request should be ignored
        //    if (isSelected && !fnes.isSelected)
        //        return;
        //}

        // enable/disable
        Collider[] aColliders = target.GetComponents<Collider>();
        foreach (Collider c in aColliders)
        {
            c.enabled = isSelected;
        }
        MonoBehaviour[] aComponents = target.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in aComponents)
        {
            c.enabled = isSelected;
        }

        Renderer[] aRenderers = target.GetComponents<Renderer>();
        foreach (Renderer c in aRenderers)
        {
            c.enabled = isSelected;
        }

        foreach (Transform child in target.transform)
        {
            EnableOrDisableObjectByComponent(child, false);
        }
    }

    /// <summary>
    /// Whether the button should be enabled.
    /// </summary>
    public bool isEnabled
    {
        get
        {
            Collider col = GetComponent<Collider>();
            return col && col.enabled;
        }
        set
        {
            Collider col = GetComponent<Collider>();
            if (!col) return;

            if (col.enabled != value)
            {
                col.enabled = value;
                if (value)
				{
                    OnPress(false);
					RefreshScale();
				}
                else
                    UpdateColor(false, true);
            }
        }
    }

    /// <summary>
    /// Update the button's color to either enabled or disabled state.
    /// </summary>
    public void UpdateColor(bool shouldBeEnabled, bool immediate)
    {
        if (!mInitDone)
            Init(true, true);

        if (tweenTargets.Length > 0)
        {
            for (int i = 0; i < tweenTargets.Length; i++)
            {
                if (tweenTargets[i])
                {
					Color c = shouldBeEnabled ? (isSelected ? pressed : mColors[i]) : disabledColor;
                    TweenColor tc = TweenColor.Begin(tweenTargets[i], 0.15f, c);

                    if (immediate)
                    {
                        tc.value = c;
                        tc.enabled = false;
                    }
                }
            }
        }
        if (additionalTweenTarget)
        {
			Color c = shouldBeEnabled ? (isSelected ? secondaryPressed : mSecondaryColor) : secondaryDisabledColor;
            TweenColor tc = TweenColor.Begin(additionalTweenTarget, 0.15f, c);

            if (immediate)
            {
                tc.value = c;
                tc.enabled = false;
            }
        }
    }
}
