﻿using d9.utl;
using Microsoft.UI.Input;
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
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public HashSet<ItemId> IrrelevantItems { get; private set; } = new();
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
        public double Weight => TotalRatings == 0 ? 1 : 1 / (double)TotalRatings;
        [JsonConstructor]
        public Rating(int timesSelected, int totalRatings)
        {
            TimesSelected = timesSelected;
            TotalRatings = totalRatings;
        }
        public override string ToString() => $"{TimesSelected}/{TotalRatings} ({CiLowerBound:F2}-{CiUpperBound:F2})";
        public void Increment(bool selected)
        {
            if (selected)
                TimesSelected++;
            TotalRatings++;
        }
    }
    [JsonInclude]
    public Dictionary<ItemId, Rating> Ratings { get; set; } = new();
    public Competition(string name)
    {
        Name = name;
        (Left, Right) = (NextItem, NextItem);
    }
    [JsonConstructor]
    public Competition(string name, HashSet<ItemId> irrelevantItems, Dictionary<ItemId, Rating> ratings)
    {
        Name = name;
        IrrelevantItems = irrelevantItems;
        Ratings = ratings;
        (Left, Right) = (NextItem, NextItem);
    }
    public bool IsIrrelevant(ItemId id) => IrrelevantItems.Contains(id);
    public Rating? RatingOf(ItemId id) => IsIrrelevant(id) ? null : Ratings.TryGetValue(id, out Rating? r) ? r : null;
    public Rating? RatingOf(Item item) => RatingOf(item.Id);
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
            Ratings[rejectedId].Increment(false);
        }
        else
        {
            Ratings[rejectedId] = new(0, 1);
        }
#pragma warning restore CA1854
        NextItems();
    }
    [JsonIgnore]
    public IEnumerable<Item> RelevantItems => ItemManager.Items.Where(x => !IsIrrelevant(x.Id));
    private Item? _previousItem = null;
    [JsonIgnore]
    public Item NextItem
    {
        get
        {
            Utils.Log(RelevantItems.Count());
            Item result = RelevantItems.Where(x => x.Id != _previousItem?.Id && RatingOf(x)?.CiUpperBound >= 0.5)
                                       .WeightedRandomElement(x => RatingOf(x)?.Weight ?? 0);
            _previousItem = result;
            return result;
        }
    }
    public void NextItems()
        => (Left, Right) = (NextItem, NextItem);
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
        => RatingOf(this[side].Id);
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