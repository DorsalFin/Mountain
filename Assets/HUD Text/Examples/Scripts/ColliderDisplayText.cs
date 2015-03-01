//--------------------------------------------
//            NGUI: HUD Text
// Copyright � 2012 Tasharen Entertainment
//--------------------------------------------

using UnityEngine;

/// <summary>
/// Example script that displays text above the collider when the collider is hovered over or clicked.
/// </summary>

[AddComponentMenu("NGUI/Examples/Collider - Display Text")]
public class ColliderDisplayText : MonoBehaviour
{
	// The UI prefab that is going to be instantiated above the player
	public GameObject prefab;
	public Transform target;
    public object myType;
    public Color textColour;

	HUDText mText = null;
	bool mHover = false;

    private bool _forcedOn;

	// Use this for initialization
	void Start ()
	{
		// We need the HUD object to know where in the hierarchy to put the element
		if (HUDRoot.go == null)
		{
			GameObject.Destroy(this);
			return;
		}

		GameObject child = NGUITools.AddChild(HUDRoot.go, prefab);
		mText = child.GetComponentInChildren<HUDText>();

		// Make the UI follow the target
		child.AddComponent<UIFollowTarget>().target = target;
	}



	void OnHover (bool isOver)
	{
		if (mText != null && isOver && !mHover)
		{
			mHover = true;
            if (!mText.isVisible)
                mText.Add(myType, textColour, 0f);
		}
		else if (!isOver)
		{
            if (!_forcedOn)
            {
                if (mText.isVisible)
                    mText.Clear(0);
            }
			mHover = false;
		}
	}



    /// <summary>
    /// if you want to force the tooltip to be displayed until switched off use this method
    /// </summary>
    public void ToggleForcedState(bool show)
    {
        if (show)
        {
            _forcedOn = true;
            if (!mText.isVisible)
                mText.Add(myType, textColour, 0f);
        }
        else if (show == false)
        {
            _forcedOn = false;
            if (mText.isVisible && !mHover)
                mText.Clear(1.0f);
        }
    }
}
