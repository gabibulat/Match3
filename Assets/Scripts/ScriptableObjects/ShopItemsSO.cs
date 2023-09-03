using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShopItemsSO : ScriptableObject
{
    public List<WallpaperItem> Wallpapers = new();
    public List<BoosterItem> Boosters = new();

    private void OnEnable()
    {
        Load();
    }

    public void Save(object sender, EventArgs e)
    {
        for (int i = 0; i < Wallpapers.Count; i++)
        {
            PlayerPrefs.SetInt("isPurchased" + Wallpapers[i].path, Wallpapers[i].isPurchased ? 1 : 0);
            PlayerPrefs.SetInt("isEquipped" + Wallpapers[i].path, Wallpapers[i].isEquipped ? 1 : 0);
        }
        for (int i = 0; i < Boosters.Count; i++)
        {
            PlayerPrefs.SetInt("Amount" + Boosters[i].boosterType.ToString(), Boosters[i].amount);
        }

    }

    public void Load()
    {
        for (int i = 0; i < Wallpapers.Count; i++)
        {
            Wallpapers[i].sprite = Resources.Load<Sprite>(Wallpapers[i].path);
            if (PlayerPrefs.HasKey("isPurchased" + Wallpapers[i].path))
            {
                Wallpapers[i].isPurchased = PlayerPrefs.GetInt("isPurchased" + Wallpapers[i].path) == 1;
            }
            if (PlayerPrefs.HasKey("isEquipped" + Wallpapers[i].path))
            {
                Wallpapers[i].isEquipped = PlayerPrefs.GetInt("isEquipped" + Wallpapers[i].path) == 1;
            }
        }

        for (int i = 0; i < Boosters.Count; i++)
        {
            if (PlayerPrefs.HasKey("Amount" + Boosters[i].boosterType.ToString()))
            {
                Boosters[i].amount = PlayerPrefs.GetInt("Amount" + Boosters[i].boosterType.ToString());
            }
        }
        foreach (WallpaperItem item in Wallpapers) item.OnChanged = Save;
        foreach (BoosterItem item in Boosters) item.OnChanged = Save;
    }

}
[Serializable]
public class WallpaperItem
{
    public int coinPrice;
    public int gemPrice;
    public Sprite sprite;
    public bool isPurchased;
    public bool isEquipped;
    public string path;
    [NonSerialized] public EventHandler OnChanged;

    public int GetCoinPrice() => coinPrice;
    public int GetGemPrice() => gemPrice;
    public Sprite GetSprite() => sprite;
    public bool GetIsPurchased() => isPurchased;
    public bool GetIsEquipped() => isEquipped;
    public void SetIsPurchased(bool isPurchased)
    {
        this.isPurchased = isPurchased;
        OnChanged?.Invoke(this, null);
    }
    public void SetIsEquipped(bool isEquipped)
    {
        this.isEquipped = isEquipped;
        OnChanged?.Invoke(this, null);
    }
}

[Serializable]
public class BoosterItem
{
    public enum BoosterType
    {
        None,
        DestroyGem,
        DestroyBox,
        DestroyGlass
    }
    public int coinPrice;
    public int gemPrice;
    public BoosterType boosterType;
    public int amount;
    [NonSerialized] public EventHandler OnChanged;

    public int GetCoinPrice() => coinPrice;
    public int GetGemPrice() => gemPrice;
    public BoosterType GetBoosterType() => boosterType;
    public int GetAmount() => amount;
    public void SetAmount(int amount)
    {
        this.amount = amount;
        OnChanged?.Invoke(this, null);
    }
}