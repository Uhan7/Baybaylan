using UnityEngine;

public class MahikaContainer : MonoBehaviour
{
    [SerializeField] private RectTransform m_mahikaGroup;
    [SerializeField] private GameObject m_mahikaBarPrefab;

    private void OnDestroy()
    {
        DestroyMahikaBars();
    }

    private void DestroyMahikaBars()
    {
        // Delete the mahika bars currently in the mahika group
        foreach (Transform mahikaBar in m_mahikaGroup.transform)
        {
            Destroy(mahikaBar.gameObject);
        }
    }

    public void InitializeMahikaBars(int _numMahikaBars = 1)
    {
        if (_numMahikaBars < 1) return;
        if (_numMahikaBars == m_mahikaGroup.transform.childCount) return;
        InitializeMahikaBarsInternal(_numMahikaBars);
    }

    private void InitializeMahikaBarsInternal(int _numMahikaBars = 1)
    {
        DestroyMahikaBars();

        // Create a new set of mahika bars
        for (int i = 0; i < _numMahikaBars; i++)
        {
            // Set mahika group as the parent of the new mahika bar
            GameObject newMahikaBar = Instantiate(m_mahikaBarPrefab);
            newMahikaBar.transform.SetParent(m_mahikaGroup.transform);

            // Set new mahika bar rect transform
            RectTransform newMahikaBarRectTransform = newMahikaBar.GetComponent<RectTransform>();
            newMahikaBarRectTransform.localScale = UnityEngine.Vector3.one;

            // This whole ahh shebal just to edit the position
            UnityEngine.Vector3 position = newMahikaBarRectTransform.localPosition;
            position.z = 0.0f;
            newMahikaBarRectTransform.localPosition = position;
        }
    }
}
