#if DOTWEEN_ENABLED
using DG.DOTweenEditor;
using DG.Tweening;
#elif PRIMETWEEN_ENABLED
using PrimeTween;
#endif
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer
{
    [CustomEditor(typeof(AnimationSequencerController), true)]
    public class AnimationSequencerControllerCustomEditor : Editor
    {
        private const float STEP_PRECENTAGE = 0.01f;

        private static readonly GUIContent CollapseAllAnimationStepsContent = new GUIContent("▸◂", "Collapse all animation steps");
        private static readonly GUIContent ExpandAllAnimationStepsContent = new GUIContent("◂▸", "Expand all animation steps");

        private ReorderableList reorderableList;

        private AnimationSequencerController sequencerController;

        private static AnimationStepAdvancedDropdown cachedAnimationStepsDropdown;
        private static AnimationStepAdvancedDropdown AnimationStepAdvancedDropdown
        {
            get
            {
                if (cachedAnimationStepsDropdown == null)
                    cachedAnimationStepsDropdown = new AnimationStepAdvancedDropdown(new AdvancedDropdownState());
                return cachedAnimationStepsDropdown;
            }
        }

        private bool showPreviewPanel = true;
        private bool showSettingsPanel;
        private bool showCallbacksPanel;
        private bool showSequenceSettingsPanel;
        private bool showStepsPanel = true;
        private float tweenTimeScale = 1f;
        private bool wasShowingStepsPanel;
        private bool justStartPreviewing;

        private (float start, float end)[] previewingTimings;

        private void OnEnable()
        {
#if PRIMETWEEN_ENABLED  
            TempPrimeTweenEditorPreview.StopAll();
#endif
            sequencerController = target as AnimationSequencerController;
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("animationSteps"), true, false, true, true);
            reorderableList.drawElementCallback += OnDrawAnimationStep;
            reorderableList.drawElementBackgroundCallback += OnDrawAnimationStepBackground;
            reorderableList.elementHeightCallback += GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback += OnClickToAddNew;
            reorderableList.onRemoveCallback += OnClickToRemove;
            reorderableList.onReorderCallback += OnListOrderChanged;
            reorderableList.drawHeaderCallback += OnDrawerHeader;
            EditorApplication.update += EditorUpdate;
            EditorApplication.playModeStateChanged += OnEditorPlayModeChanged;

#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#endif

            Repaint();
        }

        public override bool RequiresConstantRepaint()
        {
#if DOTWEEN_ENABLED
            return DOTweenEditorPreview.isPreviewing;
#elif PRIMETWEEN_ENABLED                   
            // TODO @Hiko
#endif
            return false;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        private void OnDisable()
        {
#if PRIMETWEEN_ENABLED
            TempPrimeTweenEditorPreview.StopAll();
#endif
            reorderableList.drawElementCallback -= OnDrawAnimationStep;
            reorderableList.drawElementBackgroundCallback -= OnDrawAnimationStepBackground;
            reorderableList.elementHeightCallback -= GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback -= OnClickToAddNew;
            reorderableList.onRemoveCallback -= OnClickToRemove;
            reorderableList.onReorderCallback -= OnListOrderChanged;
            reorderableList.drawHeaderCallback -= OnDrawerHeader;
            EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;
            EditorApplication.update -= EditorUpdate;
#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#endif

            if (!Application.isPlaying)
            {
#if DOTWEEN_ENABLED
                if (DOTweenEditorPreview.isPreviewing)
                {
                    sequencerController.ResetToInitialState();
                    DOTweenEditorPreview.Stop();            
                }
#elif PRIMETWEEN_ENABLED
                // TODO @Hiko
#endif
            }

            tweenTimeScale = 1f;
        }

        private void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

#if DOTWEEN_ENABLED
            //SerializedProperty progressSP = serializedObject.FindProperty("progress");
            //if (progressSP == null || Mathf.Approximately(progressSP.floatValue, -1))
            //    return;

            //SetProgress(progressSP.floatValue);
#elif PRIMETWEEN_ENABLED
            if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
            {
                // previewer will sample it
                //sequencerController.SampleTo(TempPrimeTweenEditorPreview.normalizedPreviewProgress);
            }
#endif
        }

        private void OnEditorPlayModeChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.ExitingEditMode)
            {
#if DOTWEEN_ENABLED
                if (DOTweenEditorPreview.isPreviewing)
                {
                    sequencerController.ResetToInitialState();
                    DOTweenEditorPreview.Stop();
                }
#elif PRIMETWEEN_ENABLED
                if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                {
                    TempPrimeTweenEditorPreview.Stop(sequencerController);
                    sequencerController.ResetToInitialState();
                }
#endif
            }
        }

        private void PrefabSaving(GameObject gameObject)
        {
#if DOTWEEN_ENABLED
            if (DOTweenEditorPreview.isPreviewing)
            {
                sequencerController.ResetToInitialState();
                DOTweenEditorPreview.Stop();
            }
#elif PRIMETWEEN_ENABLED
            if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
            {
                TempPrimeTweenEditorPreview.Stop(sequencerController);
                sequencerController.ResetToInitialState();
            }
#endif
        }

        private void OnDrawerHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Animation Steps");
        }

        private void AddNewAnimationStepOfType(Type targetAnimationType)
        {
            SerializedProperty animationStepsProperty = reorderableList.serializedProperty;
            int targetIndex = animationStepsProperty.arraySize;
            animationStepsProperty.InsertArrayElementAtIndex(targetIndex);
            SerializedProperty arrayElementAtIndex = animationStepsProperty.GetArrayElementAtIndex(targetIndex);
            object managedReferenceValue = Activator.CreateInstance(targetAnimationType);
            arrayElementAtIndex.managedReferenceValue = managedReferenceValue;

            //TODO copy from last step would be better here.
            SerializedProperty targetSerializedProperty = arrayElementAtIndex.FindPropertyRelative("target");
            if (targetSerializedProperty != null)
                targetSerializedProperty.objectReferenceValue = (serializedObject.targetObject as AnimationSequencerController)?.gameObject;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnClickToRemove(ReorderableList list)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
            SerializedPropertyExtensions.ClearPropertyCache(element.propertyPath);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void OnListOrderChanged(ReorderableList list)
        {
            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void OnClickToAddNew(Rect buttonRect, ReorderableList list)
        {
            AnimationStepAdvancedDropdown.Show(buttonRect, OnNewAnimationStepTypeSelected);
        }

        private void OnNewAnimationStepTypeSelected(AnimationStepAdvancedDropdownItem animationStepAdvancedDropdownItem)
        {
            AddNewAnimationStepOfType(animationStepAdvancedDropdownItem.AnimationStepType);
        }

        public override void OnInspectorGUI()
        {
            DrawFoldoutArea("Settings", ref showSettingsPanel, DrawSettings, DrawSettingsHeader);
            DrawFoldoutArea("Callback", ref showCallbacksPanel, DrawCallbacks);
            DrawFoldoutArea("Preview", ref showPreviewPanel, DrawPreviewControls);
            DrawFoldoutArea("Steps", ref showStepsPanel, DrawAnimationSteps, DrawAnimationStepsHeader, 50);
        }

        private void DrawAnimationStepsHeader(Rect rect, bool foldout)
        {
            if (!foldout)
                return;

            var collapseAllRect = new Rect(rect)
            {
                xMin = rect.xMax - 50,
                xMax = rect.xMax - 25,
            };

            var expandAllRect = new Rect(rect)
            {
                xMin = rect.xMax - 25,
                xMax = rect.xMax - 0,
            };

            if (GUI.Button(collapseAllRect, CollapseAllAnimationStepsContent, EditorStyles.miniButtonLeft))
            {
                SetStepsExpanded(false);
            }

            if (GUI.Button(expandAllRect, ExpandAllAnimationStepsContent, EditorStyles.miniButtonRight))
            {
                SetStepsExpanded(true);
            }
        }

        private void DrawAnimationSteps()
        {
            bool wasGUIEnabled = GUI.enabled;
#if DOTWEEN_ENABLED
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;
#elif PRIMETWEEN_ENABLED
            if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                GUI.enabled = false;
#endif
            reorderableList.DoLayoutList();
            GUI.enabled = wasGUIEnabled;
        }

        protected virtual void DrawCallbacks()
        {
            bool wasGUIEnabled = GUI.enabled;
#if DOTWEEN_ENABLED
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;
#elif PRIMETWEEN_ENABLED
            if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                GUI.enabled = false;
#endif
            SerializedProperty onStartEventSerializedProperty = serializedObject.FindProperty("onStartEvent");
            SerializedProperty onFinishedEventSerializedProperty = serializedObject.FindProperty("onFinishedEvent");
            SerializedProperty onProgressEventSerializedProperty = serializedObject.FindProperty("onProgressEvent");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(onStartEventSerializedProperty);
                EditorGUILayout.PropertyField(onFinishedEventSerializedProperty);
                EditorGUILayout.PropertyField(onProgressEventSerializedProperty);

                if (changedCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }

            GUI.enabled = wasGUIEnabled;
        }

        private void DrawSettingsHeader(Rect rect, bool foldout)
        {
            SerializedProperty autoPlayModeSerializedProperty = serializedObject.FindProperty("autoplayMode");
            SerializedProperty autoKillSerializedProperty = serializedObject.FindProperty("autoKill");
            AutoplayType autoplayMode = (AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;

            if (autoKillSerializedProperty.boolValue) // auto kill
                rect = DrawAutoSizedBadgeRight(rect, "Auto Kill", new Color(1f, 0.2f, 0f, 0.6f));

            if (autoplayMode == AutoplayType.Awake)
                DrawAutoSizedBadgeRight(rect, "AutoPlay on Awake", new Color(1f, 0.7f, 0f, 0.6f));
            else if (autoplayMode == AutoplayType.OnEnable)
                DrawAutoSizedBadgeRight(rect, "AutoPlay on Enable", new Color(1f, 0.7f, 0f, 0.6f));
        }

        private void DrawSettings()
        {
            SerializedProperty autoPlayModeSerializedProperty = serializedObject.FindProperty("autoplayMode");
            SerializedProperty pauseOnAwakeSerializedProperty = serializedObject.FindProperty("startPaused");

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                AutoplayType autoplayMode = (AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
                EditorGUILayout.PropertyField(autoPlayModeSerializedProperty);

                if (autoplayMode != AutoplayType.Nothing)
                    EditorGUILayout.PropertyField(pauseOnAwakeSerializedProperty);

                DrawPlaybackSpeedSlider();

                if (changedCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }

            bool wasEnabled = GUI.enabled;
#if DOTWEEN_ENABLED
            if (DOTweenEditorPreview.isPreviewing)
                GUI.enabled = false;
#elif PRIMETWEEN_ENABLED
            if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                GUI.enabled = false;
#endif

            // TODO prime tween can also change to fixed update
#if DOTWEEN_ENABLED
            SerializedProperty updateTypeSerializedProperty = serializedObject.FindProperty("updateType");
#endif
            SerializedProperty timeScaleIndependentSerializedProperty = serializedObject.FindProperty("timeScaleIndependent");
            SerializedProperty loopsSerializedProperty = serializedObject.FindProperty("loops");
            SerializedProperty loopTypeSerializedProperty = serializedObject.FindProperty("loopType");
#if DOTWEEN_ENABLED // Prime tween is default autokill
            SerializedProperty autoKillSerializedProperty = serializedObject.FindProperty("autoKill");
#endif

            using (EditorGUI.ChangeCheckScope changedCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(timeScaleIndependentSerializedProperty);
#if DOTWEEN_ENABLED
                EditorGUILayout.PropertyField(autoKillSerializedProperty);
#endif
                EditorGUILayout.PropertyField(loopsSerializedProperty);

                if (loopsSerializedProperty.intValue != 0)
                {
                    EditorGUILayout.PropertyField(loopTypeSerializedProperty);
                }

                if (changedCheck.changed)
                {
                    loopsSerializedProperty.intValue = Mathf.Clamp(loopsSerializedProperty.intValue, -1, int.MaxValue);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            GUI.enabled = wasEnabled;
        }

        private void DrawPlaybackSpeedSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            SerializedProperty playbackSpeedProperty = serializedObject.FindProperty("playbackSpeed");
            playbackSpeedProperty.floatValue = EditorGUILayout.Slider("Playback Speed", playbackSpeedProperty.floatValue, 0, 2);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateSequenceTimeScale();
            }

            GUILayout.FlexibleSpace();
        }

        private void UpdateSequenceTimeScale()
        {
#if DOTWEEN_ENABLED
            if (sequencerController.PlayingSequence == null)
                return;

            sequencerController.PlayingSequence.timeScale = sequencerController.PlaybackSpeed * tweenTimeScale;
#elif PRIMETWEEN_ENABLED
            // TODO
#endif
        }

        private void DrawPreviewControls()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool guiEnabled = GUI.enabled;
            GUIStyle previewButtonStyle = new GUIStyle(GUI.skin.button);
            previewButtonStyle.fixedWidth = previewButtonStyle.fixedHeight = 40;
            bool isSequencePlaying = sequencerController.IsPlaying;

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StepBackGUIContent, previewButtonStyle))
            {
                if (!isSequencePlaying)
                    PlaySequence();

                StepBack();
            }

            bool isApplicationPlaying = Application.isPlaying;
#if DOTWEE_ENABLED
                if (isSequencePlaying)
                {
                    if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle))
                    {
                        sequencerController.Pause();
                    }
                }
                else
                {
                    if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PlayButtonGUIContent, previewButtonStyle))
                    {
                        PlaySequence();
                    }
                }
            }
