using System;
using UnityEngine;

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
            case PlaygroundState.Draw:
                OnDraw();
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
        penalizedPlayer.AddReward(-10f);
        penalizedPlayer.EndEpisode();
        anotherPlayer.EndEpisode();
        //Academy.Instance.EnvironmentStep();
        EndGame();
    }

    private void OnWin(PlayerAgent winner, PlayerAgent looser)
    {
        winner.AddReward(1f);
        looser.AddReward(-1f);
        winner.EndEpisode();
        looser.EndEpisode();
        //Academy.Instance.EnvironmentStep();
        EndGame();
    }

    private void OnDraw()
    {
        bluePlayer.AddReward(0.2f);
        redPlayer.AddReward(0.2f);
        bluePlayer.EndEpisode();
        redPlayer.EndEpisode();
        //Academy.Instance.EnvironmentStep();
        EndGame();
    }

    private void StartGame()
    {
        playground.StartGame();
        bluePlayer.Reset();
        redPlayer.Reset();
        _turnTeam = Team.Red;
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

    private void EndGame()
    {
        playground.EndGame();
        _nextStep += stepDuration;
    }
}