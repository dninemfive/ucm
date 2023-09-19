using d9.utl;
using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace d9.ucm;
public class Competition
{
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public HashSet<ItemId> IrrelevantItems { get; private set; } = new();
    public class Rating
    {
        [JsonInclude]
        public int Selected { get; set; } = 0;
        [JsonInclude]
        public int Total { get; set; } = 0;
        [JsonIgnore]
        public float SelectedRatio => Selected / (float)Total;
        public void Increment(bool selected)
        {
            if (selected)
                Selected++;
            Total++;
        }
        [JsonConstructor]
        public Rating(int selected, int total)
        {
            Selected = selected;
            Total = total;
        }
        public override string ToString() => $"{Selected}/{Total}";
    }
    [JsonInclude]
    public Dictionary<ItemId, Rating> Ratings { get; set; } = new();
    public Competition(string name)
    {
        Name = name;
        Left = ItemManager.RandomItem;
        Right = ItemManager.RandomItem;
    }
    [JsonConstructor]
    public Competition(string name, HashSet<ItemId> irrelevantItems, Dictionary<ItemId, Rating> ratings) : this(name)
    {
        IrrelevantItems = irrelevantItems;
        Ratings = ratings;
    }
    public bool IsIrrelevant(ItemId id) => IrrelevantItems.Contains(id);
    public Rating? this[ItemId id] => IsIrrelevant(id) ? null : Ratings.TryGetValue(id, out Rating? r) ? r : null;
    public Rating? this[Item item] => this[item.Id];
    [JsonIgnore]
    public Item Left, Right;
    public Item this[Side side] {
        get => side switch
        {
            Side.Left => Left,
            Side.Right => Right,
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
            Ratings[rejectedId].Increment(true);
        }
        else
        {
            Ratings[rejectedId] = new(0, 1);
        }
#pragma warning restore CA1854
        NextItems();
    }
    public void NextItem(Side side) => this[side] = ItemManager.RandomItemWhere(x => !IsIrrelevant(x.Id));
    public void NextItems()
    {
        NextItem(Side.Left);
        NextItem(Side.Right);
    }    
    public void MarkIrrelevant(Side side)
    {
        _ = IrrelevantItems.Add(this[side].Id);
    }
    public static async Task<Competition> LoadOrCreateAsync(string name)
    {
        string path = PathFor(name);
        if (File.Exists(path))
            return await Task.Run(() => JsonSerializer.Deserialize<Competition>(File.ReadAllText(path))!);
        else
            return new(name);
    }
    public static string PathFor(string name) => Path.Join(MauiProgram.TEMP_COMP_LOCATION, $"{name}.json");
    [JsonIgnore]
    public string FilePath => PathFor(Name);
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true}));
    }
    public Rating? RatingOf(Side side) 
        => this[this[side].Id];
}
public enum Side { Left, Right }
public static class SideExtensions
{
    public static Side Opposite(this Side side) => side switch
    {
        Side.Left => Side.Right,
        Side.Right => Side.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(side))
    };
}