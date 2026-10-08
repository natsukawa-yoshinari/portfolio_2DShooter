using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField]
    private bool _isMouseHorming = false;

    private void Update()
    {
        if (_isMouseHorming == false) return;

        this.gameObject.transform.position = GetMousePosition();
    }

    public Vector3 GetMousePosition()
    {
        Vector3 screenMousePos = Input.mousePosition;
        screenMousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
        worldMousePos.z = 0;
        return worldMousePos;
    }
}
