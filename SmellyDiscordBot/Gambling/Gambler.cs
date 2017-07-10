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
        private Int64 cash;
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
        public Int64 GetCash()
        {
            return this.cash;
        }
        public void SetCash(Int64 cash)
        {
            this.cash = cash;
        }
        #endregion

        /// <summary>
        /// Add cash to the gambler.
        /// </summary>
        /// <param name="amount">The amount of cash to be added.</param>
        public void AddCash(Int64 amount)
        {
            this.cash += amount;
        }

        /// <summary>
        /// Removes cash from the gambler.
        /// </summary>
        /// <param name="amount">The amount of cash to be removed.</param>
        public void RemoveCash(Int64 amount)
        {
            this.cash -= amount;
        }
    }
}
