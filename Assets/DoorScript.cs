using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;   // how far door opens
    [SerializeField] private float speed = 2f;         // open/close speed

    private bool _isOpen = false;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openAngle,
            transform.eulerAngles.z
        );
    }

    void Update()
    {
        // Smoothly rotate toward target rotation
        Quaternion target = _isOpen ? _openRotation : _closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * speed);
    }

    /// <summary>Called by Toggle Door button in CleanRoomManager</summary>
    public void ToggleDoor()
    {
        _isOpen = !_isOpen;
    }
}