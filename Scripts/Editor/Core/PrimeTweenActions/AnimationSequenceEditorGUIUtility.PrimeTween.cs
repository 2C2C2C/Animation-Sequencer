#if PRIMETWEEN_ENABLED
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    public static partial class AnimationSequenceEditorGUIUtility
    {
        private static PrimeTweenActionsAdvancedDropdown cachedPrimeTweenActionsDropdown;
        public static PrimeTweenActionsAdvancedDropdown PrimeTweenActionsDropdown
        {
            get
            {
                if (cachedPrimeTweenActionsDropdown == null)
                    cachedPrimeTweenActionsDropdown = new PrimeTweenActionsAdvancedDropdown(new AdvancedDropdownState());
                return cachedPrimeTweenActionsDropdown;
            }
        }

        public static bool CanPrimeTweenActionBeAppliedToTarget(Type targetActionType, GameObject targetGameObject)
        {
            if (targetGameObject == null)
                return false;

            if (TypeToInstanceCache.TryGetValue(targetActionType, out object result) && result is PrimeTweenActionBase actionBaseInstance)
            {
                Type requiredComponent = actionBaseInstance.TargetComponentType;
                if (requiredComponent == typeof(Transform))
                    return true;

                if (requiredComponent == typeof(RectTransform))
                    return targetGameObject.transform is RectTransform;

                return targetGameObject.GetComponent(requiredComponent) != null;
            }
            return false;
        }

        private static void AppendPrimeTweenDisplayTypes()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(typeof(PrimeTweenActionBase));
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract)
                    continue;

                PrimeTweenActionBase doTweenActionBaseInstance = Activator.CreateInstance(type) as PrimeTweenActionBase;
                if (doTweenActionBaseInstance == null)
                    continue;
                GUIContent guiContent = new GUIContent(doTweenActionBaseInstance.DisplayName);
                if (doTweenActionBaseInstance.TargetComponentType != null)
                {
                    GUIContent targetComponentGUIContent = EditorGUIUtility.ObjectContent(null, doTweenActionBaseInstance.TargetComponentType);
                    guiContent.image = targetComponentGUIContent.image;
                    GUIContent parentGUIContent = new GUIContent(doTweenActionBaseInstance.TargetComponentType.Name)
                    {
                        image = targetComponentGUIContent.image
                    };
                    cachedTypeToInstance.Add(type, parentGUIContent);
                }

                cachedTypeToDisplayName.Add(type, guiContent);
                typeToInstanceCache.Add(type, doTweenActionBaseInstance);
            }
        }

    }
}
#endif