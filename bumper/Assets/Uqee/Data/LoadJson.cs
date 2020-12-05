using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System;
using System.IO;

public class LoadJson {
    public static Dictionary<string, List<GameInfoData>> LoadSceneJsonFromFile (string json_name) {
        string text =  GetTextForStreamingAssets(json_name);
        JsonData jsonInfo = JsonMapper.ToObject(text);
        Dictionary<string, List<GameInfoData>> game_info = new Dictionary<string, List<GameInfoData>>();
        foreach (var item in jsonInfo)
        {
            var game_info_part1 = (KeyValuePair<string, JsonData>)item;
            List<GameInfoData> game_info_part3 = new List<GameInfoData>();
            foreach (JsonData game_info_part2 in game_info_part1.Value)
            {
                GameInfoData info = new GameInfoData();
                info.EnemyID = (string)game_info_part2["EnemyID"];
                info.X = (float)((double)game_info_part2["X"]);
                info.Y = (float)((double)game_info_part2["Y"]);
                //UnityEngine.Debug.LogError(game_info_part1.Key + ", " +info.EnemyID + ", "+info.X + ", "+info.Y);
                info.Z = (float)((double)game_info_part2["Z"]);
                info.Speed = (float)((double)game_info_part2["Speed"]);
                info.VolumeCoefficient = (float)((double)game_info_part2["VolumeCoefficient"]);
                info.Radius = (float)((double)game_info_part2["Radius"]);
                info.Rounds = (int)game_info_part2["Rounds"];
                game_info_part3.Add(info);
            }
            game_info[game_info_part1.Key] = game_info_part3;
        }
        return game_info;
    }

    public static Dictionary<Tuple<string, int>, RevivePosInfo> LoadRevivePos(string json_name)
    {
        string text =  GetTextForStreamingAssets(json_name);
        JsonData jsonInfo = JsonMapper.ToObject(text);
        Dictionary<Tuple<string, int>, RevivePosInfo> game_info = new Dictionary<Tuple<string, int>, RevivePosInfo>();

        foreach (var item in jsonInfo)
        {
            var game_info_part1 = (KeyValuePair<string, JsonData>)item;
            RevivePosInfo game_info_part3 = new RevivePosInfo();
            foreach (JsonData game_info_part2 in game_info_part1.Value)
            {
                var round = (int)game_info_part2["Rounds"];
                game_info_part3.X = (float)((double)game_info_part2["X"]);
                game_info_part3.Y = (float)((double)game_info_part2["Y"]);
                game_info_part3.Z = (float)((double)game_info_part2["Z"]);
                game_info.Add(new Tuple<string, int>(game_info_part1.Key, round), game_info_part3);
            }
        }
        return game_info;
    }

    //读取StreamingAssets中的文件   参数 StreamingAssets下的路径
    public static string GetTextForStreamingAssets(string json_name)
    {
        string localPath = string.Format("{0}{1}{2}{3}", Application.streamingAssetsPath, "/Json/", json_name, ".json");
    
    #if !UNITY_ANDROID || UNITY_EDITOR
        localPath = "file:///" + localPath;
    #endif

        WWW www = new WWW(localPath);
        if (www.error != null)
        {
            Debug.LogError("error : " + localPath);
            return "";          //读取文件出错
        }
        while(!www.isDone) {}
        return www.text;
    }
}