#elif PRIMETWEEN_ENABLED
            if (isApplicationPlaying)
            {
                if (isSequencePlaying && GUILayout.Button(AnimationSequenceEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle))
                {
                    sequencerController.Pause();
                }
                else if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PlayButtonGUIContent, previewButtonStyle))
                {
                    PlaySequence();
                }
            }
            else // TODO @Hiko those controls should be also apply to prime tween preview
            {
                if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                {
                    if (!TempPrimeTweenEditorPreview.IsPaused(sequencerController))
                    {
                        if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle))
                        {
                            TempPrimeTweenEditorPreview.Pause(sequencerController);
                        }
                    }
                    else // paused
                    {
                        if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PlayButtonGUIContent, previewButtonStyle))
                        {
                            TempPrimeTweenEditorPreview.Resume(sequencerController);
                        }
                    }
                }
                else if (GUILayout.Button(AnimationSequenceEditorGUIUtility.PlayButtonGUIContent, previewButtonStyle))
                {
                    TempPrimeTweenEditorPreview.Start(sequencerController);
                }
            }
#endif

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StepNextGUIContent, previewButtonStyle))
            {
                if (!isSequencePlaying)
                    PlaySequence();

                StepNext();
            }

            if (GUILayout.Button(AnimationSequenceEditorGUIUtility.ForwardButtonGUIContent, previewButtonStyle))
            {
                if (!isSequencePlaying)
                    PlaySequence();

                sequencerController.Complete();
            }

            if (!isApplicationPlaying) // in editor time
            {
#if DOTWEEN_ENABLED
                GUI.enabled = DOTweenEditorPreview.isPreviewing;
                if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StopButtonGUIContent, previewButtonStyle))
                {
                    sequencerController.Rewind();
                    DOTween.Kill(sequencerController.PlayingSequence);
                    DOTweenEditorPreview.Stop();
                    sequencerController.ResetToInitialState();
                    sequencerController.ClearPlayingSequence();
                    if (AnimationSequencerSettings.GetInstance().AutoHideStepsWhenPreviewing)
                        showStepsPanel = wasShowingStepsPanel;
                }
