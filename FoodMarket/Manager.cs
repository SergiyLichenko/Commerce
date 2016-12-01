using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace FoodMarket
{

    public delegate void FinishManager(object sender);

    class Manager : ICommon
    {
        public event FinishManager finishManager;
        public Manager(Shop shop)
        {
            this.Shop = shop;
            this.ManagerState = State.NotWorking;

            this.Shop.riseNewBuyer += RiseVisitor;

        }

        private Shop shop;
        private volatile State managerState;
        private volatile List<Stend> stends;
        private volatile List<Thread> stendThreads;
        private volatile List<string> productNames;
        private volatile Queue<Buyer> currentBuyer;
        private volatile int countVisitedBuyers;
        private volatile List<int> countBuyersAllStends;
        private double profit;
        private volatile Random random;


        #region Properties
        public Shop Shop
        {
            get
            {
                return shop;
            }

            private set
            {
                shop = value;
            }
        }

        public State ManagerState
        {
            get
            {
                return managerState;
            }

            set
            {
                managerState = value;
            }
        }

        public List<Stend> Stends
        {
            get
            {
                if (this.stends == null)
                {
                    this.stends = new List<Stend>(Random.Next(3, this.ProductNames.Count));

                    for (int i = 0; i < stends.Capacity; i++)
                    {
                        Stend tempStend = new Stend(this.ProductNames[i], Random.Next(10, 30));
                        tempStend.backBuyer += RiseVisitor;
                        tempStend.endWork += StendFinishedWorkEvent;
                        stends.Add(tempStend);
                    }
                }
                return stends;
            }
        }


        public List<Thread> StendThreads
        {
            get
            {
                if (this.stendThreads == null)
                {
                    this.stendThreads = new List<Thread>(this.Stends.Count);
                    for (int i = 0; i < this.stendThreads.Capacity; i++)
                    {
                        stendThreads.Add(new Thread(this.Stends[i].Start));
                        this.stendThreads[i].Name = "'" + this.Stends[i].ProductName + "' Thread";
                    }
                }
                return stendThreads;
            }
        }

        public Queue<Buyer> CurrentBuyer
        {
            get
            {
                lock (Locked)
                {
                    if (this.currentBuyer == null)
                        this.currentBuyer = new Queue<Buyer>();
                    return currentBuyer;
                }
            }

            set
            {
                lock (Locked)
                {
                    currentBuyer = value;
                }
            }
        }


        public List<int> CountBuyersAllStends
        {
            get
            {
                if (this.countBuyersAllStends == null)
                    this.countBuyersAllStends = new List<int>();
                return countBuyersAllStends;
            }

            private set
            {
                countBuyersAllStends = value;
            }
        }

        public double Profit
        {
            get
            {
                return profit;
            }

            private set
            {
                profit = value;
            }
        }

        public List<string> ProductNames
        {
            get
            {
                if (this.productNames == null)
                {
                    this.productNames = new List<string>();
                    this.productNames.Add("Кофе");
                    this.productNames.Add("Мороженое");
                    this.productNames.Add("Мясо");
                    this.productNames.Add("Алкоголь");
                    this.productNames.Add("Хлеб");
                    this.productNames.Add("Молочное");
                    this.productNames.Add("Шоколад");
                    this.productNames.Add("Торт");
                    this.productNames.Add("Сок");
                    this.productNames.Add("Вода");
                    this.productNames.Add("Пиво");
                    this.productNames.Add("Фрукты");
                }
                return productNames;
            }
        }

        public Random Random
        {
            get
            {
                if (this.random == null)
                    this.random = new Random();
                return this.random;
            }

        }

        public int CountVisitedBuyers
        {
            get
            {
                return countVisitedBuyers;
            }
            private set
            {
                countVisitedBuyers = value;
            }
        }



        #endregion

        public override void Start()
        {
            PrintConsole("Менеджер приступил к работе", ConsoleColor.Red);

            this.ManagerState = State.Working;
            foreach (Thread item in this.StendThreads)
                item.Start();

            while (this.ManagerState != State.NotWorking)
            {
                Thread.Sleep(100);
                if (this.CurrentBuyer.Count == 0)
                    continue;

                lock (this.Locked)
                {
                    if (this.ManagerState == State.Working)
                    {
                        if (CurrentBuyer.Peek().DoneStends.Count == this.Stends.Count)
                        {
                            PrintConsole("Покупатель с ID = " + CurrentBuyer.Peek().ID + " покинул магазин", ConsoleColor.Cyan);
                            this.CountBuyersAllStends.Add(CurrentBuyer.Peek().ID);
                        }
                        else
                        {
                            this.Stends.Sort();
                            this.Stends.Reverse();
                            foreach (Stend item in this.Stends)
                            {
                                if (!CurrentBuyer.Peek().DoneStends.Contains(item))
                                {
                                    string visitedStends = "[ ";
                                    foreach (Stend doneStend in CurrentBuyer.Peek().DoneStends)
                                        visitedStends += doneStend.ProductName + " ";
                                    visitedStends += "]";

                                    PrintConsole("Менеджер отправил покупателя с ID = " + CurrentBuyer.Peek().ID +
                                        " (пройдены стенды " + visitedStends + ") в очередь на стенд '" + item.ProductName + "'", ConsoleColor.Cyan);
                                    item.Buyers.Enqueue(CurrentBuyer.Peek());
                                    break;
                                }
                            }
                        }
                        PrintConsole("Менеджер обработал текущего покупателя с ID = " +
                        this.CurrentBuyer.Peek().ID, ConsoleColor.Red);
                        this.CurrentBuyer.Dequeue();
                    }
                }
            }
            if (this.CurrentBuyer.Count > 0)
                foreach (Buyer item in this.CurrentBuyer)
                    PrintConsole("Покупатель с ID = " + item.ID + " покинул магазин", ConsoleColor.Cyan);

        }

        public void RiseVisitor(object sender, Buyer buyer)
        {
            switch (this.ManagerState)
            {
                case State.NotWorking:
                    return;
                case State.Closing:
                    PrintConsole("Покупатель с ID = " + buyer.ID + " покинул магазин", ConsoleColor.Cyan);
                    return;
                case State.Working:
                    this.CurrentBuyer.Enqueue(buyer);
                    if (sender is Stend)
                        PrintConsole("Менеджер получил покупателя с ID = " + buyer.ID +
                                " от стенда '" + ((Stend)sender).ProductName + "'", ConsoleColor.Cyan);
                    if (sender is Shop)
                    {
                        this.CountVisitedBuyers++;
                        PrintConsole("В магазин вошел новый покупатель с ID = " + buyer.ID, ConsoleColor.Yellow);
                    }
                    break;
            }
        }

        private void StendFinishedWorkEvent(object sender)
        {
            foreach (Stend item in this.Stends)
                if (item.StendState != State.NotWorking)
                    return;

            this.ManagerState = State.NotWorking;

            Thread.Sleep(200);
            FinalPrint();
            if (this.finishManager != null)
                this.finishManager(this);
        }
        private void FinalPrint()
        {
            Console.WriteLine();

            foreach (Stend item in this.Stends)
            {
                this.Profit += item.ProductPrice * item.CountSelledProduct;
                PrintConsole("Стенд '" + item.ProductName + "' продал " + item.CountSelledProduct +
                    " товавов по цене " + item.ProductPrice + " и прибыль: " + item.ProductPrice * item.CountSelledProduct, ConsoleColor.White);
            }

            PrintConsole("Все стенды прошло " + this.CountBuyersAllStends.Count + " человек", ConsoleColor.White);
            PrintConsole("В магазине побывало " + this.CountVisitedBuyers + " человек", ConsoleColor.White);

            foreach (int item in this.CountBuyersAllStends)
                PrintConsole("Покупатель с ID = " + item, ConsoleColor.White);

            PrintConsole("Покупатели, которые не прошли все стенды:", ConsoleColor.Red);
            for (int i = 1; i <= this.CountVisitedBuyers; i++)
                if (!CountBuyersAllStends.Contains(i))
                    PrintConsole("Покупатель с ID = " + i, ConsoleColor.Blue);

            PrintConsole("Общяя прибыль " + this.Profit, ConsoleColor.White);
        }

        public override void Finish()
        {
            PrintConsole("Менеджер дал команду стендам завершить работу", ConsoleColor.Red);
            this.ManagerState = State.Closing;
            foreach (Stend item in this.Stends)
                item.Finish();
        }

    }
}
