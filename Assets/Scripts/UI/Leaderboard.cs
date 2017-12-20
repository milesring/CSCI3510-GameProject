using System;
using System.Linq;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour {
    public SortedList<int,string> scores;

    GameObject leaderboardPanel;
	// Use this for initialization
	void Start () {
        leaderboardPanel = GameObject.FindGameObjectWithTag("Leaderboard");
        leaderboardPanel.SetActive(false);
        scores = new SortedList<int, string>();
        ReadScores();
        UpdateLeaderboard();
	}

    void ReadScores()
    {
        string[] tokens = { "", "" };
        string[] lines = System.IO.File.ReadAllLines("Assets/Scripts/UI/HighScores.txt");
        foreach ( string line in lines)
        {
            tokens = line.Split(' ');
            int score;
            if (Int32.TryParse(tokens[1], out score))
            {
                scores.Add(score, tokens[0]);
            }
        }
    }

    public bool UpdateLeaderboard(int newScore = 0 )
    {
        if(newScore==0)
        {
            leaderboardPanel.transform.GetChild(1).GetComponent<Text>().text = scores.ElementAt(4).Value + ' ' + scores.ElementAt(4).Key;
            leaderboardPanel.transform.GetChild(2).GetComponent<Text>().text = scores.ElementAt(3).Value + ' ' + scores.ElementAt(3).Key;
            leaderboardPanel.transform.GetChild(3).GetComponent<Text>().text = scores.ElementAt(2).Value + ' ' + scores.ElementAt(2).Key;
            leaderboardPanel.transform.GetChild(4).GetComponent<Text>().text = scores.ElementAt(1).Value + ' ' + scores.ElementAt(1).Key;
            leaderboardPanel.transform.GetChild(5).GetComponent<Text>().text = scores.ElementAt(0).Value + ' ' + scores.ElementAt(0).Key;
        }
        else if (newScore > scores.ElementAt(4).Key)
        {
            scores.Remove(scores.ElementAt(4).Key);
            scores.Add(newScore, "Placeholder");
            return true;
        }
        return false;
    }

    public void UpdateLeaderboardName(int newScore, string name)
    {
        if (scores.ContainsKey(newScore))
        {
            scores.Remove(newScore);
            scores.Add(newScore, name);
        }
    }
}
