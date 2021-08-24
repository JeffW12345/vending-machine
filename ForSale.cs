namespace Vending_Machine
{
    class ForSale
    {
        public string Name { get; set; }
        public double Cost { get; set; }
        public int Quantity { get; set; }
        public ItemsForSaleEnum EnumVal { get; set; }

        public ForSale(string name, double cost)
        {
            this.Name = name;
            this.Cost = cost;
        }
    }
}