using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "SaveData")]
class SaveData : ScriptableObject
{
    public enum DataType
    {
        Int,
        Float,
        String
    }

    public SaveManager.SaveDataNames dataName;
    public DataType dataType;

    [ShowIf("dataType", DataType.Int)]
    public int defualtIntValue;
    [ShowIf("dataType", DataType.Int)] [OnValueChanged("saveCurrentValue")]
    public int currentIntValue;

    [ShowIf("dataType", DataType.Float)]
    public float defaultFloatValue;
    [ShowIf("dataType", DataType.Float)] [OnValueChanged("saveCurrentValue")]
    public float currentFloatValue;

    [ShowIf("dataType", DataType.String)]
    public string defualtStringValue;
    [ShowIf("dataType", DataType.String)] [OnValueChanged("saveCurrentValue")]
    public string currentStringValue;

    void updateCurrentValue()
    {
        switch (dataType)
        {
            case DataType.Int:
                currentIntValue = PlayerPrefs.GetInt(dataName.ToString(), defualtIntValue);
                break;
            case DataType.Float:
                currentFloatValue = PlayerPrefs.GetFloat(dataName.ToString(), defaultFloatValue);
                break;
            case DataType.String:
                currentStringValue = PlayerPrefs.GetString(dataName.ToString(), defualtStringValue);
                break;
        }
    }

    void saveCurrentValue()
    {
        if(!doesDataExist())
        {
            resetToDefaultValue();
            return;
        }

        switch (dataType)
        {
            case DataType.Int:
                PlayerPrefs.SetInt(dataName.ToString(), currentIntValue);
                Debug.Log("Saved " + dataName + " with data " + currentIntValue);
                break;
            case DataType.Float:
                PlayerPrefs.SetFloat(dataName.ToString(), currentFloatValue);
                Debug.Log("Saved " + dataName + " with data " + currentFloatValue);
                break;
            case DataType.String:
                PlayerPrefs.SetString(dataName.ToString(), currentStringValue);
                Debug.Log("Saved " + dataName + " with data " + currentStringValue);
                break;
        }

        PlayerPrefs.Save();
        updateCurrentValue();
    }

    public T getCurrentData<T>()
    {
        switch (dataType)
        {
            case DataType.Int:
                return (T)(object)currentIntValue;
            case DataType.Float:
                return (T)(object)currentFloatValue;
            case DataType.String:
                return (T)(object)currentStringValue;
        }

        return default;
    }

    public bool doesDataExist()
    {
        return PlayerPrefs.HasKey(dataName.ToString());
    }

    public void resetToDefaultValue()
    {
        switch (dataType)
        {
            case DataType.Int:
                currentIntValue = defualtIntValue;
                break;
            case DataType.Float:
                currentFloatValue = defaultFloatValue;
                break;
            case DataType.String:
                currentStringValue = defualtStringValue;
                break;
        }
        saveCurrentValue();
    }

    public void setCurrentValue<T>(T data)
    {
        switch (dataType)
        {
            case DataType.Int:
                currentIntValue = Convert.ToInt32(data);
                break;
            case DataType.Float:
                currentFloatValue = Convert.ToSingle(data);
                break;
            case DataType.String:
                currentStringValue = Convert.ToString(data);
                break;
        }
        saveCurrentValue();
    }
}