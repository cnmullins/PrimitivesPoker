#define DEBUG_MODE
#if DEBUG_MODE && UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public struct Scenario {
    public Scenario(in int sceneID=-1) {
        switch (sceneID) {
            default:
                pot = 0;
                smallBlind = 10;
                bigBlind = 20;
                break;
        }
    }
    public uint pot;
    public uint smallBlind;
    public uint bigBlind;
}

public static class DebugTools {
    public static void ScoreToTextFile(Card[] comCards, params BasePlayer[] players) {
        // use Application.dataPath check for "DebuggingFiles" folder
        string path = Application.dataPath + "\\Scripts\\DebugFiles\\";
        int fileID = 0;
        string[] debugFiles = Directory.EnumerateFileSystemEntries(path) as string[];
        if (debugFiles != null) {
            fileID = debugFiles.Where(s => s.Contains("Debug_WinFile")).Count();
            //file cleanup
            for (int i = 0; i < debugFiles.Length; ++i) {
                DateTime createDate = File.GetCreationTime(path + debugFiles[i]);
                if (DateTime.Now - createDate > new TimeSpan(12, 0, 0)) {
                    Debug.Log("file successfully deleted!");
                    File.Delete(path + debugFiles[i]);
                }
            }
        }
        //figure out path and text file naming scheme
        //CHECK FOR DUPLICATE NAMES AND FIGURE OUT A WAY TO AUTOMATE CLEANING
        //formating as follows
        //cur date/time
        string textFileStr = string.Empty;
        //textFileStr = DateTime.Now.ToLongDateString() + "\n\n";
        textFileStr += "CommunityCards:\n";
        //community cards: 
            //card 1
            //card 2
            //etc.
        for (int i = 0; i < comCards.Length; ++i) {
            textFileStr += '\t' + comCards[i].ToShortString() + ":\t" + comCards[i].ToString() + '\n';
        }
        //playername (add asterisks to the winners name)
            //score
            //card 1, card 2
        textFileStr += "\nPlayers/Bots:\n";
        for (int i = 0; i < players.Length; ++i) {
            int score = Card.ScoreHand(comCards, players[i].hand);
            textFileStr += '\t' + players[i].playerName + ": " + score + '\t'
                + players[i].hand[0].ToShortString() + ", " 
                + players[i].hand[1].ToShortString() + '\n';
        }
        File.WriteAllText(path + "Debug_WinFile_" + fileID, textFileStr);
        UnityEngine.Debug.Log("file written successfully");

    }
    public static void InstantiateScenerio(in Scenario s) {

    }
}
#endif