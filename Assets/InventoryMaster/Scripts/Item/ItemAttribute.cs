using UnityEngine;
using System.Collections;

[System.Serializable]
public class ItemAttribute
{

    public string attributeName;
    public float attributeValue;
    public ItemAttribute(string attributeName, float attributeValue)
    {
        this.attributeName = attributeName;
        this.attributeValue = attributeValue;
    }

    public ItemAttribute() { }

}

