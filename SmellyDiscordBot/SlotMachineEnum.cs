using System;

namespace SmellyDiscordBot
{
    public static class SlotMachineEnum
    {
        enum Outcomes
        {
            heart,
            moneybag,
            ring,
            crown,
            frog,
            dolphin,
            zap,
            heartpulse,
            x
        }
        
        public static string GetRandomOutcome(Random rand)
        {
            var enums = Enum.GetValues(typeof(Outcomes));
            return enums.GetValue(rand.Next(enums.Length)).ToString();
        }
    }
}
