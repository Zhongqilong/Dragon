
Logger.Log("test-------------------")

function Start()
    Logger.Log("Start-------------------")
end

function Awake()
    Logger.Log("Awake-------------------")
end

function OnDestroy()
    Logger.Log("OnDestroy-------------------")
end

TestMono = {
    Awake = Awake,
    Start = Start,
    OnDestroy = OnDestroy,
    OnShow = LuaMonoEmptyFunction,
}