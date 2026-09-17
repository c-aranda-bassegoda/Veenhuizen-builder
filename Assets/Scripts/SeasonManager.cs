using System.Collections.Generic;
using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    [SerializeField] GameObject Spring;
    [SerializeField] GameObject Summer;
    [SerializeField] GameObject Autumn;
    [SerializeField] GameObject Winter;

    private Dictionary<Season, GameObject> seasonMap;
    private void Awake()
    {
        seasonMap = new Dictionary<Season, GameObject>
        {
            { Season.Spring, Spring },
            { Season.Summer, Summer },
            { Season.Autumn, Autumn },
            { Season.Winter, Winter }
        };
    }

    private void Start()
    {
        ChangeSeason(Season.Spring);
    }

    public void ChangeSeason(Season season)
    {
        foreach (var kvp in seasonMap)
        {
            kvp.Value.SetActive(kvp.Key == season);
        }
    }
}
