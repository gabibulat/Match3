using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [Header("Wallpapers")]
    [SerializeField] private ShopItemsSO shopItems;
    [SerializeField] private GameObject wallpaperPrefab;
    [SerializeField] private Transform wallpaperShopContainer;
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite GemSprite;
    private List<Toggle> wallpaperToggles = new();
    private Dictionary<Button, WallpaperItem> wallpaperItems = new();

    [Header("Boosters")]
    [SerializeField] private List<TMP_Text> boostersPrice = new();
    [SerializeField] private List<Image> boostersPriceImage = new();
    [SerializeField] private List<TMP_Text> boostersAmount = new();
    [SerializeField] private List<Button> boostersButtons = new();
    private Dictionary<Button, BoosterItem> boosterItems = new();

    [Header("PurchaseWindow")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button yesButton;

    private void Start()
    {
        LoadWallpaperShop();
        LoadBoosterShop();
    }

    public void LoadWallpaperShop()
    {
        for (int i = wallpaperShopContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(wallpaperShopContainer.GetChild(i).gameObject);
        }
        wallpaperToggles.Clear();
        wallpaperItems.Clear();
        ToggleGroup toggleGroup = wallpaperShopContainer.GetComponent<ToggleGroup>();

        foreach (WallpaperItem item in shopItems.Wallpapers)
        {
            GameObject wallpaper = Instantiate(wallpaperPrefab, wallpaperShopContainer);
            Image wallpaperImage = wallpaper.GetComponent<Image>();
            wallpaperImage.sprite = item.GetSprite();
            if (item.GetIsEquipped()) LoadLevel.Instance.SetWallpaper(item.GetSprite());

            Toggle wallpaperToggle = wallpaperImage.GetComponent<Toggle>();
            wallpaperToggle.group = toggleGroup;
            wallpaperToggle.isOn = item.GetIsEquipped();
            wallpaperToggle.onValueChanged.AddListener(OnWallpaperSelect);
            wallpaperToggles.Add(wallpaperToggle);

            Transform locked = wallpaper.transform.GetChild(1);

            // Price image
            GameObject priceImageGO = locked.GetChild(1).GetChild(0).gameObject;
            Image priceImage = priceImageGO.GetComponent<Image>();

            // Price text
            GameObject priceTextGO = locked.GetChild(1).GetChild(1).gameObject;
            TMP_Text priceText = priceTextGO.GetComponent<TMP_Text>();

            if (item.GetCoinPrice() > 0)
            {
                priceImage.sprite = coinSprite;
                priceText.text = item.GetCoinPrice().ToString();
            }
            else if (item.GetGemPrice() > 0)
            {
                priceImage.sprite = GemSprite;
                priceText.text = item.GetGemPrice().ToString();
            }
            locked.gameObject.SetActive(!item.GetIsPurchased());
            locked.GetComponent<Button>().onClick.AddListener(PurchaseWindowWallpaper);
            wallpaperItems.Add(locked.GetComponent<Button>(), item);
        }
    }

    public void OnWallpaperSelect(bool isOn)
    {
        if (isOn)
        {
            foreach (Toggle toggle in wallpaperToggles)
            {
                Sprite wallpaperSprite = toggle.gameObject.GetComponent<Image>().sprite;
                WallpaperItem wallpaperItem = shopItems.Wallpapers.Find(x => x.GetSprite() == wallpaperSprite);
                if (toggle.isOn)
                {
                    LoadLevel.Instance.SetWallpaper(wallpaperSprite);
                    wallpaperItem.SetIsEquipped(true);
                }
                else wallpaperItem.SetIsEquipped(false);
            }
        }
    }

    public void PurchaseWindowWallpaper()
    {
        GameObject buttonGO = EventSystem.current.currentSelectedGameObject.gameObject;
        Sprite priceSprite = buttonGO.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite;
        string price = buttonGO.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text;
        // Set up question
        if (priceSprite == coinSprite)
        {
            questionText.text = "Purchase this item for " + price + " coins?";
            yesButton.interactable = PlayerPrefs.GetInt("Coins") - Int32.Parse(price) >= 0;
        }
        else
        {
            questionText.text = "Purchase this item for " + price + " gems?";
            yesButton.interactable = PlayerPrefs.GetInt("Gems") - Int32.Parse(price) >= 0;
        }
        questionText.transform.parent.parent.gameObject.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            buttonGO.SetActive(false);
            if (priceSprite == coinSprite) PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - Int32.Parse(price));
            else PlayerPrefs.SetInt("Gems", PlayerPrefs.GetInt("Gems") - Int32.Parse(price));
            wallpaperItems[buttonGO.GetComponent<Button>()].SetIsPurchased(true);
            yesButton.transform.parent.parent.gameObject.SetActive(false);
			MenuUI.UpdateTextHUD();
		});
    }

    public void LoadBoosterShop()
    {
        boosterItems.Clear();
        for (int i = 0; i < shopItems.Boosters.Count; i++)
        {
            boostersAmount[i].text = shopItems.Boosters[i].GetAmount().ToString();
            if (shopItems.Boosters[i].GetCoinPrice() > 0)
            {
                boostersPriceImage[i].sprite = coinSprite;
                boostersPrice[i].text = shopItems.Boosters[i].GetCoinPrice().ToString();
            }
            else
            {
                boostersPriceImage[i].sprite = GemSprite;
                boostersPrice[i].text = shopItems.Boosters[i].GetGemPrice().ToString();
            }
            boosterItems.Add(boostersButtons[i], shopItems.Boosters[i]);
            boostersButtons[i].onClick.RemoveAllListeners();
            boostersButtons[i].onClick.AddListener(PurchaseWindowBoosters);
        }
    }

    public void PurchaseWindowBoosters()
    {
        GameObject buttonGO = EventSystem.current.currentSelectedGameObject.gameObject;
        Button button = buttonGO.GetComponent<Button>();
        int index = boostersButtons.IndexOf(button);

        if (boostersPriceImage[index].sprite == coinSprite)
        {
            questionText.text = "Purchase this item for " + boostersPrice[index].text + " coins?";
            yesButton.interactable = PlayerPrefs.GetInt("Coins") - Int32.Parse(boostersPrice[index].text) >= 0;
        }
        else
        {
            questionText.text = "Purchase this item for " + boostersPrice[index].text + " gems?";
            yesButton.interactable = PlayerPrefs.GetInt("Gems") - Int32.Parse(boostersPrice[index].text) >= 0;
        }
        questionText.transform.parent.parent.gameObject.SetActive(true);
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            if (boostersPriceImage[index].sprite == coinSprite) PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - Int32.Parse(boostersPrice[index].text));
            else PlayerPrefs.SetInt("Gems", PlayerPrefs.GetInt("Gems") - Int32.Parse(boostersPrice[index].text));
            boosterItems[button].SetAmount(boosterItems[button].GetAmount() + 1);
            boostersAmount[index].text = (Int32.Parse(boostersAmount[index].text) + 1).ToString();
            yesButton.transform.parent.parent.gameObject.SetActive(false);
			MenuUI.UpdateTextHUD();
		});
    }
}