using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Bombe bombePrefab;

    public bool isIA;
    public int rangeBombe { get; set; }
    public List<Bombe> bombes;
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
        transform.localPosition = new Vector3(0, 0, -1);
    }

    public void NextParty()
    {
        int i = 0;
        while (i < bombes.Count)
        {
            if (bombes[i].BigBomber())//problème ici
                bombes.RemoveAt(i);
            else
                i++;
        }
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
        // Animation Dead
        // Debug.Log("Dead");
    }
}
