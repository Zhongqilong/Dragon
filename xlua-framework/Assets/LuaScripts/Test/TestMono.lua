
Logger.Log("test-------------------")

local function Start()
    Logger.Log("Start-------------------")
end

local function Awake()
    Logger.Log("Awake-------------------")
end

local function OnDestroy()
    Logger.Log("OnDestroy-------------------")
end

TestMono = {
    Awake = Awake,
    Start = Start,
    OnDestroy = OnDestroy,
    OnShow = LuaMonoEmptyFunction,
}