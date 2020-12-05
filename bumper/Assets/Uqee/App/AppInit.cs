using UnityEngine;
using Uqee.Utility;

public class AppInit : MonoBehaviour {
    private void Awake () {
        AppStatus.isApplicationQuit = false;
    }

    private void Start () {
        ResManager.SetFastLoad (true);
        DontDestroyOnLoad (gameObject);

        OpenGame ();
    }

    public static void OpenGame () {
        //第DataCache.loopCP关为金币关所以需要减去上次吃的金币以免无限吃
        if (SaveData.max_cp % DataCache.loopCP == 0)
        {
            SaveData.gold_num -= SaveData.eatGold;
            SaveData.gold_num = SaveData.gold_num < 0 ? 0 : SaveData.gold_num;
            SaveData.eatGold = 0;
        }

        SceneLoadManager.I.ShowScene (SaveData.curCheckPoint);
        UIManager.I.ShowView<WelcomeView> ();
        SceneLoadManager.I.AddPerfabToSceneInit (() => { EventUtils.Dispatch ("ShowView"); });
        // if (SaveData.curCheckPointNum > DataCache.max_cp)
        // {
        //     SaveData.curCheckPointNum = 1;
        // }
        // System.Random rd = new System.Random ();
        // for (int i = 0; i < 1; i++) {
        //     SceneLoadManager.I.LoadPrefabToScene ($"role_npc0{rd.Next(1, 4)}", "role/", SaveData.curCheckPoint);
        // }
        // for (int i = 0; i < 11; i++) {
        //     ParamStruct info = new ParamStruct ();
        //     info.pos = new Vector3 (rd.Next (-9, 9), -13, rd.Next (123, 215));
        //     info.volumeCoefficient = 1 + (float) rd.Next (1, 10) / 5.0f;
        //     SceneLoadManager.I.LoadPrefabToScene ($"role_npc0{rd.Next(1, 4)}", "role/", SaveData.curCheckPoint, info);
        // }
        // for (int i = 0; i < 11; i++) {
        //     ParamStruct info = new ParamStruct ();
        //     info.pos = new Vector3 (rd.Next (-9, 9), -13, rd.Next (243, 340));
        //     info.volumeCoefficient = 1 + (float) rd.Next (1, 10) / 5.0f;
        //     SceneLoadManager.I.LoadPrefabToScene ($"role_npc0{rd.Next(1, 4)}", "role/", SaveData.curCheckPoint, info);
        // }
        // SceneLoadManager.I.LoadPrefabToScene ("role_skin", "role/", SaveData.curCheckPoint);
        // SceneLoadManager.I.AddPerfabToCurSceneByRounds (DataCache.rounds);
        // #region 测试
        // GameInfoData go = new GameInfoData();
        // go.X = 0;
        // go.Y = 0.2;
        // go.Z = 10;
        // go.EnemyID = "role_npc01";
        // go.Speed = 2;
        // go.Radius = 100;
        // go.Rounds = 1;
        // go.VolumeCoefficient = 1.0;
        // SceneLoadManager.I.LoadPrefabToScene(go.EnemyID, "role/", SaveData.curCheckPoint, go);
        // go = new GameInfoData();
        // go.X = 0;
        // go.Y = 0.2;
        // go.Z = 15;
        // go.EnemyID = "role_npc02";
        // go.Speed = 2;
        // go.Radius = 100;
        // go.Rounds = 1;
        // go.VolumeCoefficient = 1.0;
        // SceneLoadManager.I.LoadPrefabToScene(go.EnemyID, "role/", SaveData.curCheckPoint, go);
        // go = new GameInfoData();
        // go.X = 0;
        // go.Y = 0.2;
        // go.Z = 20;
        // go.EnemyID = "role_npc03";
        // go.Speed = 2;
        // go.Radius = 100;
        // go.Rounds = 1;
        // go.VolumeCoefficient = 1.0;
        // SceneLoadManager.I.LoadPrefabToScene(go.EnemyID, "role/", SaveData.curCheckPoint, go);
        // #endregion
        //SceneLoadManager.I.ShowGameObjectByRounds (DataCache.rounds);
    }
}