using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    public static partial class AnimationSequenceEditorGUIUtility
    {
        private static Dictionary<Type, GUIContent> cachedTypeToDisplayName;
        public static Dictionary<Type, GUIContent> TypeToDisplayName
        {
            get
            {
                CacheDisplayTypes();
                return cachedTypeToDisplayName;
            }
        }

        private static Dictionary<Type, GUIContent> cachedTypeToInstance;
        public static Dictionary<Type, GUIContent> TypeToParentDisplay
        {
            get
            {
                CacheDisplayTypes();
                return cachedTypeToInstance;
            }
        }

        private static Dictionary<Type, object> typeToInstanceCache;
        public static Dictionary<Type, object> TypeToInstanceCache
        {
            get
            {
                CacheDisplayTypes();
                return typeToInstanceCache;
            }
        }

        public static GUIContent GetTypeDisplayName(Type targetBaseTweenType)
        {
            if (TypeToDisplayName.TryGetValue(targetBaseTweenType, out GUIContent result))
                return result;

            return new GUIContent(targetBaseTweenType.Name);
        }

        private static void CacheDisplayTypes()
        {
            if (cachedTypeToDisplayName != null)
                return;

            cachedTypeToDisplayName = new Dictionary<Type, GUIContent>();
            cachedTypeToInstance = new Dictionary<Type, GUIContent>();
            typeToInstanceCache = new Dictionary<Type, object>();

#if DOTWEEN_ENABLED
            AppendDOTweenDisplayTypes();
#elif PRIMETWEEN_ENABLED
            AppendPrimeTweenDisplayTypes();
#endif
        }

        public static bool CanActionBeAppliedToTarget(Type targetActionType, GameObject targetGameObject)
        {
            if (targetGameObject == null)
                return false;

#if DOTWEEN_ENABLE  
            return PrimeTweenAnimationSequenceEditorGUIUtility.CanDOTweenActionBeAppliedToTarget(targetActionType, targetGameObject);
#elif PRIMETWEEN_ENABLED
            return AnimationSequenceEditorGUIUtility.CanPrimeTweenActionBeAppliedToTarget(targetActionType, targetGameObject);
#endif
            return false;
        }

        private static GUIContent cachedBackButtonGUIContent;
        internal static GUIContent BackButtonGUIContent
        {
            get
            {
                if (cachedBackButtonGUIContent == null)
                {
                    cachedBackButtonGUIContent = EditorGUIUtility.IconContent("d_beginButton");
                    cachedBackButtonGUIContent.tooltip = "Rewind";
                }

                return cachedBackButtonGUIContent;
            }
        }

        private static GUIContent cachedStepBackGUIContent;
        internal static GUIContent StepBackGUIContent
        {
            get
            {
                if (cachedStepBackGUIContent == null)
                {
                    cachedStepBackGUIContent = EditorGUIUtility.IconContent("Animation.PrevKey");
                    cachedStepBackGUIContent.tooltip = "Step Back";
                }

                return cachedStepBackGUIContent;
            }
        }

        private static GUIContent cachedStepNextGUIContent;
        internal static GUIContent StepNextGUIContent
        {
            get
            {
                if (cachedStepNextGUIContent == null)
                {
                    cachedStepNextGUIContent = EditorGUIUtility.IconContent("Animation.NextKey");
                    cachedStepNextGUIContent.tooltip = "Step Next";
                }

                return cachedStepNextGUIContent;
            }
        }

        private static GUIContent cachedStopButtonGUIContent;
        internal static GUIContent StopButtonGUIContent
        {
            get
            {
                if (cachedStopButtonGUIContent == null)
                {
                    cachedStopButtonGUIContent = EditorGUIUtility.IconContent("animationdopesheetkeyframe");
                    cachedStopButtonGUIContent.tooltip = "Stop";
                }
                return cachedStopButtonGUIContent;
            }
        }

        private static GUIContent cachedForwardButtonGUIContent;
        internal static GUIContent ForwardButtonGUIContent
        {
            get
            {
                if (cachedForwardButtonGUIContent == null)
                {
                    cachedForwardButtonGUIContent = EditorGUIUtility.IconContent("d_endButton");
                    cachedForwardButtonGUIContent.tooltip = "Fast Forward";
                }
                return cachedForwardButtonGUIContent;
            }
        }

        private static GUIContent cachedPauseButtonGUIContent;
        internal static GUIContent PauseButtonGUIContent
        {
            get
            {
                if (cachedPauseButtonGUIContent == null)
                {
                    cachedPauseButtonGUIContent = EditorGUIUtility.IconContent("PauseButton On");
                    cachedPauseButtonGUIContent.tooltip = "Pause";
                }
                return cachedPauseButtonGUIContent;
            }
        }

        private static GUIContent cachedPlayButtonGUIContent;
        internal static GUIContent PlayButtonGUIContent
        {
            get
            {
                if (cachedPlayButtonGUIContent == null)
                {
                    cachedPlayButtonGUIContent = EditorGUIUtility.IconContent("PlayButton On");
                    cachedPlayButtonGUIContent.tooltip = "Play";
                }
                return cachedPlayButtonGUIContent;
            }
        }

        private static GUIContent cachedSaveAsDefaultGUIContent;
        internal static GUIContent SaveAsDefaultButtonGUIContent
        {
            get
            {
                if (cachedSaveAsDefaultGUIContent == null)
                {
                    cachedSaveAsDefaultGUIContent = EditorGUIUtility.IconContent("d_SaveAs");
                    cachedSaveAsDefaultGUIContent.tooltip = "Save as Default";
                }
                return cachedSaveAsDefaultGUIContent;
            }
        }
    }
}