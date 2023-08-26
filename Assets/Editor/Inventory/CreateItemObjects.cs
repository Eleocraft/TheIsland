using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class CreateItemObjects : EditorWindow
{
    const string SOpath = "Assets/CustomObjects/Inventory/Items";
    const string Prefabpath = "Assets/Prefabs/Inventory/GroundItemPrefabs";
    const string Materialpath = "Assets/Art/Materials/GroundItems";
    const string FirstPersonLayerName = "FirstPerson";
    private ItemType itemType;
    private ItemObjectEditor itemObjectEditor;

    static EditorWindow window;

    [MenuItem("TheIsland/Create/ItemObject")]
    public static void OpenWindow()
    {
        window = GetWindow<CreateItemObjects>("CreateItemObjects");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Type of the item you want to create:", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        itemType = (ItemType)EditorGUILayout.EnumPopup(itemType);
        if (EditorGUI.EndChangeCheck() || itemObjectEditor == null)
            itemObjectEditor = GetItemObjectEditorByType(itemType);
        
        itemObjectEditor?.CreateUI();

        if (GUILayout.Button("Create"))
        {
            itemObjectEditor?.CreateAsset();
            Debug.Log("Asset Created");
            window.Close();
        }
    }

    private static ItemObjectEditor GetItemObjectEditorByType(ItemType itemType)
    {
        return typeof(CreateItemObjects).GetNestedTypes(BindingFlags.NonPublic)
                .Where(type => type.IsSubclassOf(typeof(ItemObjectEditor)))
                .Where(type => !type.IsAbstract)
                .Select(type => Activator.CreateInstance(type) as ItemObjectEditor)
                .Where(ItemObjectEditor => ItemObjectEditor.Type == itemType).Single();
    }

    private abstract class ItemObjectEditor
    {
        private string Name;
        private string Id;
        private Sprite Image;
        private bool stackable;
        private bool manualPickUp;
        private string description;

        private Mesh itemObjectMesh;
        private Material[] itemObjectMaterials;

        private MaterialAssigner materialAssigner;

        private bool autoGenerateId = true;
        public abstract ItemType Type {get;}
        private void SetMaterials(Material[] itemObjectMaterials) => this.itemObjectMaterials = itemObjectMaterials;
        public virtual void CreateUI()
        {
            EditorGUILayout.LabelField("General Info:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            Name = EditorGUILayout.TextField("Name:", Name);
            EditorGUI.BeginDisabledGroup(autoGenerateId);
            Id = EditorGUILayout.TextField("ID:", Id);
            EditorGUI.EndDisabledGroup();

            autoGenerateId = EditorGUILayout.Toggle("Generate Id automatically:", autoGenerateId);
            if (EditorGUI.EndChangeCheck() && autoGenerateId)
                Id = Utility.CreateID(Name);
            
            Image = (Sprite)EditorGUILayout.ObjectField("Image:", Image, typeof(Sprite), false);

            stackable = EditorGUILayout.Toggle("Stackable:", stackable);
            manualPickUp = EditorGUILayout.Toggle("Manual pick up:", manualPickUp);
            EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
            description = EditorGUILayout.TextArea(description, GUILayout.Height(80));
            
            EditorGUILayout.LabelField("GroundObject:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            itemObjectMesh = (Mesh)EditorGUILayout.ObjectField(itemObjectMesh, typeof(Mesh), false);
            if (EditorGUI.EndChangeCheck() && materialAssigner != null)
                materialAssigner.Destroy();

            if (materialAssigner == null && itemObjectMesh != null && GUILayout.Button("Assign Materials"))
                materialAssigner = MaterialAssigner.OpenWindow(itemObjectMesh, Materialpath, SetMaterials);
        }
        public abstract void CreateAsset();
        protected virtual void AddAssetInfo(ItemObject itemObject)
        {
            // Ground Prefab creation
            GameObject Obj = new(Id + "_GroundItem");
            Obj.AddComponent<MeshFilter>().mesh = itemObjectMesh;
            Obj.AddComponent<MeshRenderer>().materials = itemObjectMaterials;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{Prefabpath}/{Id}_GroundItem.prefab");
            DestroyImmediate(Obj);

            itemObject.name = Name;
            itemObject.Id = Id;
            itemObject.Image = Image;
            itemObject.GroundPrefab = ObjPrefab;
            itemObject.stackable = stackable;
            itemObject.manualPickUp = manualPickUp;
            itemObject.description = description;
        }
    }
    private abstract class PortableItemEditor : ItemObjectEditor
    {
        protected virtual string PortablePrefabpath => "Assets/Prefabs/Inventory/PortableItemPrefabs";
        protected virtual string PortableItemFileNameEnding => "PortableItem";
        private Mesh portableObjectMesh;
        private Material[] portableObjectMaterials;
        private MaterialAssigner materialAssigner;
        private bool reuseGroundItem;

        private void SetMaterials(Material[] portableObjectMaterials) => this.portableObjectMaterials = portableObjectMaterials;
        public override void CreateUI()
        {
            base.CreateUI();
            reuseGroundItem = EditorGUILayout.Toggle("reuse ground item:", reuseGroundItem);
            if (reuseGroundItem)
                return;
            
            EditorGUILayout.LabelField("Portable item object:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            portableObjectMesh = (Mesh)EditorGUILayout.ObjectField(portableObjectMesh, typeof(Mesh), false);
            if (EditorGUI.EndChangeCheck() && materialAssigner != null)
                materialAssigner.Destroy();

            if (materialAssigner == null && portableObjectMesh != null && GUILayout.Button("Assign Materials"))
                materialAssigner = MaterialAssigner.OpenWindow(portableObjectMesh, Materialpath, SetMaterials);
        }
        protected override void AddAssetInfo(ItemObject itemObject)
        {
            base.AddAssetInfo(itemObject);
            PortableItem portableItem = itemObject as PortableItem;

            GameObject Obj = new($"{portableItem.Id}_{PortableItemFileNameEnding}");
            if (reuseGroundItem)
                Obj = Instantiate(itemObject.GroundPrefab);
            else
            {
                // Portable Item Prefab creation
                Obj.AddComponent<MeshFilter>().mesh = portableObjectMesh;
                Obj.AddComponent<MeshRenderer>().materials = portableObjectMaterials;
            }
            Obj.layer = LayerMask.NameToLayer(FirstPersonLayerName);
            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{PortablePrefabpath}/{portableItem.Id}_{PortableItemFileNameEnding}.prefab");
            DestroyImmediate(Obj);

            portableItem.portableItemPrefab = ObjPrefab;
        }
    }
    private class WeaponItemEditor : PortableItemEditor
    {
        protected override string PortablePrefabpath => "Assets/Prefabs/Inventory/WeaponItemPrefabs";
        protected override string PortableItemFileNameEnding => "WeaponItem";

        public override ItemType Type => ItemType.MeleeWeapon;

        private float damage;

        public override void CreateUI()
        {
            base.CreateUI();
            damage = EditorGUILayout.FloatField("Damage:", damage);
        }
        public override void CreateAsset()
        {
            MeleeWeaponItem itemObject = CreateInstance<MeleeWeaponItem>();
            AddAssetInfo(itemObject);
            itemObject.damage = damage;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/Weapons/{itemObject.name}.asset");
        }
    }
    private abstract class ArmorItemEditor : ItemObjectEditor
    {
        private float defenceBonus;
        public override void CreateUI()
        {
            base.CreateUI();
            defenceBonus = EditorGUILayout.FloatField("DefenceBonus:", defenceBonus);
        }
        protected override void AddAssetInfo(ItemObject itemObject)
        {
            base.AddAssetInfo(itemObject);
            ArmorItem armorItem = itemObject as ArmorItem;
            armorItem.defenceBonus = defenceBonus;
        }
    }
    private class RecourceItemEditor : ItemObjectEditor
    {
        public override ItemType Type => ItemType.Resource;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            ResourceItem itemObject = CreateInstance<ResourceItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
    private class ToolItemEditor : PortableItemEditor
    {
        protected override string PortablePrefabpath => "Assets/Prefabs/Inventory/ToolItemPrefabs";
        protected override string PortableItemFileNameEnding => "ToolItem";
        public override ItemType Type => ItemType.Tool;
        private ToolType toolType;
        private float critTime = 0.2f;
        private float speed = 1;
        public override void CreateUI()
        {
            base.CreateUI();
            toolType = (ToolType)EditorGUILayout.EnumPopup("Tool type:", toolType);
            critTime = EditorGUILayout.FloatField("Crit Time:", critTime);
            speed = EditorGUILayout.FloatField("speed:", speed);
        }
        public override void CreateAsset()
        {
            ToolItem itemObject = CreateInstance<ToolItem>();
            AddAssetInfo(itemObject);
            itemObject.toolType = toolType;
            itemObject.CritTime = critTime;
            itemObject.Speed = speed;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/Tools/{itemObject.name}.asset");
        }
    }
    private class HammerItemEditor : PortableItemEditor
    {
        public override ItemType Type => ItemType.Hammer;
        private int hammerLevel = 1;
        public override void CreateUI()
        {
            base.CreateUI();
            hammerLevel = EditorGUILayout.IntField("Hammer Level:", hammerLevel);
        }
        public override void CreateAsset()
        {
            HammerItem itemObject = CreateInstance<HammerItem>();
            AddAssetInfo(itemObject);
            itemObject.HammerLevel = hammerLevel;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/Tools/{itemObject.name}.asset");
        }
    }
    private class FoodItemEditor : ItemObjectEditor
    {
        public override ItemType Type => ItemType.Food;
        private float restoreHealthValue;
        public override void CreateUI()
        {
            base.CreateUI();
            restoreHealthValue = EditorGUILayout.FloatField("HealthRestored:", restoreHealthValue);
        }
        public override void CreateAsset()
        {
            FoodItem itemObject = CreateInstance<FoodItem>();
            AddAssetInfo(itemObject);
            itemObject.restoreHealthValue = restoreHealthValue;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
    private class BootsItemEditor : ArmorItemEditor
    {
        public override ItemType Type => ItemType.Boots;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            BootsItem itemObject = CreateInstance<BootsItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
    private class BowItemEditor : PortableItemEditor
    {
        public override ItemType Type => ItemType.Bow;
        private ArrowItem arrowItem;
        private float totalDrawTime;
        private float minDrawTime;
        private float maxVelocity;
        public override void CreateUI()
        {
            base.CreateUI();
            arrowItem = (ArrowItem)EditorGUILayout.ObjectField("arrow item:", arrowItem, typeof(ArrowItem), false);
            totalDrawTime = EditorGUILayout.FloatField("Total draw time: ", totalDrawTime);
            minDrawTime = EditorGUILayout.FloatField("Min draw time: ", minDrawTime);
            maxVelocity = EditorGUILayout.FloatField("Max velocity: ", maxVelocity);
        }
        public override void CreateAsset()
        {
            BowItem itemObject = CreateInstance<BowItem>();
            AddAssetInfo(itemObject);
            itemObject.ArrowItem = arrowItem;
            itemObject.TotalDrawTime = totalDrawTime;
            itemObject.MinDrawTime = minDrawTime;
            itemObject.MaxVelocity = maxVelocity;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/Weapons/{itemObject.name}.asset");
        }
    }
    private class ChestplateItemEditor : ArmorItemEditor
    {
        public override ItemType Type => ItemType.Chestplate;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            ChestplateItem itemObject = CreateInstance<ChestplateItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
    private class HelmetItemEditor : ArmorItemEditor
    {
        public override ItemType Type => ItemType.Helmet;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            HelmetItem itemObject = CreateInstance<HelmetItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
    private class LeggingsItemEditor : ArmorItemEditor
    {
        public override ItemType Type => ItemType.Leggings;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            LeggingsItem itemObject = CreateInstance<LeggingsItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
}
