using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Bombe bombePrefab;
    public int rangeBombe {  get; set; }
    private List<Bombe> bombes;
    private string[] move = { "Up", "Down", "Right", "Left" };

    private void Start()
    {
        bombes = new List<Bombe>();
        rangeBombe = 1;
    }

    public void Move(Transform parent, int dir)
    {
        animator.SetTrigger(move[dir]);
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0,0,-1);
    }

    public void NextParty()
    {
        try
        {
            foreach (Bombe bb in bombes)
                try
                {
                    bb.BigBomber();
                }
                catch (Exception e) { bombes.Remove(bb); }
        }
        catch (Exception e) { }
    }

    public Bombe Bomber()
    {
        if (transform.parent.childCount <= 1)
        {
            Bombe bb = Instantiate(bombePrefab, transform.parent);
            bb.range = rangeBombe;
            bombes.Add(bb);
            return bb;
        }
        return null;
    }

    private void OnDestroy()
    {
        //Animation Dead
    }
}
