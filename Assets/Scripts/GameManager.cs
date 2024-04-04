using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager board;
    public Color[] playersColor;
    public List<Player> players;
    private int nextPlayer;

    private Vector2[] direct2D = { Vector2.up, Vector2.down, Vector2.right, Vector2.left }; 
    // Start is called before the first frame update
    void Start()
    {
        board.AddPlayer += AddPlayer;
        board.GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            MovePlayer(0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MovePlayer(1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MovePlayer(2);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MovePlayer(3);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            BomberPlayer();
        }
    }

    private void MovePlayer(int dir)
    {
        Player player = CurrentPlayer();
        Vector2 pos2D = new Vector2(player.transform.position.x, player.transform.position.y);
        GameObject square = board.GetAtPosition(pos2D + direct2D[dir]);
        if(!square.tag.Equals("Wall"))
        {
            if (square.transform.childCount==0 
                || square.transform.GetChild(0).tag.Equals("Cherrie"))
            player.Move(square.transform, dir);
        }
    }

    private void BomberPlayer()
    {
        CurrentPlayer().Bomber();
    }

    private Player CurrentPlayer()
    {
        int currentPlayer = nextPlayer;
        nextPlayer = (currentPlayer + 1) % players.Count;
        return players[currentPlayer];
    }

    private void AddPlayer(object sender, EventArgs e)
    {
        Player player = (Player)sender;
        player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
        players.Add(player);
    }
}
