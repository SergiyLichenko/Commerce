using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodMarket
{
    public class Buyer:ICommon
    {
        public Buyer(int id)
        {
            this.ID = id;
        }

        List<Stend> doneStends;
        public int ID { private set; get; }

        #region Properties
        public List<Stend> DoneStends
        {
            get
            {
                lock (Locked)
                {
                    if (this.doneStends == null)
                        this.doneStends = new List<Stend>();
                    return this.doneStends;
                }
            }
            set
            {
                lock (Locked)
                {
                    this.doneStends = value;
                }
            }
        }
        #endregion
    }
}
