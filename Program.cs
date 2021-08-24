using System;
using System.Collections.Generic;
using System.Linq;

namespace Vending_Machine
{
    class Program
    {
        private static List<Coin> listOfCoinsInMachine = new List<Coin>();
        private static List<Coin> listOfCoinsUserPutInMachine = new List<Coin>();
        private static List<int> validUserOptions;
        private static Dictionary<ItemsForSaleEnum, ForSale> enumToObjDict = new Dictionary<ItemsForSaleEnum, ForSale>();
        private static double paidSoFar = 0;
        private static void MenuOptions()
        {
            validUserOptions = new List<int>(); // Clears the list each time the method is run
            CoinOptionsMessage(); // Tells the users which options to select to add various coins
            bool changeAvailable = IsChangeAvailable();
            if (changeAvailable)
            {
                Console.WriteLine("\nINSERT COINS");
            }
            else
            {
                Console.WriteLine("\nEXACT CHANGE ONLY");
            }
            ForSaleItemsMessage(); // List of items for sale, along with cost. 
            if(paidSoFar > 0)
            {
                RefundMessage();
            }
            Console.WriteLine("\nYou have paid in " + paidSoFar.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + " in total.");
            string input = Console.ReadLine();
            if (int.TryParse(input, out int numChosen) || validUserOptions.Contains(numChosen))
            {
                if(numChosen == 0)
                {
                    Console.WriteLine("Refund of " + paidSoFar.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + " given");
                    paidSoFar = 0;
                    RemoveCoinsUserPutInMachine();
                    listOfCoinsUserPutInMachine.Clear();
                    MenuOptions();
                }
                // If user inserted a coin
                if(numChosen < (Enum.GetNames(typeof(CoinDenominationsEnum)).Length + 1) && numChosen > 0)
                {
                    double paidInAmount = GetCoin((CoinDenominationsEnum)numChosen).Value;
                    paidSoFar += paidInAmount;
                    Console.WriteLine("Thank you for your payment of " + paidInAmount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + ".");
                    AddCoinsToMachine((CoinDenominationsEnum)numChosen, 1);
                    listOfCoinsUserPutInMachine.Add(GetCoin((CoinDenominationsEnum)numChosen));
                    MenuOptions();
                }
                // If user buys item
                else
                {
                    ItemsForSaleEnum itemEnumRef = (ItemsForSaleEnum)numChosen - (Enum.GetNames(typeof(CoinDenominationsEnum)).Length + 1);
                    UpdateStock(itemEnumRef, -1);
                    Console.WriteLine("Thank you for purchasing " 
                        + enumToObjDict[itemEnumRef].Name.ToLower() 
                        + " Enjoy it!");
                    if (changeAvailable)
                    {
                        double change = paidSoFar - enumToObjDict[itemEnumRef].Cost;
                        Console.WriteLine("Your change is " + change.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + ". Have a nice day. :)");
                        paidSoFar = 0;
                        RemoveCoinsUserPutInMachine();
                        listOfCoinsUserPutInMachine.Clear();
                        MenuOptions();
                    }
                    else
                    {
                        Console.WriteLine("Have a nice day");
                        paidSoFar = 0;
                        MenuOptions();
                    }
                }
            }
            // User selected an invalid option
            else
            {
                Console.WriteLine("That was not a valid option. Please try again");
                MenuOptions();
            }
        }

        private static void RemoveCoinsUserPutInMachine()
        {
            foreach(var userInsertedCoin in listOfCoinsUserPutInMachine)
            {
                foreach(var coin in listOfCoinsInMachine)
                {
                    if(coin.Value == userInsertedCoin.Value)
                    {
                        listOfCoinsInMachine.Remove(coin);
                        continue;
                    }
                }
            }
        }

        private static void RefundMessage()
        {
            Console.WriteLine("\nPress 0 for a full refund.");
            validUserOptions.Add(0);
        }

