using System;
using System.Collections.Generic;
using UnityEngine;
using Uqee.Utility;

public class DataCache {
    public static Vector3 curMainPos;
    public static bool mainRoleIsDie = false;
    //当主角倒地时不能受到力的作用
    public static bool mainRoleIsDown = false;
    public static bool startGame = false;
    //当前主角体积
    public static float curMainRoleVolumeCoefficient = 1;
    //当前怪物的波数
    public static int rounds = 1;
    //当前已加载的总波数
    public static int loadedRounds = 0;
    //第rounds波的roundsCount(敌人)数量
    public static int roundsCount = 0;
    public static int restCount = 0;
    public static int loopCP = 5;
    //是否可以吃buff
    public static bool can_eat_buff = true;
    public static bool isWin = false;

    public static Dictionary<Tuple<string, int>, RevivePosInfo> reviveInfo = LoadJson.LoadRevivePos ("RevivePos");

    public static bool InMainSize (Vector3 pos, double radius) {
        var a = Math.Sqrt (Math.Pow ((pos.x - curMainPos.x), 2) + Math.Pow ((pos.z - curMainPos.z), 2));
        return Math.Sqrt (Math.Pow ((pos.x - curMainPos.x), 2) + Math.Pow ((pos.z - curMainPos.z), 2)) <= radius;
    }
}

public class SaveData {
    public static int max_cp {
        set { PlayerPrefs.SetInt ("max_cp", value); } get {
            if (PlayerPrefs.HasKey ("max_cp")) {
                //PlayerPrefs.SetInt ("max_cp", 5);
                return PlayerPrefs.GetInt ("max_cp");
            }
            return 1;
        }
    }
    //public static int max_cp = 1;
    public static int curCheckPointNum {
        set { } get {
            return max_cp % DataCache.loopCP == 0 ? DataCache.loopCP : max_cp % DataCache.loopCP;
        }
    }
    public static int isOpenMusic {
        set { PlayerPrefs.SetInt ("isOpenMusic", value); } get {
            if (PlayerPrefs.HasKey ("isOpenMusic")) {
                return PlayerPrefs.GetInt ("isOpenMusic");
            }
            return 1;
        }
    }

    public static int gold_num {
        set { PlayerPrefs.SetInt ("gold", value); } get {
            if (PlayerPrefs.HasKey ("gold")) {
                return PlayerPrefs.GetInt ("gold");
            }
            return 0;
        }
    }

    public static int eatGold {
        set {
            if (max_cp % DataCache.loopCP == 0)
                PlayerPrefs.SetInt ("eatGold", value);
            else
                PlayerPrefs.SetInt ("eatGold", 0);
        }
        get {
            if (PlayerPrefs.HasKey ("eatGold")) {
                return PlayerPrefs.GetInt ("eatGold");
            }
            return 0;
        }
    }
    public static string curCheckPoint { set { } get { return $"Scene{curCheckPointNum}"; } }
    public static Dictionary<string, List<GameInfoData>> info = LoadJson.LoadSceneJsonFromFile ("SceneInfo");
}