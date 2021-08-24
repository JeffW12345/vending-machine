using System;
using System.Collections.Generic;
using System.Linq;

namespace Vending_Machine
{
    class Program
    {
        private static List<int> validUserOptions;
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
                    CoinFactory.ListOfCoinsUserPutInMachine.Clear();
                    MenuOptions();
                }
                // If user inserted a coin
                if(numChosen < (Enum.GetNames(typeof(CoinDenominationsEnum)).Length + 1) && numChosen > 0)
                {
                    double paidInAmount = CoinFactory.GetCoin((CoinDenominationsEnum)numChosen).Value;
                    paidSoFar += paidInAmount;
                    Console.WriteLine("Thank you for your payment of " + paidInAmount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + ".");
                    CoinFactory.AddCoinsToMachine((CoinDenominationsEnum)numChosen, 1);
                    CoinFactory.ListOfCoinsUserPutInMachine.Add(CoinFactory.GetCoin((CoinDenominationsEnum)numChosen));
                    MenuOptions();
                }
                // If user buys item
                else
                {
                    ItemsForSaleEnum itemEnumRef = (ItemsForSaleEnum)numChosen - (Enum.GetNames(typeof(CoinDenominationsEnum)).Length + 1);
                    ForSaleFactory.UpdateStock(itemEnumRef, -1);
                    Console.WriteLine("Thank you for purchasing " 
                        + ForSaleFactory.EnumToForSaleObjectDict[itemEnumRef].Name.ToLower() 
                        + " Enjoy it!");
                    // Processes refund if applicable. No refund given if user was told 'EXACT MONEY ONLY' before inserting money, even if sufficient change now available 
                    // for refund.
                    if (changeAvailable)
                    {
                        double change = paidSoFar - ForSaleFactory.EnumToForSaleObjectDict[itemEnumRef].Cost;
                        Console.WriteLine("Your change is " + change.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-gb")) + ". Have a nice day. :)");
                        paidSoFar = 0;
                        RefundUser(change); // Removes money inserted by user from 'listOfCoinsUserPutInMachine' list.
                        CoinFactory.ListOfCoinsUserPutInMachine.Clear();
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
            CoinFactory.ListOfCoinsUserPutInMachine.OrderBy(o => o.Value).ToList(); // Sort in ascending order by value
            for (int i = CoinFactory.ListOfCoinsInMachine.Count - 1; i > -1; i--)
            {
                // If the coin is <= the value of the refund, it forms part of the refund.
                if ((decimal)CoinFactory.ListOfCoinsInMachine[i].Value <= amountToRefund)
                {
                    amountToRefund -= (decimal)CoinFactory.ListOfCoinsInMachine[i].Value;
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
            // Remove the objects. Creates a new list consisting of the Coin objects I don't want to remove, and then 
            // makes listOfCoinsInMachine point to the list. 
            List<Coin> tempList = new List<Coin>();
            for (int j = 0; j < CoinFactory.ListOfCoinsInMachine.Count; j++)
            {
                if (!indexesToRemove.Contains(j))
                {
                    tempList.Add(CoinFactory.ListOfCoinsInMachine[j]);
                }
            }
            CoinFactory.ListOfCoinsInMachine = tempList;
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
                ForSale forSale = ForSaleFactory.EnumToForSaleObjectDict[item];
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
                var coin = CoinFactory.GetCoin(option);
                Console.WriteLine((int) option + ": " + coin.Description);
                validUserOptions.Add((int)option);
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
                    if(ForSaleFactory.EnumToForSaleObjectDict[item].Quantity == 0)
                    {
                        continue;
                    }
                    double coinVal = CoinFactory.GetCoin(coin).Value;
                    double itemInMachineCost = ForSaleFactory.EnumToForSaleObjectDict[item].Cost;
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
            if(CoinFactory.ListOfCoinsInMachine.Count == 0)
            {
                return false;
            }
            decimal remaining = (decimal)amountRemaining; // To prevent rounding issues, such as 15 being represented as 14.999999999
            CoinFactory.ListOfCoinsInMachine.OrderBy(o => o.Value).ToList(); // Sorts low to high by value
            for (int j = CoinFactory.ListOfCoinsInMachine.Count - 1; j > -1; j--)
            {
                if (remaining > 0 && j == 0)
                {
                    return false;
                }
                if (remaining >= (decimal)CoinFactory.ListOfCoinsInMachine[j].Value)
                {
                    remaining -= (decimal)CoinFactory.ListOfCoinsInMachine[j].Value;
                }
                if (remaining < (decimal)CoinFactory.ListOfCoinsInMachine[j].Value)
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



        static void Main(string[] args)
        {
            ForSaleFactory.PopulateEnumToForSaleDictionary();
            ForSaleFactory.UpdateStock(ItemsForSaleEnum.Chocolate, 3); // Adds three chocolate bars to the stock
            ForSaleFactory.UpdateStock(ItemsForSaleEnum.Cola,4);
            ForSaleFactory.UpdateStock(ItemsForSaleEnum.Crisps, 1);
            CoinFactory.AddCoinsToMachine(CoinDenominationsEnum.FiveP, 30); // Adds 30 5p coins to the machine.
            CoinFactory.AddCoinsToMachine(CoinDenominationsEnum.TwentyP, 2);
            MenuOptions(); // Presents the user with their options. Runs in a loop till program terminates.
        }
    }
}
