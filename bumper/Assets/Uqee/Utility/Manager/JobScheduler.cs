using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Uqee.Pool;

public class JobData
{
    public uint id;
    public volatile float startTime;
    public float interval;
    public bool repeat;
    public Action callback;
    public bool isRemoved;

    private static volatile uint _idSeed = 0;
    public static JobData Create(Action callback, float startTime = 0, float interval = 0, bool repeat = false)
    {
        var data = DataFactory<JobData>.Get();
        data.id = ++_idSeed;
        data.callback = callback;
        data.startTime = startTime;
        data.interval = interval;
        data.repeat = repeat;
        data.isRemoved = false;
        return data;
    }
    public static void Release(JobData data)
    {
        data.callback = null;
        data.isRemoved = true;
        DataFactory<JobData>.Release(data);
    }
}
public class JobScheduler : Singleton<JobScheduler>
{
    protected override void Init()
    {
        base.Init();
        UpdateManager.I.AddCallback(_Update, "JobScheduler");
    }
    private ConcurrentQueue<uint> _removeList = new ConcurrentQueue<uint>();
    private ConcurrentDictionary<uint, JobData> _jobDict = new ConcurrentDictionary<uint, JobData>();
    public uint NextFrame(Action callback)
    {
        if (callback == null)
        {
            return 0;
        }

        var job = JobData.Create(callback);
        _jobDict.TryAdd(job.id, job);
        return job.id;
    }

    public uint SetTimeOut(Action callback, float interval)
    {
        if (callback == null)
        {
            return 0;
        }
        var job = JobData.Create(callback, AppStatus.realtimeSinceStartup, interval);
        if (_jobDict.TryAdd(job.id, job))
        {
            return job.id;
        }
        return 0;
    }

    public uint SetInterval(Action callback, float interval)
    {
        if (callback == null)
        {
            return 0;
        }
        var job = JobData.Create(callback, AppStatus.realtimeSinceStartup, interval, true);
        if (_jobDict.TryAdd(job.id, job))
        {
            return job.id;
        }

        return 0;
    }

    public void ClearTimer(uint timerId)
    {
        if(timerId==0)
        {
            return;
        }
        JobData job;
        _jobDict.TryGetValue(timerId, out job);
        if (job != null && !job.isRemoved)
        {
            job.isRemoved = true;
            _removeList.Enqueue(timerId);
        }
    }
    private void _DoRemove()
    {
        JobData job;
        while (!_removeList.IsEmpty)
        {
            uint timerId = 0;
            if (_removeList.TryDequeue(out timerId))
            {
                _jobDict.TryRemove(timerId, out job);
                if (job != null)
                {
                    JobData.Release(job);
                }
            }
        }
    }
    private void _Update()
    {
        JobData job;
        foreach (var pairs in _jobDict)
        {
            job = pairs.Value;
            if(job.isRemoved)
            {
                continue;
            }
            if (job.interval == 0 || AppStatus.realtimeSinceStartup > job.startTime + job.interval)
            {
                if (!job.repeat || job.callback == null)
                {
                    _removeList.Enqueue(pairs.Key);
                }
                else
                {
                    //interval如果需要把暂停或卡顿的调用次数都补上，改成 job.startTime + job.interval 
                    job.startTime = AppStatus.realtimeSinceStartup;
                }
                if (job.callback != null)
                {
                    try
                    {
                        job.callback.Invoke();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex);
                    }
                }
            }
        }

        _DoRemove();
    }
}