        private static void ForSaleItemsMessage()
        {
            Console.WriteLine("\nThese are the items for sale:\n");
            Array enumValues = Enum.GetValues(typeof(ItemsForSaleEnum));
            int itemNum = Enum.GetNames(typeof(CoinDenominationsEnum)).Length + 1; // Next number in display is the number after the coin denomination final number
            foreach (ItemsForSaleEnum item in enumValues)
            {
                ForSale forSale = enumToObjDict[item];
                if(forSale.Quantity == 0)
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " Cost: " + forSale.Cost + " OUT OF STOCK");
                    itemNum++;
                    continue;
                }
                if (forSale.Cost <= paidSoFar)
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " Cost: " + forSale.Cost + " SELECT ITEM NUMBER TO BUY");
                    validUserOptions.Add(itemNum);
                }
                else
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " Cost: " + forSale.Cost + " NOT ENOUGH PAID TO BUY");
                }
                itemNum++;
            }
        }

        private static void CoinOptionsMessage()
        {
            Console.WriteLine("\nSelect a coin by entering a number and pressing 'return':\n");
            Array enumValues = Enum.GetValues(typeof(CoinDenominationsEnum));
            foreach (CoinDenominationsEnum option in enumValues)
            {
                var coin = GetCoin(option);
                Console.WriteLine((int) option + ": " + coin.Description);
                validUserOptions.Add((int)option);
            }
        }

        private static Coin GetCoin(CoinDenominationsEnum coinValue)
        {
            switch (coinValue)
            {
                case (CoinDenominationsEnum.FiveP):
                    {
                        return new Coin("Five Pence Piece", 0.05);
                    }
                case (CoinDenominationsEnum.TwentyP):
                    {
                        return new Coin("Twenty Pence Piece", 0.20);
                    }
                case (CoinDenominationsEnum.FiftyP):
                    {
                        return new Coin("Fifty Pence Piece", 0.50);
                    }
                case (CoinDenominationsEnum.PoundCoin):
                    {
                        return new Coin("Pound Coin", 1.00);
                    }
                case (CoinDenominationsEnum.TwoPoundCoin):
                    {
                        return new Coin("Two Pound Coin", 2.00);
                    }
                default:
                    throw new NotImplementedException("Enum item not present in GetCoin method - debugging required!");
            }
        }

        // Whatever amount the user next puts in, and whatever item the user subsequently wants to buy, is change available?
        private static bool IsChangeAvailable()
        {
            Array coinEnum = Enum.GetValues(typeof(CoinDenominationsEnum));
            Array itemsEnum = Enum.GetValues(typeof(ItemsForSaleEnum));

            foreach (CoinDenominationsEnum coin in coinEnum)
            {
                foreach (ItemsForSaleEnum item in itemsEnum)
                {
                    // Move to next item if this item out of stock
                    if(enumToObjDict[item].Quantity == 0)
                    {
                        continue;
                    }
                    double coinVal = GetCoin(coin).Value;
                    double itemInMachineCost = enumToObjDict[item].Cost;
                    if ((coinVal + paidSoFar) > itemInMachineCost && !IsChangeAvailableForThisAmount((coinVal + paidSoFar) - itemInMachineCost))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsChangeAvailableForThisAmount(double amountRemaining)
        {
            if(listOfCoinsInMachine.Count == 0)
            {
                return false;
            }
            decimal remaining = (decimal)amountRemaining; // To prevent rounding issues, such as 15 being represented as 14.999999999
            listOfCoinsInMachine.OrderBy(o => o.Value).ToList(); // Sorts low to high by value
            for (int j = listOfCoinsInMachine.Count - 1; j > -1; j--)
            {
                if (remaining > 0 && j == 0)
                {
                    return false;
                }
                if (remaining >= (decimal) listOfCoinsInMachine[j].Value)
                {
                    remaining -= (decimal) listOfCoinsInMachine[j].Value;
                }
                if (remaining < (decimal) listOfCoinsInMachine[j].Value)
                {
                    continue;
                }
                if (remaining == 0)
                {
                    break;
                }
            }
            return true;
        }

        private static void UpdateStock(ItemsForSaleEnum itemForSale, int quantityToAddToStock)
        {
            if (enumToObjDict.ContainsKey(itemForSale))
            {
                enumToObjDict[itemForSale].Quantity += quantityToAddToStock;
            }
            else
            {
                throw new NotImplementedException("Object not in dictionary - debugging required!");
            }
        }
        private static void PopulateForSaleDictionary()
        {
            Array itemValues = Enum.GetValues(typeof(ItemsForSaleEnum));
            foreach (ItemsForSaleEnum item in itemValues)
            {
                if(item == ItemsForSaleEnum.Cola)
                {
                    enumToObjDict.Add(item, new ForSale(item.ToString(), 1.00));
                    continue;
                }
                if (item == ItemsForSaleEnum.Chocolate)
                {
                    enumToObjDict.Add(item, new ForSale(item.ToString(), 0.65));
                    continue;
                }
                if (item == ItemsForSaleEnum.Crisps)
                {
                    enumToObjDict.Add(item, new ForSale(item.ToString(), 0.50));
                    continue;
                }
                else
                {
                    throw new NotImplementedException("Item missing from PopulateForSaleDictionary method");
                }
            }
        }

        private static void AddCoinsToMachine(CoinDenominationsEnum coinValue, int numToAdd)
        {
            for(int i = 0; i < numToAdd; i++)
            {
                listOfCoinsInMachine.Add(GetCoin(coinValue));
            }
        }
        static void Main(string[] args)
        {
            PopulateForSaleDictionary();
            UpdateStock(ItemsForSaleEnum.Chocolate, 3);
            UpdateStock(ItemsForSaleEnum.Cola,4);
            UpdateStock(ItemsForSaleEnum.Crisps, 0);
            AddCoinsToMachine(CoinDenominationsEnum.FiveP, 30);
            AddCoinsToMachine(CoinDenominationsEnum.TwentyP, 2);
            MenuOptions();
        }
    }
}

