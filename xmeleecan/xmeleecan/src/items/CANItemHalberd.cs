using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace xmeleecan.src.items
{
    public class CANItemHalberd : XMelee.ItemPolearm, IContainedMeshSource, ITexPositionSource
    {
        private ITextureAtlasAPI targetAtlas;
        private Dictionary<string, AssetLocation> tmpTextures = new Dictionary<string, AssetLocation>();
        private Dictionary<int, MeshRef> meshrefs
        {
            get
            {
                return ObjectCacheUtil.GetOrCreate<Dictionary<int, MeshRef>>(this.api, "canlongswordsrefs", () => new Dictionary<int, MeshRef>());
            }
        }
        public string Construction
        {
            get
            {
                return this.Variant["construction"];
            }
        }
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                return this.getOrCreateTexPos(this.tmpTextures[textureCode]);
            }
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            //AddAllTypesToCreativeInventory();
        }
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            if (target == EnumItemRenderTarget.HandFp)
            {
                /* bool sneak = capi.World.Player.Entity.Controls.Sneak;
                 this.curOffY += ((sneak ? 0.4f : this.offY) - this.curOffY) * renderinfo.dt * 8f;
                 renderinfo.Transform.Translation.X = this.curOffY;
                 renderinfo.Transform.Translation.Y = this.curOffY * 1.2f;
                 renderinfo.Transform.Translation.Z = this.curOffY * 1.2f;*/
            }
            int meshrefid = itemstack.TempAttributes.GetInt("meshRefId", 0);
            if (meshrefid == 0 || !this.meshrefs.TryGetValue(meshrefid, out renderinfo.ModelRef))
            {
                int id = this.meshrefs.Count + 1;
                MeshRef modelref = capi.Render.UploadMesh(this.GenMesh(itemstack, capi.ItemTextureAtlas));
                renderinfo.ModelRef = (this.meshrefs[id] = modelref);
                itemstack.TempAttributes.SetInt("meshRefId", id);
            }
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }
        protected TextureAtlasPosition getOrCreateTexPos(AssetLocation texturePath)
        {
            TextureAtlasPosition texpos = this.targetAtlas[texturePath];
            if (texpos == null)
            {
                IAsset texAsset = this.api.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"), true);
                if (texAsset != null)
                {
                    int num;
                    this.targetAtlas.GetOrInsertTexture(texturePath, out num, out texpos, () => texAsset.ToBitmap(this.api as ICoreClientAPI), 0f);
                }
                else
                {
                    this.api.World.Logger.Warning("For render in shield {0}, require texture {1}, but no such texture found.", new object[]
                    {
                        this.Code,
                        texturePath
                    });
                }
            }
            return texpos;
        }
        public Size2i AtlasSize
        {
            get
            {
                return this.targetAtlas.Size;
            }
        }
        private JsonItemStack genJstack(string json)
        {
            JsonItemStack jsonItemStack = new JsonItemStack();
            jsonItemStack.Code = this.Code;
            jsonItemStack.Type = EnumItemClass.Item;
            jsonItemStack.Attributes = new JsonObject(JToken.Parse(json));
            jsonItemStack.Resolve(this.api.World, "canhalberd type", true);
            return jsonItemStack;
        }
        public void AddAllTypesToCreativeInventory()
        {
            /*if (this.Construction == "crude" || this.Construction == "blackguard")
            {
                return;
            }*/
            List<JsonItemStack> stacks = new List<JsonItemStack>();
            //this.Construction
            string construction = this.Construction;
            Dictionary<string, string[]> vg = this.Attributes["variantGroups"].AsObject<Dictionary<string, string[]>>(null);
            foreach (string head in vg["head"])
            {
                if (construction != head)
                {
                    continue;
                }

                foreach (string handle in vg["handle"])
                {
                    foreach (string head_type in vg["head_type"])
                    {
                            foreach (string handle_type in vg["handle_type"])
                            {
                                stacks.Add(this.genJstack(string.Format("{{ head: \"{0}\", handle: \"{1}\", head_type: \"{2}\", handle_type: \"{3}\" }}", head, handle, head_type, handle_type)));
                            }
                    }
                }
            }

            this.CreativeInventoryStacks = new CreativeTabAndStackList[]
            {
                new CreativeTabAndStackList
                {
                    Stacks = stacks.ToArray(),
                    Tabs = new string[]
                    {
                        "general",
                        "items"
                    }
                }
            };
        }
        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos = null)
        {
            this.targetAtlas = targetAtlas;
            this.tmpTextures.Clear();

            string head = itemstack.Attributes.GetString("head", "steel");
            string handle = itemstack.Attributes.GetString("handle", "oak");
            string head_type = itemstack.Attributes.GetString("head_type", "clean");
            string handle_type = itemstack.Attributes.GetString("handle_type", "clean");
            Shape shapeBlade = null;
            //
            shapeBlade = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/halberd_head_" + head_type + ".json").ToObject<Shape>();

            MeshData meshBlade;

            tmpTextures["handle"] = new AssetLocation("game:block/wood/planks/" + handle + "1.png");
            tmpTextures["head"] = new AssetLocation("game:block/metal/sheet/" + head + "1.png");
            (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeBlade, out meshBlade, this, null, 0, 0, 0, null, null);

            MeshData meshHandle;
            Shape shapeHandle = null;
            shapeHandle = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/halberd_handle_" + handle_type + ".json").ToObject<Shape>();
            (api as ICoreClientAPI).Tesselator.TesselateShape("item shape1", shapeHandle, out meshHandle, this, null, 0, 0, 0, null, null);

            meshHandle.AddMeshData(meshBlade);
            return meshHandle;
        }

        public string GetMeshCacheKey(ItemStack itemstack)
        {
            string head = itemstack.Attributes.GetString("head", "-");
            string head_type = itemstack.Attributes.GetString("head_type", "-");
            string handle = itemstack.Attributes.GetString("handle", "-");
            string handle_type = itemstack.Attributes.GetString("handle_type", "-");
            return string.Concat(new string[]
            {
                this.Code.ToShortString(),
                "-",
                head,
                "-",
                head_type,
                "-",
                handle,
                "-",
                handle_type
            });
        }
        public override void OnCreatedByCrafting(ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(inSlots, outputSlot, byRecipe);

            int headSlot = -1;
            int handleSlot = -1;

            for (int i = 0; i < inSlots.Length; i++)
            {
                if (inSlots[i].Empty)
                {
                    continue;
                }
                if (inSlots[i].Itemstack.Collectible.Code.Path.EndsWith("-head"))
                {
                    headSlot = i;
                }
                else if (inSlots[i].Itemstack.Collectible.Code.Path.EndsWith("-handle"))
                {
                    handleSlot = i;
                }
            }

            string head = inSlots[headSlot].Itemstack.Attributes.GetString("head", "steel");
            string handle = inSlots[handleSlot].Itemstack.Attributes.GetString("handle", "oak");
            string head_type = inSlots[headSlot].Itemstack.Attributes.GetString("head_type", "clean");
            string handle_type = inSlots[handleSlot].Itemstack.Attributes.GetString("handle_type", "clean");
            JsonItemStack jsonItemStack = new JsonItemStack();
            this.Code.Path.Split('-').First();
            //this.inventory[i].Itemstack.Collectible.Code.Path.Split('-').Last()
            jsonItemStack.Code = new AssetLocation("xmeleecan:" + this.Code.Path.Split('-').First() + "-" + head);
            //jsonItemStack.Code.Path = this.Code.Path.Split('-').First() + "-" + blade;
            jsonItemStack.Type = EnumItemClass.Item;
            jsonItemStack.Attributes = new JsonObject(JToken.Parse(string.Format(
                "{{ head: \"{0}\", handle: \"{1}\", head_type: \"{2}\", handle_type: \"{3}\" }}"
                , head, handle, head_type, handle_type)));
            jsonItemStack.Resolve(this.api.World, "canhalberd type", true);
            outputSlot.Itemstack = jsonItemStack.ResolvedItemstack.Clone();
            outputSlot.MarkDirty();
        }
        public override string GetHeldItemName(ItemStack itemStack)
        {

            string head = itemStack.Attributes.GetString("head", "steel");
            return Lang.Get("xmeleecan:halberd", Lang.Get("material-" + head));
        }
        public override void GetHeldItemInfo(ItemSlot itemslot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(itemslot, dsc, world, withDebugInfo);
            string blade = itemslot.Itemstack.Attributes.GetString("blade", "steel");
            string guard = itemslot.Itemstack.Attributes.GetString("guard", "steel");
            string handle = itemslot.Itemstack.Attributes.GetString("handle", "oak");
            string blade_type = itemslot.Itemstack.Attributes.GetString("blade_type", "clean");
            string guard_type = itemslot.Itemstack.Attributes.GetString("guard_type", "clean");
            string handle_type = itemslot.Itemstack.Attributes.GetString("handle_type", "clean");
            dsc.AppendLine("blade: " + blade_type + ", " + "guard: " + guard_type + ", " + "handle: " + handle_type);
        }

    }
}