#elif PRIMETWEEN_ENABLED
                // TODO @Hiko
                if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                {
                    if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StopButtonGUIContent, previewButtonStyle))
                    {
                        TempPrimeTweenEditorPreview.Stop(sequencerController);
                    }
                }
#endif
            }
            else if (isSequencePlaying)
            {
                if (GUILayout.Button(AnimationSequenceEditorGUIUtility.StopButtonGUIContent, previewButtonStyle))
                {
                    sequencerController.Kill();
                }
            }

            GUI.enabled = guiEnabled;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            DrawTimeScaleSlider();
            DrawProgressSlider();
        }

        private void StepBack()
        {
#if DOTWEEN_ENABLED
            if (!sequencerController.IsPlaying)
                PlaySequence();
            sequencerController.PlayingSequence.Goto((sequencerController.PlayingSequence.ElapsedPercentage() -
                                                      STEP_PRECENTAGE) * sequencerController.PlayingSequence.Duration());
#elif PRIMETWEEN_ENABLED
            if (Application.isPlaying)
            {
                if (sequencerController.HasValidSequence)
                {
                    Sequence sequence = sequencerController.PlayingSequence;
                    sequence.elapsedTime -= sequence.duration * STEP_PRECENTAGE;
                }
                else
                {
                    PlaySequence();
                }
            }
            else
            {
                if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController) && TempPrimeTweenEditorPreview.TryGetProgress(sequencerController, out float current))
                {
                    current -= STEP_PRECENTAGE;
                    TempPrimeTweenEditorPreview.SampleProgress(sequencerController, current);
                }
                else
                {
                    sequencerController.SetProgress(sequencerController.TempProgress - STEP_PRECENTAGE);
                }
            }
