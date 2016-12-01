using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoodMarket
{

    public delegate void NewBuyer(object sender, Buyer buyer);

    public class Shop : ICommon
    {
        public event NewBuyer riseNewBuyer;
        public Shop()
        {
            this.ShopState = State.NotWorking;
        }

        private volatile State shopState;
        private int newBuyerId;
        private Manager manager;
        private Thread managerThread;
        private Random random;

        #region Properties
        internal Manager Manager
        {
            get
            {
                if (this.manager == null)
                {
                    manager = new Manager(this);
                    manager.finishManager += Manager_finishManager;
                }

                return manager;
            }
        }



        public Thread ManagerThread
        {
            get
            {
                if (this.managerThread == null)
                {
                    managerThread = new Thread(Manager.Start);
                    managerThread.Name = "Manager Thread";
                }
                return managerThread;
            }
        }

        public State ShopState
        {
            get
            {
                return this.shopState;
            }

            private set
            {
                this.shopState = value;
            }
        }

        public Random Random
        {
            get
            {
                if (this.random == null)
                    random = new Random();
                return random;
            }

            set
            {
                random = value;
            }
        }

        #endregion

        public override void Start()
        {
            this.ShopState = State.Working;
            ManagerThread.Start();

            PrintConsole("Магазин открыт", ConsoleColor.Green);

            while (this.ShopState == State.Working)
            {
                Thread.Sleep(this.Random.Next(300, 500));

                if (this.riseNewBuyer != null && this.ShopState == State.Working)
                    this.riseNewBuyer(this, new Buyer(++newBuyerId));
            }

        }

        public override void Finish()
        {
            this.ShopState = State.Closing;
            this.Manager.Finish();

            PrintConsole("Магазин закрывается", ConsoleColor.Green);
        }
        private void Manager_finishManager(object sender)
        {
            if (((Manager)sender).ManagerState == State.NotWorking)
                this.ShopState = State.NotWorking;
            PrintConsole("Менеджер закончил работу", ConsoleColor.Green);
        }


    }
}
