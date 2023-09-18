using d9.utl;
using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace d9.ucm;
public class Competition
{
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public List<ItemId> IrrelevantItems { get; private set; } = new();
    public class Rating
    {
        [JsonInclude]
        public int Selected { get; set; }
        [JsonInclude]
        public int Total { get; set; }
        [JsonIgnore]
        public float SelectedRatio => Selected / (float)Total;
        public void Increment(bool selected)
        {
            if (selected)
                Selected++;
            Total++;
        }
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
    public Competition(string name, List<ItemId> irrelevantItems, Dictionary<ItemId, Rating> ratings) : this(name)
    {
        IrrelevantItems = irrelevantItems;
        Ratings = ratings;
    }
    public bool IsIrrelevant(ItemId id) => IrrelevantItems.Contains(id);
    public Rating? this[ItemId id] => IsIrrelevant(id) ? null : Ratings.TryGetValue(id, out Rating? r) ? r : null;
    public Rating? this[Item item] => this[item.Id];
    [JsonIgnore]
    public Item Left, Right;
    public Item this[Side side] => side switch
    {
        Side.Left => Left,
        Side.Right => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(side))
    };
    public void Choose(Side side)
    {
        Ratings[this[side].Id].Increment(side == Side.Left);
        Ratings[this[side.Opposite()].Id].Increment(side == Side.Right);
    }
    public void NextItems()
    {
        Left = ItemManager.RandomItem;
        Right = ItemManager.RandomItem;
    }    
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