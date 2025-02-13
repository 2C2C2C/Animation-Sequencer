#if PRIMETWEEN_ENABLED
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BrunoMikoski.AnimationSequencer
{
    // TODO separate DOTween dropdown item and prime tween dropdown item, since they are all in the same collection
    public sealed class PrimeTweenActionAdvancedDropdownItem : AdvancedDropdownItem
    {
        private Type basePrimeTweenActionType;
        public Type BasePrimeTweenActionType => basePrimeTweenActionType;

        public PrimeTweenActionAdvancedDropdownItem(Type basePrimeTweenActionType, string displayName) : base(displayName)
        {
            this.basePrimeTweenActionType = basePrimeTweenActionType;
        }
    }

    public sealed class PrimeTweenActionsAdvancedDropdown : AdvancedDropdown
    {
        private Action<PrimeTweenActionAdvancedDropdownItem> callback;
        private SerializedProperty actionsList;
        private GameObject targetGameObject;

        public PrimeTweenActionsAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
            this.minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("PrimeTween Actions");
            foreach (var typeToDisplayGUI in AnimationSequenceEditorGUIUtility.TypeToDisplayName)
            {
                Type basePrimeTweenActionType = typeToDisplayGUI.Key;
                AdvancedDropdownItem targetFolder = root;
                if (AnimationSequenceEditorGUIUtility.TypeToParentDisplay.TryGetValue(basePrimeTweenActionType, out GUIContent parent))
                {
                    AdvancedDropdownItem item = targetFolder.children.FirstOrDefault(dropdownItem =>
                        dropdownItem.name.Equals(parent.text, StringComparison.Ordinal));

                    if (item == null)
                    {
                        item = new AdvancedDropdownItem(parent.text)
                        {
                            icon = (Texture2D)parent.image
                        };
                        targetFolder.AddChild(item);
                    }

                    targetFolder = item;
                }

                PrimeTweenActionAdvancedDropdownItem doTweenActionAdvancedDropdownItem =
                    new PrimeTweenActionAdvancedDropdownItem(basePrimeTweenActionType, typeToDisplayGUI.Value.text)
                    {
                        enabled = !IsTypeAlreadyInUse(actionsList, basePrimeTweenActionType) && AnimationSequenceEditorGUIUtility.CanPrimeTweenActionBeAppliedToTarget(basePrimeTweenActionType, targetGameObject)
                    };

                if (typeToDisplayGUI.Value.image != null)
                {
                    doTweenActionAdvancedDropdownItem.icon = (Texture2D)typeToDisplayGUI.Value.image;
                }
                targetFolder.AddChild(doTweenActionAdvancedDropdownItem);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            callback?.Invoke(item as PrimeTweenActionAdvancedDropdownItem);
        }

        public void Show(Rect rect, SerializedProperty actionsListSerializedProperty, Object targetGameObject, Action<PrimeTweenActionAdvancedDropdownItem>
        onActionSelectedCallback)
        {
            callback = onActionSelectedCallback;
            this.actionsList = actionsListSerializedProperty;
            if (targetGameObject is GameObject target)
                this.targetGameObject = target;
            base.Show(rect);
        }

        private bool IsTypeAlreadyInUse(SerializedProperty actionsSerializedProperty, Type targetType)
        {
            if (string.IsNullOrEmpty(targetType.FullName))
                return false;
            for (int i = 0; i < actionsSerializedProperty.arraySize; i++)
            {
                SerializedProperty actionElement = actionsSerializedProperty.GetArrayElementAtIndex(i);
                if (actionElement.managedReferenceFullTypename.IndexOf(targetType.FullName, StringComparison.Ordinal) > -1)
                    return true;
            }

            return false;
        }
    }
}
#endif