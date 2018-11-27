using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMatchMaking : MonoBehaviour
{
    public MatchmakingOptions Options;

    public int MyId;
    public int IdPlayerAdded;

    private MatchMaking _matchMaking;

    void Start()
    {
        _matchMaking = new MatchMaking(Options);
        _matchMaking.OnMatched += OnMatchDone;
    }

    public void StartTest()
    {
        //на 100000 подвисает на несколько секунд
        for (int i = 0; i < 10000; ++i)
        {
            _matchMaking.AddRequest(new Player() {Id = i, Power = Random.Range(1, 1000) });
        }
    }

    public void CancelTest()
    {
        _matchMaking.CancelRequest(new Player() { Id = MyId, Power = 0 });
    }

    public void AddedPlayerTest()
    {
        _matchMaking.AddRequest(new Player() { Id = IdPlayerAdded, Power = 0 });
    }

    private void OnDestroy()
    {
        _matchMaking.Stop();
        if(_matchMaking.OnMatched != null)
            _matchMaking.OnMatched -= OnMatchDone;
    }

    private void OnMatchDone(Player[] players)
    {
        foreach (var player in players)
        {
            Debug.LogFormat("PARTY MATCH = {0}; {1}", player.Id, player.Power);
        }
    }
}
