using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GridManager board;
    public Color[] playersColor;
    private List<Player> players;
    private int currentPlayer;

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

    private void MovePlayer(int dir)
    {
        Player player = CurrentPlayer();
        player.NextParty();
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
        if(bb != null)
            bb.Explosion += exposionBombe;
    }

    private void ApplyAction(Action action)
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
            ApplyAction(GetAIAction());
        }
    }

    private Player CurrentPlayer()
    {
        return players[currentPlayer];
    }

    private void ChangeCurrentPlayer()
    {
        currentPlayer = (currentPlayer + 1) % players.Count;
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

    private Action GetAIAction()
    {
        int bestScore = int.MinValue;
        Action bestAction = Action.Up;

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            int score = Minimax(action, 3, false); // Profondeur de recherche 3 pour cet exemple
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction;
    }

    // Implémentation récursive de l'algorithme Minimax
    private int Minimax(Action action, int depth, bool isMaximizingPlayer)
    {
        if (depth == 0 || isGameOver())
        {
            return EvaluateBoard();
        }

        ApplyAction(action); // Appliquer l'action sur le plateau de jeu

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                maxEval = Math.Max(maxEval, Minimax(nextAction, depth - 1, false));
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                minEval = Math.Min(minEval, Minimax(nextAction, depth - 1, true));
            }
            return minEval;
        }
    }

    // Méthode pour évaluer l'état actuel du plateau de jeu
    private int EvaluateBoard()
    {
        int score = 0;

        // Parcours du plateau de jeu pour évaluer différentes conditions
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject go = board.GetAtPosition(new Vector2(i, j));
                if (go.tag.Equals("Wall"))
                    score -= 1;
                else
                    for (int k = 0; k < go.transform.childCount; k++)
                    {
                        if(go.transform.GetChild(k).tag.Equals("Cherrie"))
                            score += 20;
                        else if(go.transform.GetChild(k).tag.Equals("Player"))
                            score += 10;
                        else if (go.transform.GetChild(k).tag.Equals("Box"))
                            score -= 1;
                        else if (go.transform.GetChild(k).tag.Equals("Bombe"))
                            score -= 10;
                    }
            }
        }

        return score;
    }

    private bool isGameOver()
    {
        return players.Count <= 1;
    }
}
