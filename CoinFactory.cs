using System;
using System.Collections.Generic;
using System.Text;

namespace Vending_Machine
{
    class CoinFactory
    {
        private static List<Coin> listOfCoinsInMachine = new List<Coin>();
        private static List<Coin> listOfCoinsUserPutInMachine = new List<Coin>();

        internal static List<Coin> ListOfCoinsUserPutInMachine { get => listOfCoinsUserPutInMachine; set => listOfCoinsUserPutInMachine = value; }
        internal static List<Coin> ListOfCoinsInMachine { get => listOfCoinsInMachine; set => listOfCoinsInMachine = value; }

        public static Coin GetCoin(CoinDenominationsEnum coinValue)
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

        public static void AddCoinsToMachine(CoinDenominationsEnum coinValue, int numToAdd)
        {
            for (int i = 0; i < numToAdd; i++)
            {
                ListOfCoinsInMachine.Add(CoinFactory.GetCoin(coinValue));
            }
        }
    }
}
