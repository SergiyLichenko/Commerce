using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodMarket
{
    public delegate void BackBuyer(object sender, Buyer buyer);
    public interface IBackBuyer
    {
        event BackBuyer backBuyer;
    }

    public enum State
    {
        Working,
        NotWorking,
        Closing
    }

    public abstract class ICommon
    {
        private object locked;
        private ILog logger;

        #region Properties
        public object Locked
        {
            get
            {
                if (this.locked == null)
                    this.locked = new object();
                return locked;
            }
            set
            {
                this.locked = value;
            }
        }
        public ILog Logger
        {
            get
            {
                if (this.logger == null)
                    this.logger = LogManager.GetLogger(typeof(Manager));
                return logger;
            }
        }
#endregion

        public virtual void Start()
        {
        }
        public virtual void Finish()
        {

        }

        protected void PrintConsole(string message, ConsoleColor color)
        {
            lock (Console.In)
            {
                Console.ForegroundColor = color;
                this.Logger.InfoFormat("{0}", message);
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
    
}
