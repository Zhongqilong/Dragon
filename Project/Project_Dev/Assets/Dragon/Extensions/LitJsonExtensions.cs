using LitJson;

public static class LitJsonExtensions
{

    public static JsonData GetValue(this JsonData json, string name)
    {
        if (!json.IsObject || !json.Keys.Contains(name))
        {
            return null;
        }
        return json[name];
    }

    public static string GetString(this JsonData json, string name)
    {
        if (!json.IsObject || !json.Keys.Contains(name))
        {
            return string.Empty;
        }
        var ret = json[name];
        if (ret.IsString)
        {
            return (string)ret;
        }
        return ret.ToString();
    }

    public static int GetInt(this JsonData json, string name, int defaultVal = 0)
    {
        if (!json.IsObject || !json.Keys.Contains(name))
        {
            return defaultVal;
        }
        var ret = json[name];
        if (ret.IsInt || ret.IsLong)
        {
            return (int)ret;
        }
        return int.Parse(ret.ToString());
    }

    public static bool GetBool(this JsonData json, string name)
    {
        if (!json.IsObject || !json.Keys.Contains(name))
        {
            return false;
        }
        var ret = json[name];
        if (ret.IsBoolean)
        {
            return (bool)ret;
        }
        return bool.Parse(ret.ToString());
    }

    public static bool Contains(this JsonData json, string value)
    {
        if (json.IsArray)
        {
            for (int i = 0; i < json.Count; i++)
            {
                if (json[i].ToString() == value)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static int IndexOf(this JsonData json, string value)
    {
        if (json.IsArray)
        {
            for (int i = 0; i < json.Count; i++)
            {
                if (json[i].ToString() == value)
                {
                    return i;
                }
            }
        }
        return -1;
    }
}