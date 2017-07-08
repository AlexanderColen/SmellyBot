using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyDiscordBot.Gambling
{
    public class Gambler
    {
        #region Fields
        private string name;
        private int cash;
        #endregion
        #region Properties
        public string GetName()
        {
            return this.name;
        }
        public void SetName(string name)
        {
            this.name = name;
        }
        public int GetCash()
        {
            return this.cash;
        }
        public void SetCash(int cash)
        {
            this.cash = cash;
        }
        #endregion

        /// <summary>
        /// Add cash to the gambler.
        /// </summary>
        /// <param name="amount">The amount of cash to be added.</param>
        public void AddCash(int amount)
        {
            this.cash += amount;
        }
    }
}
