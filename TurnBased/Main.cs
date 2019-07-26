using ModMaker;
using ModMaker.Utility;
using System.Reflection;
using UnityModManagerNet;

namespace TurnBased
{
#if (DEBUG)
    [EnableReloading]
#endif
    static class Main
    {
        public static ModManager<Core, Settings> Mod;
        public static MenuManager Menu;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Mod = new ModManager<Core, Settings>(modEntry, assembly);
            Menu = new MenuManager(modEntry, assembly);
            modEntry.OnToggle = OnToggle;
#if (DEBUG)
            modEntry.OnUnload = Unload;
            return true;
        }

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            Menu = null;
            Mod.Disable(modEntry, true);
            Mod = null;
            return true;
        }
#else
            return true;
        }
#endif

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value)
            {
                Mod.Enable(modEntry);
                Menu.Enable(modEntry);
            }
            else
            {
                Menu.Disable(modEntry);
                Mod.Disable(modEntry, false);
                ReflectionCache.Clear();
            }
            return true;
        }
    }
}
