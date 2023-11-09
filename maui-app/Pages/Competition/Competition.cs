using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class Competition
{
    #region constructors
    public Competition(string name)
    {
        Name = name;
        foreach(Item item in ItemManager.Items)
        {
            Ratings[item.Id] = new(0, 0);
        }
    }
    [JsonConstructor]
    public Competition(string name, HashSet<ItemId> irrelevantItems, Dictionary<ItemId, Rating> ratings)
    {
        Name = name;
        IrrelevantItems = irrelevantItems;
        Ratings = ratings;
        foreach(Item item in ItemManager.Items.Where(x => !ratings.ContainsKey(x.Id)))
        {
            Ratings[item.Id] = new(0, 0);
        }
    }
    #endregion constructors
    #region serialized properties
    [JsonInclude]
    public HashSet<ItemId> IrrelevantItems { get; private set; } = new();
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public Dictionary<ItemId, Rating> Ratings { get; set; } = new();
    #endregion serialized properties
    #region nonserialized properties
    [JsonIgnore]
    public string FilePath => PathFor(Name);
    [JsonIgnore]
    public Item NextItem
    {
        get
        {
            double threshold = RelevantRatings.Select(x => x.CiLowerBound).Percentile(ThresholdPercentile);
            Item result = RelevantItems.Where(x => x.Id != _previousItem?.Id && (RatingOf(x)?.ShouldShow(threshold) ?? true))
                                       .WeightedRandomElement(x => RatingOf(x)?.Weight ?? Rating.WeightFunction(0));
            _previousItem = result;
            return result;
        }
    }
    [JsonIgnore]
    public IEnumerable<Item> RelevantItems => ItemManager.Items.Where(x => !IsIrrelevant(x.Id));
    [JsonIgnore]
    public IEnumerable<Rating> RelevantRatings => RelevantItems.Select(RatingOf).Where(x => x is not null)!;
    [JsonIgnore]
    public IEnumerable<Rating> ShownRatings
    {
        get
        {
            double threshold = RelevantRatings.Select(x => x.CiLowerBound).Percentile(ThresholdPercentile);
            return RelevantRatings.Where(x => x.CiLowerBound >= threshold);
        }
    }
    [JsonIgnore]
    public double ThresholdPercentile { get; set; } = 0;
    #endregion nonserialized properties
    #region static properties/methods
    public static async Task<Competition?> LoadOrCreateAsync(string? name)
    {
        if (name is null or "")
            return null;
        string path = PathFor(name);
        if (File.Exists(path))
        {
            return await Task.Run(() => JsonSerializer.Deserialize<Competition>(File.ReadAllText(path))!);
        }
        else
        {
            Competition result = new(name);
            await result.SaveAsync();
            return result;
        }
    }
    public static Competition? Named(string? name)
        => name is null ? null : CompetitionManager.CompetitionsByName.TryGetValue(name, out Competition? competition) ? competition : null;
    public static IEnumerable<string> Names => CompetitionManager.Names;
    public static string PathFor(string name) => Path.Join(MauiProgram.TEMP_COMP_LOCATION, $"{name}.json");
    #endregion static properties/methods
    public bool IsIrrelevant(ItemId id) => IrrelevantItems.Contains(id) || ItemManager.TryGetItemById(id) is null;
    public Rating? RatingOf(ItemId id)
        => RatingOf(ItemManager.TryGetItemById(id));
    public Rating? RatingOf(Item? item)
    {
        if (item is null || IsIrrelevant(item.Id))
            return null;
        Rating? primaryRating = Ratings.TryGetValue(item.Id, out Rating? r) ? r : null;
        if (primaryRating is not null && item.MergeInfo is null)
            return primaryRating;
        IEnumerable<Rating?> otherRatings = item.MergeInfo?.ParentIds.Select(x => Ratings.TryGetValue(x, out Rating? r) ? r : null) ?? Enumerable.Empty<Rating>();
        int timesSelected = primaryRating?.TimesSelected ?? 0;
        int totalRatings = primaryRating?.TotalRatings ?? 0;
        foreach(Rating? rating in otherRatings)
        {
            if (rating is null)
                continue;
            timesSelected += rating.TimesSelected;
            totalRatings += rating.TotalRatings;
        }
        return new(timesSelected, totalRatings);
    }
    [JsonIgnore]
    public Item? Left, Right;
    public Item this[Side side] {
        get => side switch
        {
            Side.Left => Left!,
            Side.Right => Right!,
            _ => throw new ArgumentOutOfRangeException(nameof(side))
        };
        set
        {
            if (side is Side.Left)
                Left = value;
            else if (side is Side.Right)
                Right = value;
        }
    }
    public void Choose(Side side)
    {
        ItemId chosenId = this[side].Id, rejectedId = this[side.Opposite()].Id;
#pragma warning disable CA1854 // "prefer TryGetValue": need reference access to object
        if (Ratings.ContainsKey(chosenId))
        {
            Ratings[chosenId].Increment(true);
        } 
        else
        {
            Ratings[chosenId] = new(1, 1);
        }
        if (Ratings.ContainsKey(rejectedId))
        {
            Ratings[rejectedId].Increment(false);
        }
        else
        {
            Ratings[rejectedId] = new(0, 1);
        }
#pragma warning restore CA1854
        NextItems();
    }
    private Item? _previousItem = null;
    public event EventHandler? ItemsUpdated;
    public void NextItems()
    {
        (Left, Right) = (NextItem, NextItem);
        ItemsUpdated?.Invoke(this, new EventArgs());
    }
    public void SetIrrelevant(ItemId? id, bool value)
    {
        if (id is null)
            return;
        if (value)
        {
            _ = IrrelevantItems.Add(id.Value);
        } 
        else
        {
            _ = IrrelevantItems.Remove(id.Value);
        }
    }
    public void ToggleIrrelevant(ItemId id)
        => SetIrrelevant(id, !IsIrrelevant(id));
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
    }
    public class Rating
    {
        [JsonInclude]
        public int TimesSelected { get; set; } = 0;
        [JsonInclude]
        public int TotalRatings { get; set; } = 0;
        [JsonIgnore]
        public double Ratio => TimesSelected / (double)TotalRatings;
        [JsonIgnore]
        private const double _z = 1.644853627; // result of `=NORM.S.INV(0.95)` in Excel, equivalent to Statistics2.pnormaldist(0.95)
        [JsonIgnore]
        public double CiLowerBound
            => TotalRatings == 0 ? 0 : (Ratio + _z * _z / (2 * TotalRatings) - _z * Math.Sqrt((Ratio * (1 - Ratio) + _z * _z / (4 * TotalRatings)) / TotalRatings)) / (1 + _z * _z / TotalRatings);
        [JsonIgnore]
        public double CiUpperBound
            => TotalRatings == 0 ? 0 : (Ratio + _z * _z / (2 * TotalRatings) + _z * Math.Sqrt((Ratio * (1 - Ratio) + _z * _z / (4 * TotalRatings)) / TotalRatings)) / (1 + _z * _z / TotalRatings);
        [JsonIgnore]
        public double CiCenter
            => (CiLowerBound + CiUpperBound) / 2;
        [JsonIgnore]
        public double MarginOfError
            => (CiUpperBound - CiLowerBound) / 2;
        [JsonIgnore]
        public double Weight => WeightFunction(TotalRatings);
        public static double WeightFunction(int i) => i > 0 ? 1 / (double)(i * i) : 10.0;
        public bool ShouldShow(double percentile) => CiLowerBound > percentile; // TotalRatings < 7 || CiUpperBound >= 0.42;
        [JsonConstructor]
        public Rating(int timesSelected, int totalRatings)
        {
            TimesSelected = timesSelected;
            TotalRatings = totalRatings;
        }
        public override string ToString() => $"{TimesSelected}/{TotalRatings} ({CiCenter:F2}±{MarginOfError:F2})";
        public void Increment(bool selected)
        {
            if (selected)
                TimesSelected++;
            TotalRatings++;
        }
    }
}
public enum Side { Left, Right, None }
public static class SideExtensions
{
    public static Side Opposite(this Side side) => side switch
    {
        Side.Left => Side.Right,
        Side.Right => Side.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(side))
    };
}