using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class MahikaManager : MonoBehaviour
{
    [System.Serializable]
    public struct MahikaData
    {
        public int currentMahika;
        public int targetMahika;
        public float GetMahikaPercent()
        {
            return (float) this.currentMahika / this.targetMahika;
        }
        public void AddScore(int _score)
        {
            this.currentMahika += _score;
            if (this.currentMahika > this.targetMahika) this.currentMahika = this.targetMahika;
        }
    }

    // Singleton ----------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static MahikaManager Instance;
    
    // Mahika Manager ----------------------------------------------------------
    [ReadOnly, SerializeField] private MahikaData[] m_mahikaDatas;
    [ReadOnly, SerializeField] private int m_mahikaDataCounter = 0;
    [SerializeField] private MahikaContainer m_mahikaContainer;
    [SerializeField] private TextMeshProUGUI m_mahikaScoreText;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Initialize(LevelConfig config)
    {
        m_mahikaDataCounter = 0;
        if (null == config) return;

        // Check if Multiple Target Mahika is enabled
        int numMahikaBars = 1;
        if (config.hasMultipleTargetMahika) numMahikaBars = config.multipleTargetMahika.Length;
        if (numMahikaBars < 1) numMahikaBars = 1;

        // Initialize UI
        m_mahikaContainer.InitializeMahikaBars(numMahikaBars);
        
        // Initialize Data
        m_mahikaDatas = new MahikaData[numMahikaBars]; // Set the m_mahikaDatas to a new array of MahikaData
        for (int i = 0; i < numMahikaBars; i++)
        {
            // If multiple target mahika is DISABLED, get the value of the 'target mahika'
            if (numMahikaBars == 1) m_mahikaDatas[i].targetMahika = config.targetMahika;
            // if multiple target mahika is ENABLED, get the values of the 'multiple target mahika'
            else m_mahikaDatas[i].targetMahika = config.multipleTargetMahika[i];
        }

        UpdateMahikaText();
    }
    public void UpdateMahika(int score)
    {
        int counter = m_mahikaDataCounter;

        m_mahikaDatas[counter].AddScore(score);
        float mahikaPercent = m_mahikaDatas[counter].GetMahikaPercent();
        m_mahikaContainer.SetMahikaBarFill(mahikaPercent, counter);

        if (mahikaPercent >= 1.0f) IncrementCounter();
        UpdateMahikaText();
    }
    public float GetMahikaPercent(int _index = 0)
    {
        int mahikaDataLength = m_mahikaDatas.Length;
        // By DEFAULT, always return the percent of the LAST mahika bar
        int index = (mahikaDataLength - 1);

        if (_index > 0 && _index < mahikaDataLength) index = _index;

        return m_mahikaDatas[index].GetMahikaPercent();
    }
    public bool DidWin()
    {
        if (GetMahikaPercent() >= 1.0f) return true;
        else return false;
    }
    private void IncrementCounter()
    {
        if (m_mahikaDataCounter >= (m_mahikaDatas.Length - 1)) return;
        m_mahikaDataCounter++;
    }
    private void UpdateMahikaText()
    {
        m_mahikaScoreText.text = m_mahikaDatas[m_mahikaDataCounter].currentMahika.ToString() + "/" + m_mahikaDatas[m_mahikaDataCounter].targetMahika;
    }
}
