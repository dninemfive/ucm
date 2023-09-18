using d9.utl;
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
    }
    [JsonConstructor]
    public Competition(string name, List<ItemId> irrelevantItems, Dictionary<ItemId, Rating> ratings)
    {
        Name = name;
        IrrelevantItems = irrelevantItems;
        Ratings = ratings;
    }
    public bool IsIrrelevant(ItemId id) => IrrelevantItems.Contains(id);
    public Rating? this[ItemId id] => IsIrrelevant(id) ? null : Ratings.TryGetValue(id, out Rating? r) ? r : null;
    public Rating? this[Item item] => this[item.Id];
    [JsonIgnore]
    public Item? Left, Right;
    public void ChooseLeft()
    {
        Ratings[Left!.Id].Increment(true);
        Ratings[Right!.Id].Increment(false);
    }
    public void ChooseRight()
    {
        Ratings[Left!.Id].Increment(false);
        Ratings[Right!.Id].Increment(true);
    }
    public void NextItems()
    {
        Left = ItemManager.RandomItem;
        Right = ItemManager.RandomItem;
    }
}
