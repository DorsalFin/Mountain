using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    public Camera playerMainCamera;
    public float targetFaceAngle;
    public float rotationSpeed = 20.0f;

    private Player _player;
    private Vector3 _lookAt;
    private int _rotateDirection = 0;

    void Awake()
    {
        _player = GetComponent<Player>();
        // cache the initial look at vector using camera's height
        _lookAt = new Vector3(0, playerMainCamera.transform.position.y, 0);
    }

    public void ChangeFaceFocus(int direction)
    {
        _rotateDirection = direction;
        string newFace = Mountain.Instance.GetNextFaceInDirection(_player.closestTile.face, direction);
        targetFaceAngle = Mountain.Instance.GetRotationValueForFace(newFace);
        Mountain.Instance.SetFaceVisibility(newFace);
        _player.currentFace = newFace;
    }

    void LateUpdate()
    {
        // if we aren't quite there yet
        if (!RoughlyEqual(playerMainCamera.transform.rotation.eulerAngles.y, targetFaceAngle, targetFaceAngle == 0 ? 0.75f : 0.5f))
        {
            float currentRotationSpeed = _rotateDirection == Mountain.LEFT ? rotationSpeed * Time.deltaTime : -rotationSpeed * Time.deltaTime;
            playerMainCamera.transform.position = RotatePointAroundMountain(playerMainCamera.transform.position, Quaternion.Euler(0, currentRotationSpeed, 0));
            playerMainCamera.transform.LookAt(_lookAt);
        }
    }

    Vector3 RotatePointAroundMountain(Vector3 point, Quaternion angle)
    {
        Vector3 pivot = Vector3.zero; // the mountain is at vector3 zero
        return angle * (point - pivot) + pivot;
    }

    /// <summary>
    /// checks if any two floats are 'roughly' equal to within a given threshold
    /// </summary>
    /// <param name="a">float a</param>
    /// <param name="b">float b</param>
    /// <param name="epsilon">the threshold to use when checking if they are close enough / defaults to 0.5</param>
    bool RoughlyEqual(float a, float b, float epsilon = 0.5f)
    {
        return (Mathf.Abs(a - b) < epsilon);
    }
}
