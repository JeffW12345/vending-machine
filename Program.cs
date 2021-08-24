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
        private static Dictionary<ItemsForSaleEnum, ForSale> enumToForSaleObjectDict = new Dictionary<ItemsForSaleEnum, ForSale>();
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
            if (int.TryParse(input, out int numChosen) && validUserOptions.Contains(numChosen))
            {
                if(numChosen == 0)
                {
                    Console.WriteLine("Refund of " + paidSoFar.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + " given");
                    paidSoFar = 0;
                    RefundUser(paidSoFar); // Removes money inserted by user from 'listOfCoinsUserPutInMachine' list.
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
                        + enumToForSaleObjectDict[itemEnumRef].Name.ToLower() 
                        + " Enjoy it!");
                    // Processes refund if applicable. No refund given if user was told 'EXACT MONEY ONLY' before inserting money, even if sufficient change now available 
                    // for refund.
                    if (changeAvailable)
                    {
                        double change = paidSoFar - enumToForSaleObjectDict[itemEnumRef].Cost;
                        Console.WriteLine("Your change is " + change.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + ". Have a nice day. :)");
                        paidSoFar = 0;
                        RefundUser(change); // Removes money inserted by user from 'listOfCoinsUserPutInMachine' list.
                        listOfCoinsUserPutInMachine.Clear();
                        MenuOptions();
                    }
                    else
                    {
                        Console.WriteLine("No change is available. Have a nice day");
                        paidSoFar = 0; // The balance is reset rather than carried forward. 
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

        private static void RefundUser(double amount)
        {
            decimal amountToRefund = (decimal)amount; // To prevent rounding issues.
            // Get indexes of Coin objects to remove from listOfCoinsUserPutInMachine list
            List<int> indexesToRemove = new List<int>();
            listOfCoinsUserPutInMachine.OrderBy(o => o.Value).ToList(); // Sort in ascending order by value
            for (int i = listOfCoinsInMachine.Count - 1; i > -1; i--)
            {
                // If the coin is <= the value of the refund, it forms part of the refund.
                if ((decimal)listOfCoinsInMachine[i].Value <= amountToRefund)
                {
                    amountToRefund -= (decimal)listOfCoinsInMachine[i].Value;
                    indexesToRemove.Add(i);
                }
                // If we have obtained all the coins we need to fully refund the customer.
                if (amountToRefund == 0)
                {
                    break;
                }
                // If we can't refund the customer with the available coins, there is a bug.
                if (amountToRefund > 0 && i == 0)
                {
                    throw new NotImplementedException("Refund not possible - debugging required!");
                }
            }
            // Remove the objects
            List<Coin> tempList = new List<Coin>();
            for (int j = 0; j < listOfCoinsInMachine.Count; j++)
            {
                if (indexesToRemove.Contains(j))
                {
                    tempList.Add(listOfCoinsInMachine[j]);
                }
            }
            listOfCoinsInMachine = tempList;
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
                ForSale forSale = enumToForSaleObjectDict[item];
                if(forSale.Quantity == 0)
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " cost: " + forSale.Cost.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) 
                        + " OUT OF STOCK");
                    itemNum++;
                    continue;
                }
                if (forSale.Cost <= paidSoFar)
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " cost: " + forSale.Cost.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) 
                        + " SELECT ITEM NUMBER TO BUY");
                    validUserOptions.Add(itemNum);
                }
                else
                {
                    Console.WriteLine(itemNum + ": " + forSale.Name + " cost: " + forSale.Cost.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) 
                        + " NOT ENOUGH PAID TO BUY");
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
                    if(enumToForSaleObjectDict[item].Quantity == 0)
                    {
                        continue;
                    }
                    double coinVal = GetCoin(coin).Value;
                    double itemInMachineCost = enumToForSaleObjectDict[item].Cost;
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
            if (enumToForSaleObjectDict.ContainsKey(itemForSale))
            {
                enumToForSaleObjectDict[itemForSale].Quantity += quantityToAddToStock;
            }
            else
            {
                throw new NotImplementedException("Object not in dictionary - debugging required!");
            }
        }
        private static void PopulateEnumToForSaleDictionary()
        {
            Array itemValues = Enum.GetValues(typeof(ItemsForSaleEnum));
            foreach (ItemsForSaleEnum item in itemValues)
            {
                if(item == ItemsForSaleEnum.Cola)
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

        private static void AddCoinsToMachine(CoinDenominationsEnum coinValue, int numToAdd)
        {
            for(int i = 0; i < numToAdd; i++)
            {
                listOfCoinsInMachine.Add(GetCoin(coinValue));
            }
        }
        static void Main(string[] args)
        {
            PopulateEnumToForSaleDictionary();
            UpdateStock(ItemsForSaleEnum.Chocolate, 3); // Adds three chocolate bars to the stock
            UpdateStock(ItemsForSaleEnum.Cola,4);
            UpdateStock(ItemsForSaleEnum.Crisps, 0);
            AddCoinsToMachine(CoinDenominationsEnum.FiveP, 30); // Adds 30 5p coins to the machine.
            AddCoinsToMachine(CoinDenominationsEnum.TwentyP, 2);
            MenuOptions(); // Presents the user with their options. Runs in a loop till program terminates.
        }
    }
}

