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
    private ItemType itemType;
    private ItemObjectEditor itemObjectEditor;

    [MenuItem("TheIsland/Create/ItemObject")]
    public static void OpenWindow()
    {
        GetWindow<CreateItemObjects>("CreateItemObjects");
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
        const string PortablePrefabpath = "Assets/Prefabs/Inventory/PortableItemPrefabs";
        private Mesh portableObjectMesh;
        private Material[] portableObjectMaterials;
        private MaterialAssigner materialAssigner;

        private void SetMaterials(Material[] portableObjectMaterials) => this.portableObjectMaterials = portableObjectMaterials;
        public override void CreateUI()
        {
            base.CreateUI();

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
            // Portable Item Prefab creation
            GameObject Obj = new(portableItem.Id + "_PortableItem");
            Obj.AddComponent<MeshFilter>().mesh = portableObjectMesh;
            Obj.AddComponent<MeshRenderer>().materials = portableObjectMaterials;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{PortablePrefabpath}/{portableItem.Id}_PortableItem.prefab");
            DestroyImmediate(Obj);

            portableItem.portableItemPrefab = ObjPrefab;
        }
    }
    private abstract class WeaponItemEditor : ItemObjectEditor
    {
        const string WeaponPrefabpath = "Assets/Prefabs/Inventory/WeaponItemPrefabs";
        private Mesh weaponObjectMesh;
        private Material[] weaponObjectMaterials;
        private MaterialAssigner materialAssigner;

        private float damage;

        private void SetMaterials(Material[] weaponObjectMaterials) => this.weaponObjectMaterials = weaponObjectMaterials;
        public override void CreateUI()
        {
            base.CreateUI();

            EditorGUILayout.LabelField("Weapon object:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            weaponObjectMesh = (Mesh)EditorGUILayout.ObjectField(weaponObjectMesh, typeof(Mesh), false);
            if (EditorGUI.EndChangeCheck() && materialAssigner != null)
                materialAssigner.Destroy();

            if (materialAssigner == null && weaponObjectMesh != null && GUILayout.Button("Assign Materials"))
                materialAssigner = MaterialAssigner.OpenWindow(weaponObjectMesh, Materialpath, SetMaterials);
            
            damage = EditorGUILayout.FloatField("Damage:", damage);
        }
        protected override void AddAssetInfo(ItemObject itemObject)
        {
            base.AddAssetInfo(itemObject);
            MeleeWeaponItem weaponItem = itemObject as MeleeWeaponItem;
            // Weapon Item Prefab creation
            GameObject Obj = new(weaponItem.Id + "_WeaponItem");
            Obj.AddComponent<MeshFilter>().mesh = weaponObjectMesh;
            Obj.AddComponent<MeshRenderer>().materials = weaponObjectMaterials;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{WeaponPrefabpath}/{weaponItem.Id}_WeaponItem.prefab");
            DestroyImmediate(Obj);

            weaponItem.weaponPrefab = ObjPrefab;
            weaponItem.damage = damage;
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
        public override ItemType Type => ItemType.Tool;
        private ToolType toolType;
        public override void CreateUI()
        {
            base.CreateUI();
            toolType = (ToolType)EditorGUILayout.EnumPopup("Tool type:", toolType);
        }
        public override void CreateAsset()
        {
            ToolItem itemObject = CreateInstance<ToolItem>();
            AddAssetInfo(itemObject);
            itemObject.toolType = toolType;
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
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
    private class BowItemEditor : ItemObjectEditor
    {
        public override ItemType Type => ItemType.Bow;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            BowItem itemObject = CreateInstance<BowItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
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
    private class SwordItemEditor : WeaponItemEditor
    {
        public override ItemType Type => ItemType.Sword;
        public override void CreateUI()
        {
            base.CreateUI();
        }
        public override void CreateAsset()
        {
            SwordItem itemObject = CreateInstance<SwordItem>();
            AddAssetInfo(itemObject);
            AssetDatabase.CreateAsset(itemObject, $"{SOpath}/{itemObject.name}.asset");
        }
    }
}
