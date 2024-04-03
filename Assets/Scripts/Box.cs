using UnityEngine;

public class Box : MonoBehaviour
{
    [SerializeField]
    private Cherrie cherriePrefab;

    private bool cherry;
    public bool cherrie { get { return cherry; } set { cherry = value; } }

    private void OnDestroy()
    {
        if (cherrie)
            Instantiate(cherriePrefab, transform.parent);    
    }
}
