using System;
using System.Net;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayBox : MonoBehaviour
{
    [SerializeField] private Playground playground;
    [SerializeField] private PlayerAgent bluePlayer;
    [SerializeField] private PlayerAgent redPlayer;
    [SerializeField] private float stepDuration = 2f;

    private Team _turnTeam;
    private float _nextStep;
    
    private void Update()
    {
        if (Time.time >= _nextStep)
        {
            _nextStep = Time.time + stepDuration;
            NextStep();
        }
    }

    private void NextStep()
    {
        switch (playground.State)
        {
            case PlaygroundState.None:
                StartGame();
                break;
            case PlaygroundState.Playing:
                MakeMove();
                break;
            case PlaygroundState.BlueWins:
                OnWin(bluePlayer, redPlayer);
                break;
            case PlaygroundState.RedWins:
                OnWin(redPlayer, bluePlayer);
                break;
            case PlaygroundState.BlueMadeInvalidMove:
                OnInvalidMove(bluePlayer, redPlayer);
                break;
            case PlaygroundState.RedMadeInvalidMove:
                OnInvalidMove(redPlayer, bluePlayer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MakeMove()
    {
        if (GetPlayer(_turnTeam).IsWaitingAction)
        {
            return;
        }
        
        _turnTeam = _turnTeam switch
        {
            Team.Blue => Team.Red,
            Team.Red => Team.Blue,
            _ => throw new ArgumentOutOfRangeException(nameof(_turnTeam), _turnTeam, null)
        };
        
        GetPlayer(_turnTeam).MakeMove();
    }

    private void OnInvalidMove(PlayerAgent penalizedPlayer, PlayerAgent anotherPlayer)
    {
        penalizedPlayer.AddReward(-10);
        penalizedPlayer.EndEpisode();
        anotherPlayer.EndEpisode();
        playground.EndGame();
        Academy.Instance.EnvironmentStep();
    }

    private void OnWin(PlayerAgent winner, PlayerAgent looser)
    {
        winner.AddReward(1);
        looser.AddReward(-1);
        winner.EndEpisode();
        looser.EndEpisode();
        playground.EndGame();
        Academy.Instance.EnvironmentStep();
    }

    private void StartGame()
    {
        playground.StartGame();
        bluePlayer.Reset();
        redPlayer.Reset();
        _turnTeam = Random.Range(0, 2) == 0 ? Team.Blue : Team.Red;
    }

    private PlayerAgent GetPlayer(Team team)
    {
        return team switch
        {
            Team.Blue => bluePlayer,
            Team.Red => redPlayer,
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, null)
        };
    }
}