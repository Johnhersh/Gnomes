using UnityEngine;

public class PositionRendererSorterStatic : MonoBehaviour
{
    [SerializeField] private int _sortingOrderBase = 5000;
    [SerializeField] private int _offset = 0;

    private Renderer _myRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
        _myRenderer.sortingOrder = (int)(_sortingOrderBase - transform.position.y - _offset);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
