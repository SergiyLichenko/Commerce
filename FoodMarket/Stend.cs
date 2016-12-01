using System;
using System.Collections.Generic;
using System.Threading;


namespace FoodMarket
{

    public delegate void EndWork(object sender);

    public class Stend : ICommon, IComparable, IBackBuyer
    {
        public event BackBuyer backBuyer;
        public event EndWork endWork;
        public Stend(string name, double price)
        {
            this.ProductName = name;
            this.ProductPrice = price;

            SetProfit();
            PrintConsole("Стенд '" + this.ProductName + "' был создан с ценой товара [" + this.ProductPrice +
                "], количество продавцов [" + this.Sellers.Count + "], рентабельность [" + this.Profit + "]",
                ConsoleColor.White);
        }

        private double profit;
        private volatile int countSelledProduct;
        private volatile List<Seller> sellers;
        private volatile List<Thread> threadSeller;
        private volatile Queue<Buyer> buyers;
        private volatile State stendState;

        #region Properties
        public double Profit
        {
            get
            {
                lock (Locked)
                {
                    return profit;
                }
            }

            set
            {
                lock (Locked)
                {
                    profit = value;
                }
            }
        }
        public int CountSelledProduct
        {
            get
            {
                return countSelledProduct;
            }

            set
            {
                countSelledProduct = value;
            }
        }
        public string ProductName { get; private set; }

        public double ProductPrice { get; private set; }

        public Queue<Buyer> Buyers
        {
            get
            {
                lock (Locked)
                {
                    if (this.buyers == null)
                        this.buyers = new Queue<Buyer>();
                    return buyers;
                }
            }
        }

        public List<Seller> Sellers
        {
            get
            {
                if (this.sellers == null)
                {
                    this.sellers = new List<Seller>(new Random().Next(1, 6));
                    for (int i = 0; i < sellers.Capacity; i++)
                        this.sellers.Add(new Seller());
                }
                return sellers;
            }
        }

        public State StendState
        {
            get
            {
                lock (Locked)
                {
                    return stendState;
                }
            }
            set
            {
                lock (Locked)
                {
                    stendState = value;
                }
            }
        }

        public List<Thread> ThreadSeller
        {
            get
            {
                if (this.threadSeller == null)
                {
                    this.threadSeller = new List<Thread>(this.Sellers.Count);
                    for (int i = 0; i < threadSeller.Capacity; i++)
                    {
                        Thread tempThread = new Thread(this.Sellers[i].Start);
                        tempThread.Name = "Продавец " + i + " стенда '" + this.ProductName + "' Thread";
                        Sellers[i].backBuyer += Stend_backBuyer;
                        this.threadSeller.Add(tempThread);
                    }
                }
                return threadSeller;
            }
        }

        #endregion

        public override void Start()
        {
            this.StendState = State.Working;
            PrintConsole("Стенд '" + this.ProductName + "' начал работу",
                ConsoleColor.Green);

            foreach (Thread item in this.ThreadSeller)
                item.Start();

            while (true)
            {
                Thread.Sleep(500);
                if (this.Buyers.Count > 0)
                {
                    PrintConsole("На стенде '" + this.ProductName + "' очередь в [" +
                        this.Buyers.Count + "] человек, рентабельность [" + this.Profit + "]",
                        ConsoleColor.Green);

                    foreach (Seller item in this.Sellers)
                    {
                        if (item.IsFree == true)
                        {
                            if (this.Buyers.Count == 0)
                                break;

                            PrintConsole("Стенд '" + this.ProductName + "' отдал продавцу покупателя с ID = " +
                                this.Buyers.Peek().ID, ConsoleColor.Green);
                            if (item.SellerState == State.Working)
                                item.NewBuyer(this.Buyers.Dequeue());                            
                            SetProfit();
                        }
                    }
                }
                else if (this.StendState == State.Working)
                    PrintConsole("В очереди стенда '" + this.ProductName + "' 0 покупателей",
                        ConsoleColor.Green);

                if (this.StendState == State.Closing)
                {
                    if (this.Buyers.Count > 0)
                        continue;
                    if (this.IsAllSellersFree())
                    {
                        foreach (Seller item in Sellers)
                            item.Finish();
                        this.StendState = State.NotWorking;
                        if (this.endWork != null)
                            this.endWork(this);
                        return;
                    }
                }
            }
        }
        private void Stend_backBuyer(object sender, Buyer buyer)
        {
            this.CountSelledProduct++;
            buyer.DoneStends.Add(this);
            PrintConsole("Стенд '" + this.ProductName +
                "' принял от продавца покупателя с ID = " + buyer.ID, ConsoleColor.Green);

            PrintConsole("Стенд '" + this.ProductName +
                "' отправил менеджеру покупателя с ID = " + buyer.ID, ConsoleColor.Green);


            if (backBuyer != null)
                backBuyer(this, buyer);
        }
        public override void Finish()
        {
            this.StendState = State.Closing;
        }
        private void SetProfit()
        {
            if (this.Buyers.Count != 0)
                this.Profit = this.ProductPrice * this.Sellers.Count / this.Buyers.Count;
            else
                this.Profit = this.ProductPrice * this.Sellers.Count;
        }
        private bool IsAllSellersFree()
        {
            lock (this)
            {
                foreach (Seller item in this.Sellers)
                {
                    if (item.IsFree == false)
                        return false;
                }
            }
            return true;
        }

        public int CompareTo(object obj)
        {
            Stend temp = obj as Stend;
            if (this.Profit > temp.Profit)
                return 1;
            if (this.Profit < temp.Profit)
                return -1;
            return 0;
        }


    }
}
