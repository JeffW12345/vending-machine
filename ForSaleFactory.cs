using System;
using System.Collections.Generic;
using System.Text;

namespace Vending_Machine
{
    class ForSaleFactory
    {
        private static Dictionary<ItemsForSaleEnum, ForSale> enumToForSaleObjectDict = new Dictionary<ItemsForSaleEnum, ForSale>();
        public static Dictionary<ItemsForSaleEnum, ForSale> EnumToForSaleObjectDict { get => enumToForSaleObjectDict; }
        public static void PopulateEnumToForSaleDictionary()
        {
            Array itemValues = Enum.GetValues(typeof(ItemsForSaleEnum));
            foreach (ItemsForSaleEnum item in itemValues)
            {
                if (item == ItemsForSaleEnum.Cola)
                {
                    enumToForSaleObjectDict.Add(item, new ForSale(item.ToString(), 1.00));
                    continue;
                }
                if (item == ItemsForSaleEnum.Chocolate)
                {
                    enumToForSaleObjectDict.Add(item, new ForSale(item.ToString(), 0.65));
                    continue;
                }
                if (item == ItemsForSaleEnum.Crisps)
                {
                    enumToForSaleObjectDict.Add(item, new ForSale(item.ToString(), 0.50));
                    continue;
                }
                else
                {
                    throw new NotImplementedException("Item missing from PopulateForSaleDictionary method");
                }
            }
        }
        public static void UpdateStock(ItemsForSaleEnum itemForSale, int quantityToAddToStock)
        {
            if (ForSaleFactory.EnumToForSaleObjectDict.ContainsKey(itemForSale))
            {
                ForSaleFactory.EnumToForSaleObjectDict[itemForSale].Quantity += quantityToAddToStock;
            }
            else
            {
                throw new NotImplementedException("Object not in dictionary - debugging required!");
            }
        }
    }
}
