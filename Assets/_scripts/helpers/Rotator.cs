﻿using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public float speed;

	void Update () 
    {
        transform.Rotate(0, 0, Time.deltaTime * speed);
	}
}
