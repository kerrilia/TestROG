using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Timers;
using System.Linq;

public struct Player
{
    public int Id;
    public int Power;
}

[Serializable]
public struct MatchmakingOptions
{
    public int PartySize;
    public int DefaultRange;
    public int RangeIncreaseTime;
    public int RangeIncrease;
    public int MatchingTime;
}

public class PlayerMatch
{
    public Player PlayerData;
    public int TimeWait;
    public bool isMatch;
}

public class MatchMaking 
{
    public Action<Player[]> OnMatched;
    public Action<Player> OnCanceled;
    private static Timer aTimer;

    public List<PlayerMatch> ListWaitingPlayers = new List<PlayerMatch>();

    private MatchmakingOptions _options;

    public MatchMaking(MatchmakingOptions options)
    {
        _options = options;
        SetTimer();
        aTimer.Start();
    }

    public void AddRequest(Player player)
    {
        var playerAdded = ListWaitingPlayers.Find(f => f.PlayerData.Id == player.Id);
        if (playerAdded == null)
        {
            ListWaitingPlayers.Add(new PlayerMatch() { PlayerData = player, TimeWait = 0 });
        }
        else
        {
            ListWaitingPlayers.Remove(playerAdded);
            throw new ArgumentException(string.Format("Player {0} already exist", player.Id));
        }
    }

    public void CancelRequest(Player player)
    {
        var playerCancel = ListWaitingPlayers.Find(f => f.PlayerData.Id == player.Id);

        if (playerCancel != null)
        {
            if(OnCanceled != null)
                OnCanceled(player);

            ListWaitingPlayers.Remove(playerCancel);
        }
        else
        {
            throw new ArgumentException(string.Format("Player {0} not found", player.Id));
        }
    }

    public void Update(int time)
    {
        List<Party> parties = new List<Party>() { };

        foreach (var item in ListWaitingPlayers)
        {
            if (item.TimeWait > _options.MatchingTime)
                continue;

            float intervalPowerPlayer = item.TimeWait * (_options.RangeIncrease + _options.DefaultRange) * 0.5f;
            int powerPlayerMax = item.PlayerData.Power + Mathf.CeilToInt(intervalPowerPlayer);
            int powerPlayerMin = item.PlayerData.Power - Mathf.FloorToInt(intervalPowerPlayer);

            UpdateParties(parties, item);

            if (!item.isMatch)
                parties.Add(new Party() { players = new List<PlayerMatch>() { item }, MaxPower = powerPlayerMax, MinPower = powerPlayerMin });

            item.TimeWait += time;
        }

        RemovePlayers();
    }

    private void UpdateParties(List<Party> parties, PlayerMatch item)
    {
        foreach (var party in parties)
        {
            if (party.players.Count < _options.PartySize)
            {
                if (item.PlayerData.Power <= party.MaxPower && item.PlayerData.Power >= party.MinPower)
                    party.players.Add(item);

                if (party.players.Count == _options.PartySize)
                {
                    foreach (var player in party.players)
                        player.isMatch = true;

                    if(OnMatched != null)
                        OnMatched(party.players.Select(f => f.PlayerData).ToArray());
                }
            }
        }

        parties.RemoveAll(f => f.players.Count == _options.PartySize);
    }

    private void RemovePlayers()
    {
        List<PlayerMatch> canceledPlayers = new List<PlayerMatch>();
        List<PlayerMatch> matchedPlayers = new List<PlayerMatch>();

        foreach (var item in ListWaitingPlayers)
        {
            if (item.isMatch)
            {
                matchedPlayers.Add(item);
            }
            else if (item.TimeWait > _options.MatchingTime)
            {
                canceledPlayers.Add(item);
            }
        }

        foreach (var m in matchedPlayers)
        {
            ListWaitingPlayers.Remove(m);
        }

        foreach (var c in canceledPlayers)
        {
            CancelRequest(c.PlayerData);
        }
    }

    private void SetTimer()
    {
        aTimer = new Timer(_options.RangeIncreaseTime * 1000);//перевод в миллисекунды
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        Update(_options.RangeIncreaseTime);
    }

    public void Stop()
    {
        aTimer.Stop();
        aTimer.Dispose();
    }
}

public class Party
{
    public List<PlayerMatch> players;
    public int MinPower;
    public int MaxPower;
}
