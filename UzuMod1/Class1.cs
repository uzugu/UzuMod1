using BepInEx;
using RoR2;



namespace uzugu
{
    [BepInDependency("com.bepis.r2api")]
    //Change these
    [BepInPlugin("com.UzuStation.UzuMod1", "For now, a test", "0.0.1")]
    public class UzuMod1 : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogMessage("Loaded MyModName!");
            On.EntityStates.Huntress.ArrowRain.OnEnter += (orig, self) =>
            {
                // [The code we want to run]
                Chat.AddMessage("You used Huntress's Arrow Rain!");

                // Call the original function (orig)
                // on the object it's normally called on (self)
                orig(self);
            };
        }
    }
}