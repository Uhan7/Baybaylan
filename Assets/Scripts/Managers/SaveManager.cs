using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public enum SaveDataNames
    {
        //misc
        NewSave,

        //level data
        LevelProgress,

        //settings data
        //add ts eventually

        //alahas current inventory data
        //uses the unlock data names as a raw string id
        AlahasSlot1,
        AlahasSlot2,
        AlahasSlot3,
        AlahasSlot4,
        AlahasSlot5,
        AlahasSlot6,

        //alahas unlock data 
        UnlockedBalahiboNiAmihan,
        UnlockedDahonNgKawayan,
        UnlockedBilaongRatan,
        UnlockedDaliriNiTarabusaw,
        UnlockedPakpakNiPah,
        UnlockedKuwintasNaLuya,
        UnlockedKuwintasNgPitongTuka,
        UnlockedSungayNiTandayag,
        UnlockedMataNiRabot,
        UnlockedPangilNiOryol,
        UnlockedIlangIlang,
        UnlockedGumamela,
        UnlockedPulseras,
        UnlockedDahonNgMakahiya,
    }

    public static SaveManager Instance;
    [SerializeField] SaveData[] saveDatas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        foreach(SaveData data in saveDatas)
        {
            if(!data.doesDataExist())
                defualtData(data.dataName);

            data.updateCurrentValue();
        }
    }

    public void saveData<T>(SaveDataNames name, T value)
    {
        foreach(SaveData data in saveDatas)
            if(data.dataName == name)
            {
                data.setCurrentValue(value);
                return;
            }

        Debug.LogWarning("Data name " + name + " couldnt be found!");
    }

    public T getSaveData<T>(SaveDataNames name)
    {
        foreach(SaveData data in saveDatas)
            if(data.dataName == name)
            {
                return data.getCurrentData<T>();
            }

        Debug.LogWarning("Data name " + name + " couldnt be found!");
        return default;
    }

    public void defualtData(SaveDataNames name)
    {
        foreach(SaveData data in saveDatas)
            if(data.dataName == name)
            {
                data.resetToDefaultValue();
                return;
            }

        Debug.LogWarning("Data name " + name + " couldnt be found!");
    }

    public void defualtAllData()
    {
        foreach(SaveData data in saveDatas)
            data.resetToDefaultValue();
    }

    void Update()
    {
        foreach(SaveData data in saveDatas)
        {
            //data.updateCurrentValue();
        }
    }
}