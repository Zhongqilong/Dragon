using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Uqee.Resource;
using Uqee.Utility;

public class SceneLoadManager : Singleton<SceneLoadManager> {
    private Dictionary<string, bool> _allLoadingScene = new Dictionary<string, bool> ();
    private List<GameObject> _poolGo = new List<GameObject> ();
    private Action _loadedCallBack = null;
    //每隔0.5检测一次场景是否加载完成
    private float _seconds = 0.01f;
    private int _loadedCount = 0;
    protected override void Init () { }

    public void ShowScene (string scene_name, Action call_back = null, bool release_scene = false) {
        CoroutineHelper.Start (_LoadingScene (scene_name, call_back, release_scene));
    }

    //初始化某个场景需要将常驻元素添加进来,其中当前场景中常驻元素用Rounds = 0来区分
    public void AddPerfabToSceneInit (Action call_back = null) {
        foreach (var item in SaveData.info[SaveData.curCheckPoint]) {
            if (item.Rounds == 0 || item.Rounds == 1) {
                SceneLoadManager.I.LoadPrefabToScene (item.EnemyID, "GameObject/", SaveData.curCheckPoint, item);
                _loadedCount += 1;
                if (item.Rounds == 1)
                    DataCache.roundsCount += 1;

            }
        }
        DataCache.restCount = (int) ((float) DataCache.roundsCount * 0.4f);
        DataCache.rounds = 1;
        _loadedCallBack = call_back;
    }

    public void LoadPrefabToScene (string prefab_name, string child_path, string scene_name, object param = null) {
        if (_allLoadingScene.ContainsKey (scene_name)) {
            _LoadPrefabToSceneFunc (prefab_name, child_path, scene_name, param);
            return;
        }
        CoroutineHelper.Start (_LoadPrefabToScene (prefab_name, child_path, scene_name, param));
    }

    public Transform GetSceneNodeTrans (string scene_name, string node_name) {
        var cur_scene = SceneManager.GetSceneByName (scene_name);
        if (cur_scene == null) return null;
        return cur_scene.GetRootGameObjects () [0].transform.Find (node_name);
    }

    public void RemoveAllScene () {
        foreach (var scene_name in _allLoadingScene) {
            var cur_scene = SceneManager.GetSceneByName (scene_name.Key);
            SceneManager.UnloadSceneAsync (cur_scene);
        }
        _allLoadingScene.Clear ();
    }

    public void UnloadSceneByName (string scene_name) {
        var cur_scene = SceneManager.GetSceneByName (scene_name);
        if (cur_scene == null) return;
        if (!cur_scene.isLoaded) {
            _allLoadingScene.Remove (scene_name);
            return;
        }
        SceneManager.UnloadSceneAsync (cur_scene);
        _allLoadingScene.Remove (scene_name);
    }

    /// <summary>
    /// 利用当前波数初始化怪物的位置
    /// </summary>
    public void ReStartSceneGame () {
        var cur_scene = SceneManager.GetSceneByName (SaveData.curCheckPoint);
        DataCache.roundsCount = 0;
        if (cur_scene != null) {
            foreach (var item in _poolGo) {
                var gameBaseObj = item.GetComponent<AllObjectBase> ();
                if (gameBaseObj.tag == GameTag.Gold)
                    continue;
                if (gameBaseObj.obFlag == 2) {
                    gameBaseObj.gameObject.SetActive (true);
                    continue;
                }
                if (gameBaseObj.obFlag == 1) {
                    gameBaseObj.RestartGame ();
                    continue;
                }
                var showOrNot = gameBaseObj.rounds == DataCache.rounds;
                if (showOrNot) {
                    DataCache.roundsCount += 1;
                    gameBaseObj.RestartGame ();
                } else if (gameBaseObj.rounds == 0)
                    gameBaseObj.gameObject.SetActive (true);
                else
                    gameBaseObj.gameObject.SetActive (false);
            }
        }
        DataCache.restCount = (int) ((float) DataCache.roundsCount * 0.4f);
    }

    /// <summary>
    /// 按照波数加载对象
    /// </summary>
    /// <param name="rounds">传需要显示的波数</param>
    /// <param name="null_call_back"></param>
    public void ShowGameObjectByRounds (int rounds, Action null_call_back = null) {
        //需求的波数大于加载的波数
        if (rounds > DataCache.loadedRounds) {
            DataCache.loadedRounds = rounds;
            SceneLoadManager.I._AddPerfabToCurSceneByRounds (DataCache.loadedRounds, null_call_back);
            EventUtils.Dispatch ("Start");
            return;
        }

        //已经加载的敌人只需要重新显示即可
        DataCache.roundsCount = 0;
        foreach (var item in _poolGo) {
            var gameBaseObj = item.GetComponent<Enemy> ();
            // if (item.tag == GameTag.Gold)
            // {
            //     gameBaseObj.gameObject.SetActive (true);
            //     continue;
            // }
            if (gameBaseObj != null && gameBaseObj.rounds == rounds) {
                DataCache.roundsCount += 1;
                gameBaseObj.RestartGame();
                gameBaseObj.gameObject.SetActive (true);
                EventUtils.Dispatch ("Start");
            }
        }
        if (DataCache.roundsCount == 0)
        {
            null_call_back?.Invoke();
            DataCache.restCount = 0;
            return;
        }
        DataCache.restCount = (int) ((float) DataCache.roundsCount * 0.4f);
    }

