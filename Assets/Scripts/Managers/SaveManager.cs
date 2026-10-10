using UnityEngine;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

public class SaveManager : MonoBehaviour
{
    //when making a new enum, just give it a unique number, doesnt need to be in order
    public enum SaveDataNames
    {
        //misc
        NoneOrNull = 0,
        NewSave = 1,

        //level data
        LevelSelectionIndex = 2,
        UnlockedArea0Intro = 3,
        UnlockedArea0Level1 = 4,
        UnlockedArea0Level2 = 5,
        UnlockedArea1Intro = 6,
        UnlockedArea1Level1 = 7,
        UnlockedArea1Level2 = 8,
        UnlockedArea1Level3 = 9,

        //settings data
        //add ts eventually

        //alahas current inventory data
        //uses the unlock data names as a raw string id
        AlahasSlot1 = 10,
        AlahasSlot2 = 11,
        AlahasSlot3 = 12,
        AlahasSlot4 = 13,
        AlahasSlot5 = 14,
        AlahasSlot6 = 15,

        //alahas unlock data 
        UnlockedBalahiboNiAmihan = 16,
        UnlockedDahonNgKawayan = 17,
        UnlockedBilaongRatan = 18,
        UnlockedDaliriNiTarabusaw = 19,
        UnlockedPakpakNiPah = 20,
        UnlockedKuwintasNaLuya = 21,
        UnlockedKuwintasNgPitongTuka = 22,
        UnlockedSungayNiTandayag = 23,
        UnlockedMataNiRabot = 24,
        UnlockedPangilNiOryol = 25,
        UnlockedIlangIlang = 26,
        UnlockedGumamela = 27,
        UnlockedPulseras = 28,
        UnlockedDahonNgMakahiya = 29,
    }

    public static SaveManager Instance;
    [OnValueChanged("defualtAllData")]
    [SerializeField] bool manualSaveReset;
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

        checkAllSaves();

        foreach(SaveData data in saveDatas)
        {
            if(!data.doesDataExist())
                defualtData(data.dataName);

            data.updateCurrentValue();
        }
    }

    void checkAllSaves()
    {
        //by gpt
        HashSet<int> ids = new HashSet<int>();
        foreach (SaveDataNames value in (SaveDataNames[])Enum.GetValues(typeof(SaveDataNames)))
        {
            int id = Convert.ToInt32(value);

            if (!ids.Add(id))
            {
                Debug.LogError($"Duplicate enum ID found: {value} = {id}");
            }
        }

        foreach(SaveDataNames id in (SaveDataNames[])Enum.GetValues(typeof(SaveDataNames)))
        {
            bool found = false;
            foreach(SaveData data in saveDatas)
                if(data.dataName == id)
                    found = true;
            if(!found)
                Debug.LogError("Save Data: " + id + " not found in SaveManager's list");
        }
    }

    public void saveData<T>(SaveDataNames name, T value, bool affectNewStatus)
    {
        foreach(SaveData data in saveDatas)
            if(data.dataName == name)
            {
                data.setCurrentValue(value);
                break;
            }

        if(affectNewStatus && getSaveData<int>(SaveDataNames.NewSave) == 1)
            saveData(SaveDataNames.NewSave, 0, false);
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