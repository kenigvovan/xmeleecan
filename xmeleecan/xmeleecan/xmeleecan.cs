using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using XMelee;
using xmeleecan.src.harmony;
using xmeleecan.src.items;

namespace xmeleecan
{
    public class xmeleecan : ModSystem
    {
        public override double ExecuteOrder()
        {
            return 0.03;
        }
        
        public const string harmonyID = "xmeleecan.Patches";
        public static Harmony harmonyInstance = new Harmony(harmonyID);
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("CANItemZweihander", typeof(CANItemZweihander));
            api.RegisterItemClass("CANItemZweihanderComponent", typeof(CANItemZweihanderComponent));

            api.RegisterItemClass("CANItemHalberd", typeof(CANItemHalberd));
            api.RegisterItemClass("CANItemHalberdComponent", typeof(CANItemHalberdComponent));
            //harmonyInstance = new Harmony(harmonyID);

            // harmonyInstance.get
        }
        public override void Dispose()
        {
            base.Dispose();
            harmonyInstance.UnpatchAll(harmonyID);
        }
        public override void StartPre(ICoreAPI api)
        {

            var pm = harmonyInstance.GetPatchedMethods();

            if (pm.Count() < 2)
            {               
                harmonyInstance.Patch(typeof(Vintagestory.API.Common.LayeredVoxelRecipe<SmithingRecipe>).GetMethod("FromBytes"), postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Postfix_FromBytes")));              
            }

            harmonyInstance.Patch(typeof(Vintagestory.API.Common.LayeredVoxelRecipe<SmithingRecipe>).GetMethod("ToBytes"), postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Postfix_ToBytes")));
            
            
           

            
           // pm = harmonyInstance.GetPatchedMethods();
            /* harmonyInstance = new Harmony(harmonyID);
             harmonyInstance.Patch(typeof(Vintagestory.ServerMods.RecipeLoader).GetMethod("AssetsLoaded"), prefix: new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_AssetsLoaded")));
             var gc = typeof(Vintagestory.ServerMods.RecipeLoader)
                .GetMethod("LoadGenericRecipe", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(SmithingRecipe));
             var pr = new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_LoadGenericRecipe"));*/
            /*harmonyInstance.Patch(gc,
                prefix: pr);*/
            base.StartPre(api);

        }
        public override void StartServerSide(ICoreServerAPI api)
        {

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            //harmonyInstance = new Harmony(harmonyID + "client");
            
            /*harmonyInstance.Patch(AccessTools.Constructor(typeof(GuiDialogBlockEntityRecipeSelector), new[] { typeof(string), typeof(ItemStack[]), typeof(Action<int>), typeof(Action), typeof(BlockPos), typeof(ICoreClientAPI) }),
                prefix: new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_GuiDialogBlockEntityRecipeSelector")));
           var pm =  harmonyInstance.GetPatchedMethods();
            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityAnvil).GetMethod("OpenDialog",
                    BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_OpenDialog")));*/

        }
    }
}
