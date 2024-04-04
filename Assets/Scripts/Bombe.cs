using System;
using UnityEngine;

public class Bombe : MonoBehaviour
{
    [SerializeField]
    private Animator animator; 
    private int height;
    public int range {  get; set; }
    public event EventHandler Explosion;

    public void BigBomber()
    {
        height += 1;
        switch (height)
        {
            case 2:
                animator.SetTrigger("Medium");
                break;
            case 4:
                animator.SetTrigger("Big");
                break;
            case 6:
                Destroy(gameObject);
                break;
        }
    }

    private void OnDestroy()
    {
        Explosion(this,EventArgs.Empty);
    }
}
