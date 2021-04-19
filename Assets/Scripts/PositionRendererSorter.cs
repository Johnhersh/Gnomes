using UnityEngine;

public class PositionRendererSorter : MonoBehaviour
{
    [SerializeField] private int _sortingOrderBase = 5000;
    [SerializeField] private int _offset = 0;
    [SerializeField] private bool _runOnlyOnce = false;

    private Renderer _myRenderer;

    private void Awake()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        _myRenderer.sortingOrder = (int)(_sortingOrderBase - transform.position.y - _offset);
        if (_runOnlyOnce)
        {
            Destroy(this);
        }
    }
}
