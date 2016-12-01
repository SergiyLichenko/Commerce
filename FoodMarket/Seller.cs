using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FoodMarket
{
    public class Seller : ICommon, IBackBuyer
    {
        public event BackBuyer backBuyer;
        public Seller()
        {
            this.SellerState = State.NotWorking;
            this.IsFree = true;
        }

        private volatile State sellerState;
        private volatile Buyer currentBuyer;
        private volatile bool isFree;
        private volatile Random random;


        #region Properties
        public State SellerState
        {
            get
            {
                lock (Locked)
                {
                    return sellerState;
                }
            }
            private set
            {
                lock (Locked)
                {
                    sellerState = value;
                }
            }
        }

        public bool IsFree
        {
            get
            {
                return isFree;
            }
            private set
            {
                isFree = value;
            }
        }

        private Buyer CurrentBuyer
        {
            get
            {
                return currentBuyer;
            }

            set
            {
                currentBuyer = value as Buyer;
            }
        }

        public Random Random
        {
            get
            {
                if (this.random == null)
                    this.random = new Random();
                return random;
            }

            set
            {
                random = value;
            }
        }


        #endregion

        public void NewBuyer(Buyer buyer)
        {
            this.CurrentBuyer = buyer;
            this.IsFree = false;

            PrintConsole("Продавец принял покупателя c ID = " + CurrentBuyer.ID,
                 ConsoleColor.DarkGray);
        }

        public override void Start()
        {
            this.SellerState = State.Working;
            this.IsFree = true;

            while (this.SellerState == State.Working)
            {
                Thread.Sleep(500);
                if (this.CurrentBuyer != null)
                {
                    Thread.Sleep(Random.Next(100, 200));

                    PrintConsole("Продавец отдал стенду покупателя с ID = " +
                        CurrentBuyer.ID, ConsoleColor.DarkGray);
                    lock(this.Locked)
                    {
                        this.IsFree = true;
                        if (backBuyer != null)
                            backBuyer(this, CurrentBuyer);
                        this.CurrentBuyer = null;
                    }
                    Thread.Sleep(200);
                }
                else
                    this.IsFree = true;
            }
        }
        public override void Finish()
        {
            this.SellerState = State.NotWorking;
        }


    }
}
