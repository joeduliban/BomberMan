using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    protected GridManager board;
    public Color[] playersColor;
    protected List<Player> players;
    protected int currentPlayer;
    private AIManager aiManager;

    protected Vector2[] direct2D = { Vector2.up, Vector2.down, Vector2.right, Vector2.left };

    private void Start()
    {
        Init();
    }

    protected virtual void Init(GridManager grid = null)
    {
        players = new List<Player>();
        board.AddPlayer += AddPlayer;
        board.GenerateGrid();
        aiManager = gameObject.AddComponent<AIManager>();
    }

    private void Update()
    {
        eventAction();
    }

    protected virtual void eventAction()
    {
        if (!isGameOver())
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ApplyAction(Action.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ApplyAction(Action.Down);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ApplyAction(Action.Right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ApplyAction(Action.Left);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyAction(Action.PlaceBomb);
            }
        }
    }

    protected void MovePlayer(int dir)
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

    protected void BomberPlayer()
    {
        Bombe bb = CurrentPlayer().Bomber();
        if(bb != null)
            bb.Explosion += exposionBombe;
    }

    protected virtual void ApplyAction(Action action)
    {
        switch (action)
        {
            case Action.Up:
                MovePlayer(0);
                break;
            case Action.Down:
                MovePlayer(1);
                break;
            case Action.Right:
                MovePlayer(2);
                break;
            case Action.Left:
                MovePlayer(3);
                break;
            case Action.PlaceBomb:
                BomberPlayer();
                break;
        }
        ChangeCurrentPlayer();
        if (CurrentPlayer().isIA)
        {
            //ApplyAction(aiManager.GetAIAction());
        }
    }

    protected Player CurrentPlayer()
    {
        return players[currentPlayer];
    }

    protected void ChangeCurrentPlayer()
    {
        CurrentPlayer().NextParty();
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    protected void AddPlayer(object sender, EventArgs e)
    {
        Player player = (Player)sender;
        player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
        players.Add(player);
    }

    protected void exposionBombe(object sender, EventArgs e)
    {
        Bombe bombe = (Bombe)sender;
        Vector2 pos2D = new Vector2(bombe.transform.position.x, bombe.transform.position.y);
        foreach (Vector2 direction in direct2D)
        {
            for (int i = 0; i <= bombe.range; i++)
            {
                GameObject go = board.GetAtPosition((i * direction) + pos2D);
                if(!go.tag.Equals("Wall"))
                    for (int j = 0; j < go.transform.childCount; j++)
                    {
                        if (!go.transform.GetChild(j).tag.Equals("Cherrie"))
                        {
                            if (go.transform.GetChild(j).tag.Equals("Player"))
                                players.Remove(go.transform.GetComponentInChildren<Player>());
                            Destroy(go.transform.GetChild(j).gameObject);
                        }

                    }
                else
                    break;
            }
        }
    }
    protected bool isGameOver()
    {
        return players.Count <= 1;
    }
}
