using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace xmeleecan.src.harmony
{
    [HarmonyPatch]
    public class harmPatches
    {
        public static void Prefix_GuiDialogBlockEntityRecipeSelector(GuiDialogBlockEntityRecipeSelector __instance,  string DialogTitle, ItemStack[] recipeOutputs, Action<int> onSelectedRecipe, Action onCancelSelect, BlockPos blockEntityPos, ICoreClientAPI capi)
        {
            var c = 3;
        }
        public static void Prefix_OpenDialog(BlockEntityAnvil __instance, ItemStack ingredient)
        {
            var c = 3;
            var RRS = __instance.Api.ModLoader.GetModSystem<RecipeRegistrySystem>();
            var f1 = RRS;
            List<SmithingRecipe> recipes = (ingredient.Collectible as IAnvilWorkable).GetMatchingRecipes(ingredient);
            List<ItemStack> stacks = (from r in recipes
                                      select r.Output.ResolvedItemstack).ToList<ItemStack>();
            var f = 3;
        }
        public static void Prefix_AssetsLoaded(RecipeLoader __instance, ICoreAPI api)
        {
            var c = 3;
        }
        public static void Prefix_LoadGenericRecipe(RecipeLoader __instance, string className, AssetLocation path, SmithingRecipe recipe, Action<SmithingRecipe> RegisterMethod, ref int quantityRegistered, ref int quantityIgnored)
        {
            var c = 3;
        }
        public static void Postfix_ToBytes(SmithingRecipe __instance, BinaryWriter writer)
        {
           
            var c = 3;
            if(__instance.Output.Code.Domain == "xmeleecan")
            {
                var f = 3;
                //writer.Write(__instance.Output.Attributes.Token.ToString());

                //writer.Write("hello");
                /* writer.Write(__instance.Output.Attributes == null);
                 if (__instance.Output.Attributes != null)
                 {
                     writer.Write(__instance.Output.Attributes.Token.ToString());
                 }*/
               // writer.Write(__instance.Output.Attributes == null);
                //if (__instance.Output.Attributes != null)
                {
                    //var ou = __instance.Output.Attributes.Token.ToString();
                    writer.Write(__instance.Output.Attributes.Token.ToString());
                }
            }
            

        }
        public static void Postfix_FromBytes(SmithingRecipe __instance, BinaryReader reader, IWorldAccessor resolver)
        {
            if (__instance.Output.Code.Domain == "xmeleecan")
            {
                var p = reader.ReadString();
                __instance.Output.Attributes = new JsonObject(JToken.Parse(p));
                __instance.Output.ResolvedItemstack = new ItemStack(resolver.GetItem(__instance.Output.Code), __instance.Output.Quantity);
                /*foreach(var ke in __instance.Output.Attributes.)
                {

                }*/
                ITreeAttribute p2 = (ITreeAttribute)__instance.Output.Attributes.ToAttribute();
                __instance.Output.ResolvedItemstack.Attributes = p2;
                // if (reader.ReadBoolean())
                {

                    /*string json = reader.ReadString();
                    if (json == "")
                    {
                        var fff = 3;
                    }
                    try
                    {
                        __instance.Output.Attributes = new JsonObject(JToken.Parse(json));

                    }
                    catch (Newtonsoft.Json.JsonReaderException e)
                    {
                        var p3 = 3;
                    }*/
                }
            }
            
            return;
            var ing = new CraftingRecipeIngredient();
            __instance.RecipeId = reader.ReadInt32();
            ing.FromBytes(reader, resolver);
            int num = reader.ReadInt32();
            __instance.Pattern = new string[num][];
            for (int i = 0; i < __instance.Pattern.Length; i++)
            {
                __instance.Pattern[i] = reader.ReadStringArray();
            }

            var Name = new AssetLocation(reader.ReadString());
            __instance.Output = new JsonItemStack();
            //FromBytes(reader, resolver.ClassRegistry);
            __instance.Output.FromBytes(reader, resolver.ClassRegistry);
            __instance.Output.Resolve(resolver, "[Voxel recipe FromBytes]", ing.Code);
            __instance.GenVoxels();
        }
       /* public static void FromBytes(BinaryReader reader, IClassRegistryAPI instancer)
        {
            var t  = (EnumItemClass)reader.ReadInt16();
            var c = new AssetLocation(reader.ReadString());
            var st = reader.ReadInt32();
            var s = reader.ReadString();
            var ff = 1;
            if (reader.ReadBoolean())
            {
                ResolvedItemstack = new ItemStack(reader);
            }
        }*/
    }
}
