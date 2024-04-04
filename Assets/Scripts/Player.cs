using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    public int rangeBombe {  get; set; }
    private string[] move = { "Up", "Down", "Right", "Left" };

    public void Move(Transform parent, int dir)
    {
        animator.SetTrigger(move[dir]);
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0,0,-1);
    }

    public void Bomber()
    {
        Debug.Log("Bombe");
    }
}
