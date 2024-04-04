using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager board;
    public Color[] playersColor;
    private List<Player> players;
    private int nextPlayer;

    private Vector2[] direct2D = { Vector2.up, Vector2.down, Vector2.right, Vector2.left };

    void Start()
    {
        players = new List<Player>();
        board.AddPlayer += AddPlayer;
        board.GenerateGrid();
    }

    void Update()
    {
        if (!isGameOver())
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
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
    }

    private void MovePlayer(int dir)
    {
        Player player = CurrentPlayer();
        Vector2 pos2D = new Vector2(player.transform.position.x, player.transform.position.y);
        GameObject square = board.GetAtPosition(pos2D + direct2D[dir]);
        if(!square.tag.Equals("Wall"))
        {
            if (square.transform.childCount==0)
                player.Move(square.transform, dir);
            else if (square.transform.GetChild(0).tag.Equals("Cherrie"))
            {
                player.Move(square.transform, dir);
                Cherrie cherrie = square.GetComponentInChildren<Cherrie>();
                player.rangeBombe+= cherrie.bonusRange;
                Destroy(cherrie.gameObject);
            }
        }
    }

    private void BomberPlayer()
    {
        Bombe bb = CurrentPlayer().Bomber();
        bb.Explosion += exposionBombe;
    }

    private Player CurrentPlayer()
    {
        int currentPlayer = nextPlayer;
        players[currentPlayer].NextParty();
        nextPlayer = (currentPlayer + 1) % players.Count;
        return players[currentPlayer];
    }

    private void AddPlayer(object sender, EventArgs e)
    {
        Player player = (Player)sender;
        player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
        players.Add(player);
    }

    private void exposionBombe(object sender, EventArgs e)
    {
        Bombe bombe = (Bombe)sender;
        Vector2 pos2D = new Vector2(bombe.transform.position.x, bombe.transform.position.y);
        foreach (Vector2 direction in direct2D)
        {
            for (int i = 1; i <= bombe.range; i++)
            {
                GameObject go = board.GetAtPosition((i * direction) + pos2D);
                for (int j = 0; j < go.transform.childCount && !go.tag.Equals("Wall"); j++)
                {
                    if (!go.transform.GetChild(j).tag.Equals("Cherrie"))
                    {
                        if (go.transform.GetChild(j).tag.Equals("Player"))
                            players.Remove(go.transform.GetComponentInChildren<Player>());
                        Destroy(go.transform.GetChild(j).gameObject);
                    }

                }
            }
        }
    }

    private bool isGameOver()
    {
        return players.Count <= 1;
    }
}
