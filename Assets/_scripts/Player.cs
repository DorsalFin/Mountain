using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public Mountain.Tile tileTarget;
    public float speed;
    public LineRenderer movementLine;
    public string currentSide = "";

    private bool _moving;
    private float _minDistanceToTile = 0.75f;

    void Start()
    {
        tileTarget = Mountain.Instance.GetStartPosition(currentSide);
    }

    void Update()
    {
        if (tileTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, tileTarget.tileTransform.position, speed);
            if (Vector3.Distance(transform.position, tileTarget.tileTransform.position) < _minDistanceToTile && _moving)
            {
                Mountain.Instance.ArrivedAtTile(this, tileTarget);
                _moving = false;
            }
            else
                _moving = true;

            // show line where player is moving
            if (_moving)
            {
                if (!movementLine.gameObject.activeSelf)
                    movementLine.gameObject.SetActive(true);
                movementLine.SetPosition(0, transform.position);
                movementLine.SetPosition(1, tileTarget.tileTransform.position);
            }
            else
            {
                if (movementLine.gameObject.activeSelf)
                    movementLine.gameObject.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                tileTarget = Mountain.Instance.GetClosestTile(currentSide, hit.point);
            }
        }
    }
}