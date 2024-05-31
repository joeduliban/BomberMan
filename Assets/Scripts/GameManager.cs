using System;
using System.Collections;
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
        if (!isGameOver() && !CurrentPlayer().isIA)
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
        if (player == null) return;

        player.NextParty();
        Vector2 pos2D = new Vector2(player.transform.position.x, player.transform.position.y);
        GameObject square = board.GetAtPosition(pos2D + direct2D[dir]);

        if (square != null && !square.CompareTag("Wall"))
        {
            if (square.transform.childCount == 0)
                player.Move(square.transform, dir);
            else if (square.transform.GetChild(0).CompareTag("Cherrie"))
            {
                player.Move(square.transform, dir);
                Cherrie cherrie = square.GetComponentInChildren<Cherrie>();
                player.rangeBombe += cherrie.bonusRange;
                Destroy(cherrie.gameObject);
            }
        }
    }

    private void BomberPlayer()
    {
        Bombe bb = CurrentPlayer()?.Bomber();
        if (bb != null)
            bb.Explosion += exposionBombe;
    }

    private void ApplyAction(Action action, bool ia = false)
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

        if (!ia)
        {
            ChangeCurrentPlayer();
            if (CurrentPlayer().isIA)
            {
                StartCoroutine(ExecuteAIAction());
            }
        }
    }

    public Player CurrentPlayer()
    {
        return players[currentPlayer];
    }


    private void ChangeCurrentPlayer()
    {
        if (players == null || players.Count == 0) return;
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    private void AddPlayer(object sender, EventArgs e)
    {
        Player player = (Player)sender;
        if (player != null)
        {
            player.GetComponent<SpriteRenderer>().color = playersColor[players.Count];
            players.Add(player);
        }
    }

    private void exposionBombe(object sender, EventArgs e)
    {
        Bombe bombe = (Bombe)sender;
        if (bombe == null) return;

        Vector2 pos2D = new Vector2(bombe.transform.position.x, bombe.transform.position.y);
        foreach (Vector2 direction in direct2D)
        {
            for (int i = 0; i <= bombe.range; i++)
            {
                GameObject go = board.GetAtPosition((i * direction) + pos2D);
                if (go == null) continue;

                if (!go.CompareTag("Wall"))
                {
                    for (int j = 0; j < go.transform.childCount; j++)
                    {
                        Transform child = go.transform.GetChild(j);
                        if (!child.CompareTag("Cherrie"))
                        {
                            if (child.CompareTag("Player"))
                            {
                                players.Remove(child.GetComponent<Player>());
                            }
                            Destroy(child.gameObject);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    private IEnumerator ExecuteAIAction()
    {
        yield return new WaitForSeconds(0.5f); // Adjust this delay as needed
        ApplyAction(GetAIAction(), true);
        ChangeCurrentPlayer();
    }

    public Action GetAIAction()
    {
        Action bestAction = Action.Up; // Default action
        int bestValue = int.MinValue;

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            ApplyAction(action, true);

            int boardValue = Minimax(action, 3, false, int.MinValue, int.MaxValue); // Depth, isMaximizingPlayer, alpha and beta might need adjustment

            if (boardValue > bestValue)
            {
                bestValue = boardValue;
                bestAction = action;
            }
        }
        return bestAction;
    }

    // Implémentation récursive de l'algorithme Minimax
    private int Minimax(Action action, int depth, bool isMaximizingPlayer, int alpha, int beta)
    {
        // Cloner le plateau et les joueurs
        List<Player> originalPlayers = players;
        players = new List<Player>();
        var originalBoard = board.board;
        var clonedBoard = board.CloneBoard();
        board.board = clonedBoard; // Use the cloned board

        if (depth == 0 || isGameOver())
        {
            int score = EvaluateBoard();
            // Nettoyer les clones et restaurer le plateau original
            board.board = originalBoard;
            players = originalPlayers;
            board.CleanupClones(clonedBoard);
            return score;
        }

        // Appliquer l'action sur le plateau de jeu cloné
        ApplyAction(action, true);

        int evaluation;
        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                board.board = clonedBoard; // Use the cloned board for the next action
                maxEval = Math.Max(maxEval, Minimax(nextAction, depth - 1, false, alpha, beta));
                alpha = Math.Max(alpha, maxEval);
                if (beta <= alpha)
                {
                    break; // Beta cut-off
                }
            }
            evaluation = maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Action nextAction in Enum.GetValues(typeof(Action)))
            {
                board.board = clonedBoard; // Use the cloned board for the next action
                minEval = Math.Min(minEval, Minimax(nextAction, depth - 1, true, alpha, beta));
                beta = Math.Min(beta, minEval);
                if (beta <= alpha)
                {
                    break; // Alpha cut-off
                }
            }
            evaluation = minEval;
        }

        // Nettoyer les clones et restaurer le plateau original
        board.board = originalBoard;
        players = originalPlayers;
        board.CleanupClones(clonedBoard);

        return evaluation;
    }

    // Méthode pour évaluer l'état actuel du plateau de jeu
    private int EvaluateBoard()
    {
        int score = 0;
        Player aiPlayer = CurrentPlayer();
        if (aiPlayer == null)
        {
            Debug.LogError("AI Player is null.");
            return score;
        }
        Vector2 aiPosition = new Vector2(aiPlayer.transform.position.x, aiPlayer.transform.position.y);

        foreach (Player player in players)
        {
            if (player == null)
            {
                Debug.LogWarning("Encountered a null player in the players list.");
                continue;
            }

            Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

            if (player == aiPlayer)
            {
                // AI Player
                score += 100; // Base points for AI player
                score += aiPlayer.rangeBombe * 5; // Bonus for bomb range

                // Proximity to cherries
                foreach (Vector2 direction in direct2D)
                {
                    Vector2 targetPos = aiPosition + direction;
                    GameObject square = board.GetAtPosition(targetPos);
                    if (square != null && square.CompareTag("Cherrie"))
                    {
                        score += 20; // Bonus for proximity to a cherry
                    }
                }

                // Malus for proximity to bombs
                foreach (Vector2 direction in direct2D)
                {
                    Vector2 targetPos = aiPosition + direction;
                    GameObject square = board.GetAtPosition(targetPos);
                    if (square != null && square.CompareTag("Bombe"))
                    {
                        score -= 10; // Malus for proximity to a bomb
                    }
                }
            }
            else
            {
                // Other players
                score += 50; // Base points for other players (indicating survival)

                // Proximity to AI player
                float distance = Vector2.Distance(aiPosition, playerPosition);
                score -= (int)(distance * 5); // Malus for being far from an opponent

                // Check if AI can trap the opponent
                foreach (Vector2 direction in direct2D)
                {
                    Vector2 targetPos = playerPosition + direction;
                    GameObject square = board.GetAtPosition(targetPos);
                    if (square != null && (square.CompareTag("Wall") || square.CompareTag("Box")))
                    {
                        score += 10; // Bonus for trapping opportunities
                    }
                }
            }
        }

        // Evaluate board state for open spaces and obstacles
        foreach (var kvp in board.board)
        {
            GameObject go = kvp.Value;
            if (go != null && go.CompareTag("Box"))
            {
                score += 5; // Bonus for potential destruction of boxes
            }
            else if (go != null && go.CompareTag("Bombe"))
            {
                score -= 5; // Malus for each bomb present
            }
        }

        return score;
    }

    private bool isGameOver()
    {
        return players.Count <= 1;
    }
}
