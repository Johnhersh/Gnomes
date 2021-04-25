using UnityEngine;

public class PositionRendererSorterStatic : MonoBehaviour
{
    [SerializeField] private int _sortingOrderBase = 5000;
    [SerializeField] private int _offset = 0;

    private Renderer _myRenderer;

    // Start is called before the first frame update
    void Start()
    {
        SetSortingOrder();
        SetZPosition();
    }

    /// <summary>
    /// Set this sprite's sorting order based on its Y position in the world
    /// </summary>
    private void SetSortingOrder()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
        _myRenderer.sortingOrder = (int)(_sortingOrderBase - transform.position.y + _offset);
    }

    /// <summary>
    /// Objects with the same sorting order will be sorted based on their Z position
    /// This will move them in 3D space so adjacent objects won't flicker as they're being sorted
    /// </summary>
    private void SetZPosition()
    {
        var myPosition = transform.parent.position;
        myPosition.z = myPosition.x / 10;
        transform.parent.position = myPosition;
    }
}
