using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    public Camera playerMainCamera;
    public Camera playerVectorLineCamera;
    //public float targetFaceAngle;
    //public float rotationSpeed = 20.0f;

    //private Player _player;
    //private Vector3 _lookAt;
    //private int _rotateDirection = 0;

    //private bool _valuesSet = false;

    public Transform cameraTarget;
    float rotateTime = 3.0f;
    float rotateDegrees = 90.0f;
    private bool _rotating = false;

    void Awake()
    {
        //_player = GetComponent<Player>();
        cameraTarget = Mountain.Instance.cameraTarget.transform;
    }

    public void SetStartFaceRotation(string face)
    {
        float degrees = face == "north" ? 0 : face == "west" ? 90 : face == "south" ? 180 : 270; 
        StartCoroutine(Rotate(playerMainCamera.transform, cameraTarget, Vector3.up, degrees, 0.10f));
    }

    /// <summary>
    ///  convenience function that calls Rotate
    /// </summary>
    public void ChangeFace(int direction)
    {
        if (direction == Mountain.LEFT)
        {
            StartCoroutine(Rotate(playerMainCamera.transform, cameraTarget, Vector3.up, rotateDegrees, rotateTime));
        }
        else
        {
            StartCoroutine(Rotate(playerMainCamera.transform, cameraTarget, -Vector3.up, rotateDegrees, rotateTime));
        }
    }
 
    IEnumerator Rotate (Transform thisTransform, Transform otherTransform, Vector3 rotateAxis, float degrees, float totalTime) 
    {
        if (!_rotating)
        {
            _rotating = true;

            Quaternion startRotation = thisTransform.rotation;
            Vector3 startPosition = thisTransform.position;
            playerMainCamera.transform.RotateAround(otherTransform.position, rotateAxis, degrees);
            Quaternion endRotation = thisTransform.rotation;
            Vector3 endPosition = thisTransform.position;
            thisTransform.rotation = startRotation;
            thisTransform.position = startPosition;

            float rate = degrees / totalTime;
            for (float i = 0.0f; i < degrees; i += Time.deltaTime * rate)
            {
                yield return null;
                thisTransform.RotateAround(otherTransform.position, rotateAxis, Time.deltaTime * rate);
            }

            thisTransform.rotation = endRotation;
            thisTransform.position = endPosition;
            _rotating = false;
        }
    }


    //public void SetInitialValue()
    //{
    //    // cache the initial look at vector using camera's height
    //    _lookAt = new Vector3(0, playerMainCamera.transform.position.y, 0);
    //    targetFaceAngle = Mountain.Instance.GetRotationValueForFace(_player.currentFace);
    //    _valuesSet = true;
    //}

    //public void ChangeFaceFocus(int direction)
    //{
    //    _rotateDirection = direction;
    //    string newFace = Mountain.Instance.GetNextFaceInDirection(_player.closestTile.face, direction);
    //    targetFaceAngle = Mountain.Instance.GetRotationValueForFace(newFace);
    //    Mountain.Instance.SetFaceVisibility(newFace);
    //    _player.currentFace = newFace;
    //}

    //void LateUpdate()
    //{
    //    if (!_valuesSet)
    //        return;

    //    // if we aren't quite there yet
    //    if (!RoughlyEqual(playerMainCamera.transform.rotation.eulerAngles.y, targetFaceAngle, targetFaceAngle == 0 ? 0.75f : 0.5f))
    //    {
    //        float currentRotationSpeed = _rotateDirection == Mountain.LEFT ? rotationSpeed * Time.deltaTime : -rotationSpeed * Time.deltaTime;
    //        playerMainCamera.transform.position = RotatePointAroundMountain(playerMainCamera.transform.position, Quaternion.Euler(0, currentRotationSpeed, 0));
    //        playerMainCamera.transform.LookAt(_lookAt);
    //    }
    //}

    //Vector3 RotatePointAroundMountain(Vector3 point, Quaternion angle)
    //{
    //    Vector3 pivot = Vector3.zero; // the mountain is at vector3 zero
    //    return angle * (point - pivot) + pivot;
    //}

    ///// <summary>
    ///// checks if any two floats are 'roughly' equal to within a given threshold
    ///// </summary>
    ///// <param name="a">float a</param>
    ///// <param name="b">float b</param>
    ///// <param name="epsilon">the threshold to use when checking if they are close enough / defaults to 0.5</param>
    //bool RoughlyEqual(float a, float b, float epsilon = 0.5f)
    //{
    //    return (Mathf.Abs(a - b) < epsilon);
    //}
}
