namespace PlotThoseLines.Services;

public class ChartsStateService
{
    public string Interval { get; set; } = "day";
    public int DataLength { get; set; } = 30;
    public string VsCurrency { get; set; } = "usd";
    public Dictionary<string, CoinHistoryDataResponse?> AllAssetData { get; set; } = new();
    public List<LocalAsset> SelectedAssets { get; set; } = new();
    public string SelectedRange { get; set; } = "1m";
    public string? ErrorMessage { get; set; }
    public bool IsInitialized { get; set; } = false;
    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
    public void ClearState()
    {
        Interval = "day";
        DataLength = 30;
        VsCurrency = "usd";
        AllAssetData.Clear();
        SelectedAssets.Clear();
        SelectedRange = "1m";
        ErrorMessage = null;
        IsInitialized = false;
        NotifyStateChanged();
    }
}
