namespace Vending_Machine
{
    internal class Coin
    {
        public string Description { get; set; }
        public double Value { get; set; }

        public Coin(string description, double value)
        {
            Description = description;
            Value = value;
        }
    }
}