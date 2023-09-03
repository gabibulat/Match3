using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoostersUI : MonoBehaviour
{
    [SerializeField] private Transform shelfTransform;
    [SerializeField] private Match3Visual match3Visual;
    [SerializeField] private ToggleGroup boostersToggleGroup;
    [SerializeField] private List<Toggle> boostersToggles;
    [SerializeField] private List<TMP_Text> boostersAmount;
    [SerializeField] private ShopItemsSO shopItemsSO;
    [SerializeField] private GameObject darkBackground;
    [SerializeField] private Toggle shopToggle;
    private BoosterItem currentSelectedBooster;
    private Vector3 backVector = new Vector3(-434, 0, 0);
    private Vector3 moveVector = new Vector3(0, 0, 0);
    private bool boosterUsed = false;

    private void Awake()
    {
        LoadBoosters();
        match3Visual.OnBoosterFinished += Match3Visual_BoosterFinished;
        foreach (Toggle toggle in boostersToggles)
        {
            toggle.onValueChanged.AddListener(delegate
            {
                OnSelectBooster(toggle);
            });
        }
    }

    private void LoadBoosters()
    {
        int i = 0;
        foreach (BoosterItem item in shopItemsSO.Boosters)
        {
            boostersAmount[i].text = item.GetAmount().ToString();
            i++;
        }
    }

    public void OpenShelf(bool open)
    {
        if (open)
        {
            shelfTransform.localPosition = moveVector;
            match3Visual.SetState(Match3Visual.State.BoosterSelecting);
            match3Visual.SetSelectedBoosterType(BoosterItem.BoosterType.None);
            darkBackground.SetActive(true);
        }
        else CloseShelf();
    }

    private void CloseShelf()
    {
        shelfTransform.localPosition = backVector;
        boostersToggleGroup.SetAllTogglesOff();
        darkBackground.SetActive(false);
        if (!boosterUsed)
        {
            match3Visual.SetState(Match3Visual.State.TryFindMatches);
        }
        boosterUsed = false;
        shopToggle.SetIsOnWithoutNotify(false);
    }

    private void Match3Visual_BoosterFinished(object sender, EventArgs e)
    {
        currentSelectedBooster.SetAmount(currentSelectedBooster.GetAmount() - 1);
        boosterUsed = true;
        LoadBoosters();
        CloseShelf();
    }

    public void OnSelectBooster(Toggle toggle)
    {
        if (toggle.isOn)
        {
            int index = boostersToggles.IndexOf(toggle);
            currentSelectedBooster = null;
            if (Int32.Parse(boostersAmount[index].text) > 0)
            {
                currentSelectedBooster = shopItemsSO.Boosters[index];
                int typeIndex = index + 1;
                match3Visual.SetSelectedBoosterType((BoosterItem.BoosterType)typeIndex);
            }
        }
        else
        {
            bool ifAllAreOff = true;
            foreach (Toggle t in boostersToggles)
            {
                if (t.isOn) ifAllAreOff = false;
            }
            if (ifAllAreOff)
            {
                match3Visual.SetSelectedBoosterType(BoosterItem.BoosterType.None);
            }
        }
    }

}