    private void _AddPerfabToCurSceneByRounds (int rounds, Action null_call_back = null) {
        DataCache.roundsCount = 0;
        foreach (var item in SaveData.info[SaveData.curCheckPoint]) {
            // if (item.EnemyID == "Coin")
            // {
            //     SceneLoadManager.I.LoadPrefabToScene (item.EnemyID, "GameObject/", SaveData.curCheckPoint, item);
            //     continue;
            // }
            if (item.Rounds == rounds) {
                DataCache.roundsCount += 1;
                SceneLoadManager.I.LoadPrefabToScene (item.EnemyID, "GameObject/", SaveData.curCheckPoint, item);
            }
        }
        DataCache.restCount = (int) ((float) DataCache.roundsCount * 0.4f);
        if (DataCache.roundsCount == 0) {
            DataCache.restCount = 0;
            null_call_back?.Invoke ();
        }
    }

    /// <summary>
    /// 加载场景必须等到WaitForEndOfFrame结束后场景状态才会变为loading
    /// </summary>
    /// <param name="prefab_name"></param>
    /// <param name="child_path"></param>
    /// <param name="scene_name"></param>
    /// <returns></returns>
    private IEnumerator _LoadPrefabToScene (string prefab_name, string child_path, string scene_name, object param = null) {
        while (true) {
            if (_allLoadingScene.ContainsKey (scene_name)) {
                _LoadPrefabToSceneFunc (prefab_name, child_path, scene_name, param);
                yield break;
            }
            yield return new WaitForSeconds (_seconds);
        }
    }

    private IEnumerator _LoadingScene (string scene_name, Action call_back = null, bool release_scene = false) {
        try {
            if (release_scene) {
                _poolGo.Clear ();
            }
            _InitLoadNewSceneInfo ();
            SceneManager.LoadScene (scene_name);
        } catch (System.Exception e) {
            UnityEngine.Debug.LogError (e);
            throw;
        }
        while (true) {
            if (SceneManager.GetSceneByName (scene_name).isLoaded) {
                if (release_scene) {
                    UnloadSceneByName (SaveData.curCheckPoint);
                }
                _allLoadingScene[scene_name] = true;
                SaveData.curCheckPoint = scene_name;
                call_back?.Invoke ();
                UnityEngine.Debug.LogError ("加载完毕!");
                yield break;
            }
            yield return new WaitForSeconds (_seconds);
        }
    }

    private void _LoadPrefabToSceneFunc (string prefab_name, string child_path, string scene_name, object param = null) {
        var cur_scene = SceneManager.GetSceneByName (scene_name);
        SceneManager.SetActiveScene (cur_scene);
        Transform cur_parent = null;
        if (cur_scene != null) {
            var go_array = cur_scene.GetRootGameObjects ();
            cur_parent = go_array[0].transform;
        }
        GameObject go = null, new_go = null;
        //当在缓冲池则自动拷贝
        if (InstantiateCache.I.CanSpawn (RESOURCE_CATEGORY.Game, prefab_name)) {
            new_go = InstantiateCache.I.Spawn (RESOURCE_CATEGORY.Game, prefab_name, cur_parent).gameObject;
        } else {
            var path = InstantiateCache.GetAssetPath (RESOURCE_CATEGORY.Game, prefab_name, child_path);
            go = Resources.Load (path) as GameObject;
            new_go = MonoBehaviour.Instantiate (go, cur_parent);
            new_go.name = prefab_name;
            //不存在缓冲池则创建缓冲池
            if (!InstantiateCache.I.hasThisPool (RESOURCE_CATEGORY.Game)) {
                InstantiateCache.AddPool (RESOURCE_CATEGORY.Game, prefab_name, new_go);
            }
            if (!InstantiateCache.I.HasThisPrefabPool (RESOURCE_CATEGORY.Game, prefab_name)) {
                InstantiateCache.I.AddPrefabPool (RESOURCE_CATEGORY.Game, new_go);
            }
        }
        _poolGo.Add (new_go);
        new_go.SetActive (true);
        new_go.GetComponent<AllObjectBase> ()?.OnShow (param);
        _loadedCount -= 1;
        if (_loadedCount < 1)
            _loadedCallBack?.Invoke();
    }

    private void _InitLoadNewSceneInfo () {
        DataCache.loadedRounds = 0;
        DataCache.rounds = 1;
    }
}