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
    public class CANItemHalberdComponent : Item, IContainedMeshSource, ITexPositionSource
    {
        private ITextureAtlasAPI targetAtlas;
        private Dictionary<string, AssetLocation> tmpTextures = new Dictionary<string, AssetLocation>();
        private Dictionary<int, MeshRef> meshrefs
        {
            get
            {
                return ObjectCacheUtil.GetOrCreate<Dictionary<int, MeshRef>>(this.api, "canhalberdcomponentrefs", () => new Dictionary<int, MeshRef>());
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
            if (Construction == "head")
            {
                string blade = itemStack.Attributes.GetString("head", "steel");
                string blade_type = itemStack.Attributes.GetString("head_type", "clean");
                return Lang.Get("xmeleecan:head_component", Lang.Get("material-" + blade), blade_type);
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
            if (construction == "head")
            {
                string head = itemstack.Attributes.GetString("head", "steel");

                string head_type = itemstack.Attributes.GetString("head_type", "clean");
                tmpTextures["head"] = new AssetLocation("game:block/metal/sheet/" + head + "1.png");
                Shape shapeHead = null;
                //
                shapeHead = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/halberd_head_" + head_type + ".json").ToObject<Shape>();

                MeshData meshHead;
                (api as ICoreClientAPI).Tesselator.TesselateShape("item shape", shapeHead, out meshHead, this, null, 0, 0, 0, null, null);
                return meshHead;
            }

            else if (construction == "handle")
            {
                string handle = itemstack.Attributes.GetString("handle", "oak");
                string handle_type = itemstack.Attributes.GetString("handle_type", "clean");
                tmpTextures["handle"] = new AssetLocation("game:block/wood/planks/" + handle + "1.png");

                Shape shapeHandle = null;

                shapeHandle = (this.api as ICoreClientAPI).Assets.TryGet("xmeleecan:shapes/halberd_handle_" + handle_type + ".json").ToObject<Shape>();

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
            jsonItemStack.Resolve(this.api.World, "canhalberd type", true);
            return jsonItemStack;
        }
        public void AddAllTypesToCreativeInventory()
        {
            List<JsonItemStack> stacks = new List<JsonItemStack>();
            string construction = this.Construction;
            Dictionary<string, string[]> vg = this.Attributes["variantGroups"].AsObject<Dictionary<string, string[]>>(null);
            if (construction == "head")
            {
                foreach (string head in vg["head"])
                {
                    foreach (string head_type in vg["head_type"])
                    {
                        stacks.Add(this.genJstack(string.Format("{{ head: \"{0}\", head_type: \"{1}\" }}", head, head_type)));
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
    }
}
