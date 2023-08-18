using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using XMelee;

namespace xmeleecan.src.items
{
    public class CANItemZweihander : XMelee.ItemLongsword, IContainedMeshSource, ITexPositionSource
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
                if(textureCode == "guard")
                {
                    var c = 3;
                }
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
            jsonItemStack.Resolve(this.api.World, "canlongsword type", true);
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
            foreach (string blade in vg["blade"])
            {
                if(construction != blade)
                {
                    continue;
                }

                foreach (string guard in vg["guard"])
                {
                    foreach (string handle in vg["handle"])
                    {
                        foreach (string blade_type in vg["blade_type"])
                        {
                            foreach (string guard_type in vg["guard_type"])
                            {
                                foreach (string handle_type in vg["handle_type"])
                                {
                                    stacks.Add(this.genJstack(string.Format("{{ blade: \"{0}\", guard: \"{1}\", handle: \"{2}\", blade_type: \"{3}\", guard_type: \"{4}\", handle_type: \"{5}\" }}", blade, guard, handle, blade_type, guard_type, handle_type)));

                                }
                            }
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

            string blade = itemstack.Attributes.GetString("blade", "steel");
            string guard = itemstack.Attributes.GetString("guard", "steel");
            string handle = itemstack.Attributes.GetString("handle", "orange");
            string blade_type = itemstack.Attributes.GetString("blade_type", "clean");
            string guard_type = itemstack.Attributes.GetString("guard_type", "clean");
            string handle_type = itemstack.Attributes.GetString("handle_type", "clean");
            Shape shapeBlade = null;
            //
            shapeBlade = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_blade_" + blade_type + ".json").ToObject<Shape>();
          
            MeshData meshBlade;
            
            tmpTextures["handle"] = new AssetLocation("game:block/leather/" + handle + ".png");
            tmpTextures["blade"] = new AssetLocation("game:block/metal/sheet/" + blade + "1.png");
            tmpTextures["guard"] = new AssetLocation("game:block/metal/sheet/" + guard + "1.png");
            (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeBlade, out meshBlade, this, null, 0, 0, 0, null, null);

            MeshData meshHandle;
            Shape shapeHandle = null;
            shapeHandle = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_handle_" + handle_type + ".json").ToObject<Shape>();
            (api as ICoreClientAPI).Tesselator.TesselateShape("item shape1", shapeHandle, out meshHandle, this, null, 0, 0, 0, null, null);

            MeshData meshGuard;
            Shape shapeGuard = null;

            shapeGuard = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_guard_" + guard_type + ".json").ToObject<Shape>();

            
            (api as ICoreClientAPI).Tesselator.TesselateShape("item shape2", shapeGuard, out meshGuard, this, null, 0, 0, 0, null, null);

            //(this.api as ICoreClientAPI).Tesselator.TesselateItem(this, out mesh1, this);

            //if (blade == "iron")
              //  meshBlade.Rotate(new Vec3f(0.5f, 0.5f, 0.6f), 40, 40, 40);
            meshHandle.AddMeshData(meshBlade);
            meshHandle.AddMeshData(meshGuard);
            return meshHandle;
            //return mesh;
        }

        public string GetMeshCacheKey(ItemStack itemstack)
        {
            string blade = itemstack.Attributes.GetString("blade", "steel");
            string blade_type = itemstack.Attributes.GetString("blade_type", "clean");
            string guard = itemstack.Attributes.GetString("guard", "steel");
            string guard_type = itemstack.Attributes.GetString("guard", "clean");
            string handle = itemstack.Attributes.GetString("handle", "steel");
            string handle_type = itemstack.Attributes.GetString("handle_type", "clean");
            return string.Concat(new string[]
            {
                this.Code.ToShortString(),
                "-",
                blade,
                "-",
                blade_type,
                "-",
                guard,
                "-",
                guard_type,
                "-",
                handle,
                "-",
                handle_type
            });
        }
        public override void OnCreatedByCrafting(ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(inSlots, outputSlot, byRecipe);

            int bladeSlot = -1;
            int guardSlot = -1;
            int handleSlot = -1;

            for(int i = 0;i < inSlots.Length; i++)
            {
                if (inSlots[i].Empty)
                {
                    continue;
                }
                if (inSlots[i].Itemstack.Collectible.Code.Path.EndsWith("-blade"))
                {
                    bladeSlot = i;
                }
                else if (inSlots[i].Itemstack.Collectible.Code.Path.EndsWith("-guard"))
                {
                    guardSlot = i;
                }
                else if (inSlots[i].Itemstack.Collectible.Code.Path.EndsWith("-handle"))
                {
                    handleSlot = i;
                }
            }
        
            string blade = inSlots[bladeSlot].Itemstack.Attributes.GetString("blade", "steel");
            string guard = inSlots[guardSlot].Itemstack.Attributes.GetString("guard", "steel");
            string handle = inSlots[handleSlot].Itemstack.Attributes.GetString("handle", "orange");
            string blade_type = inSlots[bladeSlot].Itemstack.Attributes.GetString("blade_type", "clean");
            string guard_type = inSlots[guardSlot].Itemstack.Attributes.GetString("guard_type", "clean");
            string handle_type = inSlots[handleSlot].Itemstack.Attributes.GetString("handle_type", "clean");
            JsonItemStack jsonItemStack = new JsonItemStack();
            this.Code.Path.Split('-').First();
            //this.inventory[i].Itemstack.Collectible.Code.Path.Split('-').Last()
            jsonItemStack.Code = new AssetLocation("xmeleecan:" + this.Code.Path.Split('-').First() + "-" + blade);
            //jsonItemStack.Code.Path = this.Code.Path.Split('-').First() + "-" + blade;
            jsonItemStack.Type = EnumItemClass.Item;
            jsonItemStack.Attributes = new JsonObject(JToken.Parse(string.Format(
                "{{ blade: \"{0}\", guard: \"{1}\", handle: \"{2}\", blade_type: \"{3}\", guard_type: \"{4}\", handle_type: \"{5}\" }}"
                , blade, guard, handle, blade_type, guard_type, handle_type)));
            var c = jsonItemStack.Resolve(this.api.World, "canlongsword type", true);
            outputSlot.Itemstack = jsonItemStack.ResolvedItemstack.Clone();
            outputSlot.MarkDirty();
        }
        public override string GetHeldItemName(ItemStack itemStack)
        {
           
            string blade = itemStack.Attributes.GetString("blade", "steel");
            return Lang.Get("xmeleecan:zweihander", Lang.Get("material-" + blade));
        }
        public override void GetHeldItemInfo(ItemSlot itemslot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(itemslot, dsc, world, withDebugInfo);
            string blade = itemslot.Itemstack.Attributes.GetString("blade", "steel");
            string guard = itemslot.Itemstack.Attributes.GetString("guard", "steel");
            string handle = itemslot.Itemstack.Attributes.GetString("handle", "orange");
            string blade_type = itemslot.Itemstack.Attributes.GetString("blade_type", "clean");
            string guard_type = itemslot.Itemstack.Attributes.GetString("guard_type", "clean");
            string handle_type = itemslot.Itemstack.Attributes.GetString("handle_type", "clean");
            dsc.AppendLine("blade: " + blade_type + ", " + "guard: " + guard_type + ", " + "handle: " + handle_type);
        }

        /*public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
        {
            throw new NotImplementedException();
        }*/
    }
}