#endif
        }

        private void StepNext()
        {
#if DOTWEEN_ENABLED
            if (!sequencerController.IsPlaying)
                PlaySequence();
            sequencerController.PlayingSequence.Goto((sequencerController.PlayingSequence.ElapsedPercentage() +
                                                      STEP_PRECENTAGE) * sequencerController.PlayingSequence.Duration());
#elif PRIMETWEEN_ENABLED
            if (Application.isPlaying)
            {
                if (sequencerController.HasValidSequence)
                {
                    Sequence sequence = sequencerController.PlayingSequence;
                    sequence.elapsedTime += sequence.duration * STEP_PRECENTAGE;
                }
                else
                {
                    PlaySequence();
                }
            }
            else
            {
                if (TempPrimeTweenEditorPreview.IsPreviewing(sequencerController) && TempPrimeTweenEditorPreview.TryGetProgress(sequencerController, out float current))
                {
                    current -= STEP_PRECENTAGE;
                    TempPrimeTweenEditorPreview.SampleProgress(sequencerController, current);
                }
                else
                {
                    sequencerController.SetProgress(sequencerController.TempProgress + STEP_PRECENTAGE);
                }
            }
#endif
        }

        private void PlaySequence()
        {
            justStartPreviewing = false;
            if (Application.isPlaying)
            {
#if DOTWEEN_ENABLED
                if (sequencerController.PlayingSequence == null)
                    sequencerController.Play();
                else
                {
                    if (sequencerController.PlayingSequence.IsActive())
                        sequencerController.TogglePause();
                    else
                        sequencerController.Play();
                }
#elif PRIMETWEEN_ENABLED
                if (sequencerController.PlayingSequence.isAlive)
                {
                    if (sequencerController.PlayingSequence.isPaused)
                        sequencerController.Resume();
                }
                sequencerController.Play();
#endif
            }
            else // in editor mode
            {
#if DOTWEEN_ENABLED
                if (!DOTweenEditorPreview.isPreviewing)
                {
                    justStartPreviewing = true;
                    DOTweenEditorPreview.Start();

                    sequencerController.Play();
                    DOTweenEditorPreview.PrepareTweenForPreview(sequencerController.PlayingSequence);

                    if (AnimationSequencerSettings.GetInstance().DrawTimingsWhenPreviewing)
                        previewingTimings = DOTweenProxy.GetTimings(sequencerController.PlayingSequence, sequencerController.AnimationSteps);
                    else
                        previewingTimings = null;
                }
                else
                {
                    if (sequencerController.PlayingSequence == null)
                    {
                        sequencerController.Play();
                    }
                    else
                    {
                        if (!sequencerController.PlayingSequence.IsBackwards() &&
                            sequencerController.PlayingSequence.fullPosition >= sequencerController.PlayingSequence.Duration())
                        {
                            sequencerController.Rewind();
                        }
                        else if (sequencerController.PlayingSequence.IsBackwards() && sequencerController.PlayingSequence.fullPosition <= 0f)
                        {
                            sequencerController.Complete();
                        }
                        sequencerController.TogglePause();
                    }
                }
#elif PRIMETWEEN_ENABLED
                if (!TempPrimeTweenEditorPreview.IsPreviewing(sequencerController))
                {
                    sequencerController.ResetToInitialState();
                    TempPrimeTweenEditorPreview.Start(sequencerController);
                }
#endif
            }

            if (justStartPreviewing)
                wasShowingStepsPanel = showStepsPanel;

            //showStepsPanel = !AnimationSequencerSettings.GetInstance().AutoHideStepsWhenPreviewing;
            // TODO
            showStepsPanel = true;
        }

        private void DrawProgressSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            float tweenProgress = GetCurrentSequencerProgress();
