using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
    public class CANItemZweihanderComponent : Item, IContainedMeshSource, ITexPositionSource
    {
        private ITextureAtlasAPI targetAtlas;
        private Dictionary<string, AssetLocation> tmpTextures = new Dictionary<string, AssetLocation>();
        private Dictionary<int, MeshRef> meshrefs
        {
            get
            {
                return ObjectCacheUtil.GetOrCreate<Dictionary<int, MeshRef>>(this.api, "canzweicomponentrefs", () => new Dictionary<int, MeshRef>());
            }
        }
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                return this.getOrCreateTexPos(this.tmpTextures[textureCode]);
            }
        }
        public string Construction
        {
            get
            {
                return this.Variant["construction"];
            }
        }
        public override string GetHeldItemName(ItemStack itemStack)
        {
            if(Construction == "blade")
            {
                string blade = itemStack.Attributes.GetString("blade", "steel");
                string blade_type = itemStack.Attributes.GetString("blade_type", "clean");
                return Lang.Get("xmeleecan:blade_component", Lang.Get("material-" + blade), blade_type);
            }
            if (Construction == "guard")
            {
                string guard = itemStack.Attributes.GetString("guard", "steel");
                string guard_type = itemStack.Attributes.GetString("guard_type", "clean");
                return Lang.Get("xmeleecan:guard_component", Lang.Get("material-" + guard), guard_type);
            }
            if (Construction == "handle")
            {
                string handle = itemStack.Attributes.GetString("handle", "steel");
                string handle_type = itemStack.Attributes.GetString("handle_type", "clean");
                return Lang.Get("xmeleecan:handle_component", Lang.Get("material-" + handle), handle_type);
            }
            return base.GetHeldItemName(itemStack);
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
        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos = null)
        {
            this.targetAtlas = targetAtlas;
            this.tmpTextures.Clear();
            string construction = this.Construction;
            if (construction == "blade")
            {
                string blade = itemstack.Attributes.GetString("blade", "steel");

                string blade_type = itemstack.Attributes.GetString("blade_type", "clean");
                tmpTextures["blade"] = new AssetLocation("game:block/metal/sheet/" + blade + "1.png");
                Shape shapeBlade = null;
                //
                shapeBlade = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_blade_" + blade_type + ".json").ToObject<Shape>();

                MeshData meshBlade;
                (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeBlade, out meshBlade, this, null, 0, 0, 0, null, null);
                return meshBlade;
            }
            else if (construction == "guard")
            {
                string guard_type = itemstack.Attributes.GetString("guard_type", "clean");
                string guard = itemstack.Attributes.GetString("guard", "steel");
               
                tmpTextures["guard"] = new AssetLocation("game:block/metal/sheet/" + guard + "1.png");
                Shape shapeGuard = null;
                //
                shapeGuard = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_guard_" + guard_type + ".json").ToObject<Shape>();

                MeshData meshGuard;
                (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeGuard, out meshGuard, this, null, 0, 0, 0, null, null);
                return meshGuard;
            }
            else if (construction == "handle")
            {
                string handle = itemstack.Attributes.GetString("handle", "orange");
                string handle_type = itemstack.Attributes.GetString("handle_type", "clean");
                tmpTextures["handle"] = new AssetLocation("game:block/leather/" + handle + ".png");
                tmpTextures["blade"] = new AssetLocation("game:block/metal/sheet/" + "steel" + "1.png");

                Shape shapeHandle = null;
                
                shapeHandle = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/zweihander_handle_" + handle_type + ".json").ToObject<Shape>();

                MeshData meshHandle;
                (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeHandle, out meshHandle, this, null, 0, 0, 0, null, null);
                return meshHandle;
            }
            return null;
        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            AddAllTypesToCreativeInventory();
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
            List<JsonItemStack> stacks = new List<JsonItemStack>();
            string construction = this.Construction;
            Dictionary<string, string[]> vg = this.Attributes["variantGroups"].AsObject<Dictionary<string, string[]>>(null);
            if (construction == "blade")
            {
                foreach (string blade in vg["blade"])
                {
                    foreach (string blade_type in vg["blade_type"])
                    {
                        stacks.Add(this.genJstack(string.Format("{{ blade: \"{0}\", blade_type: \"{1}\" }}", blade, blade_type)));
                    }
                }
            }
            else if (construction == "guard")
            {
                foreach (string guard in vg["guard"])
                {
                    foreach (string guard_type in vg["guard_type"])
                    {
                        stacks.Add(this.genJstack(string.Format("{{ guard: \"{0}\", guard_type: \"{1}\" }}", guard, guard_type)));
                    }
                }
            }
            else if (construction == "handle")
            {
                foreach (string handle in vg["handle"])
                {
                    foreach (string handle_type in vg["handle_type"])
                    {
                        stacks.Add(this.genJstack(string.Format("{{ handle: \"{0}\", handle_type: \"{1}\" }}", handle, handle_type)));
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
                        "items",
                        "xmeleecan"
                    }
                }
            };
        }

        public string GetMeshCacheKey(ItemStack itemstack)
        {
            string blade = itemstack.Attributes.GetString("blade", "-");
            string blade_type = itemstack.Attributes.GetString("blade_type", "-");
            string guard = itemstack.Attributes.GetString("guard", "steel");
            string guard_type = itemstack.Attributes.GetString("guard", "-");
            string handle = itemstack.Attributes.GetString("handle", "-");
            string handle_type = itemstack.Attributes.GetString("handle_type", "-");
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
    }
}
