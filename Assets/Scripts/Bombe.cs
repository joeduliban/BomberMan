using System;
using UnityEngine;

public class Bombe : MonoBehaviour
{
    [SerializeField]
    private Animator animator; 
    private int height;
    public int range {  get; set; }
    public event EventHandler Explosion;

    public bool BigBomber()
    {
        height += 1;
        switch (height)
        {
            case 3:
                animator.SetTrigger("Medium");
                break;
            case 5:
                animator.SetTrigger("Big");
                break;
            case 6:
                Explosion(this, EventArgs.Empty);
                Destroy(gameObject);
                return true;
        }
        return false;
    }
}