#if PRIMETWEEN_ENABLED
            if (!Application.isPlaying && TempPrimeTweenEditorPreview.IsPreviewing(sequencerController) &&
                TempPrimeTweenEditorPreview.TryGetProgress(sequencerController, out float innerProgress))
            {
                tweenProgress = innerProgress;
            }
#endif
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenProgress = EditorGUILayout.Slider("Progress", tweenProgress, 0, 1);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            if (EditorGUI.EndChangeCheck())
            {
                SetProgress(tweenProgress);
                if (!Application.isPlaying)
                {
                    serializedObject.FindProperty("progress").floatValue = tweenProgress;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUILayout.FlexibleSpace();
        }

        private void SetProgress(float tweenProgress)
        {
#if DOTWEEN_ENABLED
            if (!sequencerController.IsPlaying)
                PlaySequence();
            sequencerController.PlayingSequence.Goto(tweenProgress * sequencerController.PlayingSequence.Duration());
#elif PRIMETWEEN_ENABLED
            sequencerController.SetProgress(tweenProgress);
#endif
        }

        private float GetCurrentSequencerProgress()
        {
            float tweenProgress = 0f;
#if DOTWEEN_ENABLED
            if (sequencerController.PlayingSequence != null && sequencerController.PlayingSequence.IsActive())
                tweenProgress = sequencerController.PlayingSequence.ElapsedPercentage();
            else
                tweenProgress = 0;
#elif PRIMETWEEN_ENABLED
            tweenProgress = Application.isPlaying ?
                (sequencerController.HasValidSequence ? sequencerController.PlayingSequence.progress : 0f) :
                sequencerController.TempProgress;
#endif
            return tweenProgress;
        }

        private void DrawTimeScaleSlider()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenTimeScale = EditorGUILayout.Slider("TimeScale", tweenTimeScale, 0, 2);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            UpdateSequenceTimeScale();
            GUILayout.FlexibleSpace();
        }

        private void DrawFoldoutArea(string title, ref bool foldout, Action additionalInspectorGUI,
            Action<Rect, bool> additionalHeaderGUI = null, float additionalHeaderWidth = 0)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            if (Event.current.type == EventType.Repaint)
            {
                GUI.skin.box.Draw(rect, false, false, false, false);
            }

            using (new EditorGUILayout.VerticalScope(AnimationSequencerStyles.InspectorSideMargins))
            {
                Rect rectWithMargins = new Rect(rect)
                {
                    xMin = rect.xMin + AnimationSequencerStyles.InspectorSideMargins.padding.left,
                    xMax = rect.xMax - AnimationSequencerStyles.InspectorSideMargins.padding.right,
                };

                Rect foldoutRect = new Rect(rectWithMargins)
                {
                    xMax = rectWithMargins.xMax - additionalHeaderWidth,
                };

                Rect additionalHeaderRect = new Rect(rectWithMargins)
                {
                    xMin = foldoutRect.xMax,
                };

                foldout = EditorGUI.Foldout(foldoutRect, foldout, title, true);
                additionalHeaderGUI?.Invoke(additionalHeaderRect, foldout);
                if (foldout)
                {
                    additionalInspectorGUI.Invoke();
                    GUILayout.Space(10);
                }
            }
        }

        private void OnDrawAnimationStepBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Rect titlebarRect = new Rect(rect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };

                if (isActive)
                    ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, isFocused, false);
                else
                    AnimationSequencerStyles.InspectorTitlebar.Draw(titlebarRect, false, false, false, false);
            }

            if (Event.current.type == EventType.Repaint &&
#if DOTWEEN_ENABLED // TODO
                DOTweenEditorPreview.isPreviewing &&
#elif PRIMETWEEN_ENABLED
               TempPrimeTweenEditorPreview.IsPreviewing(sequencerController) &&
#endif
                previewingTimings != null &&
                index >= 0 && index < previewingTimings.Length)
            {
                (float start, float end) = previewingTimings[index];

                float progress = GetCurrentSequencerProgress();
                Rect progressRect = new Rect(rect)
                {
                    xMin = Mathf.Lerp(rect.xMin, rect.xMax, start) - 1,
                    xMax = Mathf.Lerp(rect.xMin, rect.xMax, end) + 1,
                    height = EditorGUIUtility.singleLineHeight,
                };

                Rect markerRect = new Rect(rect)
                {
                    xMin = Mathf.Lerp(rect.xMin, rect.xMax, progress) - 1,
                    xMax = Mathf.Lerp(rect.xMin, rect.xMax, progress) + 1,
                    height = EditorGUIUtility.singleLineHeight,
                };

                Color oldColor = GUI.color;
                GUI.color = new Color(0f, 0.5f, 0f, 0.45f);
                GUI.DrawTexture(progressRect, EditorGUIUtility.whiteTexture);

                GUI.color = Color.black;
                GUI.DrawTexture(markerRect, EditorGUIUtility.whiteTexture);
                GUI.color = oldColor;
            }
        }

        private void OnDrawAnimationStep(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty flowTypeSerializedProperty = element.FindPropertyRelative("flowType");

            if (!element.TryGetTargetObjectOfProperty(out AnimationStepBase animationStepBase))
                return;

            FlowType flowType = (FlowType)flowTypeSerializedProperty.enumValueIndex;
            int baseIdentLevel = EditorGUI.indentLevel;

            GUIContent guiContent = new GUIContent(element.displayName);
            if (animationStepBase != null)
                guiContent = new GUIContent(animationStepBase.GetDisplayNameForEditor(index + 1));

            if (flowType == FlowType.Join)
                EditorGUI.indentLevel = baseIdentLevel + 1;

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 10;
            rect.width -= 20;

            EditorGUI.PropertyField(
                rect,
                element,
                guiContent,
                false
            );

            EditorGUI.indentLevel = baseIdentLevel;
            // DrawContextInputOnItem(element, index, rect);
        }

        private float GetAnimationStepHeight(int index)
        {
            if (index > reorderableList.serializedProperty.arraySize - 1)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return element.GetPropertyDrawerHeight();
        }

        private void SetStepsExpanded(bool expanded)
        {
            SerializedProperty animationStepsProperty = reorderableList.serializedProperty;
            for (int i = 0; i < animationStepsProperty.arraySize; i++)
            {
                animationStepsProperty.GetArrayElementAtIndex(i).isExpanded = expanded;
            }
        }

        private static Rect DrawAutoSizedBadgeRight(Rect rect, string text, Color color)
        {
            GUIStyle style = AnimationSequencerStyles.Badge;
            Vector2 size = style.CalcSize(EditorGUIUtility.TrTempContent(text));
            Rect buttonRect = new Rect(rect)
            {
                xMin = rect.xMax - size.x,
            };

            if (Event.current.type == EventType.Repaint)
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
                style.Draw(buttonRect, text, false, false, true, false);
                GUI.backgroundColor = oldColor;
            }

            return new Rect(rect)
            {
                xMax = rect.xMax - size.x - style.margin.left,
            };
        }
    }
}