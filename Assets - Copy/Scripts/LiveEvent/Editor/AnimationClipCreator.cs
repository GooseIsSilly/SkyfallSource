using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace TPSBR
{
    public class AnimationClipCreator : EditorWindow
    {
        private GameObject _targetObject;
        private string _animationName = "NewAnimation";
        private AnimationType _animationType = AnimationType.FadeIn;
        
        private enum AnimationType
        {
            FadeIn,
            FadeOut,
            SlideInFromLeft,
            SlideInFromRight,
            SlideInFromBottom,
            ScalePulse,
            SpinAndScale,
            TypeWriter
        }
        
        [MenuItem("Tools/Live Event/Create UI Animation Clip")]
        public static void ShowWindow()
        {
            AnimationClipCreator window = GetWindow<AnimationClipCreator>("Animation Creator");
            window.minSize = new Vector2(400, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("UI Animation Clip Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "Quick-create common UI animations for Era Intro!\n" +
                "Select a UI GameObject, choose animation type, and create!",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            _targetObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject", 
                _targetObject, 
                typeof(GameObject), 
                true
            );
            
            _animationName = EditorGUILayout.TextField("Animation Name", _animationName);
            
            _animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type", _animationType);
            
            GUILayout.Space(10);
            
            ShowAnimationPreview();
            
            GUILayout.Space(20);
            
            GUI.enabled = _targetObject != null;
            
            if (GUILayout.Button("Create Animation Clip", GUILayout.Height(40)))
            {
                CreateAnimationClip();
            }
            
            GUI.enabled = true;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Animation Window", GUILayout.Height(30)))
            {
                EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
            }
        }
        
        private void ShowAnimationPreview()
        {
            EditorGUILayout.HelpBox(GetAnimationDescription(), MessageType.None);
        }
        
        private string GetAnimationDescription()
        {
            switch (_animationType)
            {
                case AnimationType.FadeIn:
                    return "Fades element from transparent to opaque over 1 second.\n" +
                           "Good for: Text, images, UI panels";
                
                case AnimationType.FadeOut:
                    return "Fades element from opaque to transparent over 1 second.\n" +
                           "Good for: Hiding elements, transitions";
                
                case AnimationType.SlideInFromLeft:
                    return "Slides element from left side of screen over 1 second.\n" +
                           "Good for: Titles, character images";
                
                case AnimationType.SlideInFromRight:
                    return "Slides element from right side of screen over 1 second.\n" +
                           "Good for: Logos, UI panels";
                
                case AnimationType.SlideInFromBottom:
                    return "Slides element from bottom of screen over 1 second.\n" +
                           "Good for: Prompts, progress bars";
                
                case AnimationType.ScalePulse:
                    return "Pulses element scale (1.0 → 1.1 → 1.0) over 1 second. Loops.\n" +
                           "Good for: Prompt text, call-to-action buttons";
                
                case AnimationType.SpinAndScale:
                    return "Spins and scales up element over 2 seconds with overshoot.\n" +
                           "Good for: Logos, icons, epic reveals";
                
                case AnimationType.TypeWriter:
                    return "Reveals text character-by-character over 2 seconds.\n" +
                           "Good for: Dramatic text reveals (requires TextMeshPro)";
                
                default:
                    return "";
            }
        }
        
        private void CreateAnimationClip()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Animation Clip",
                _animationName,
                "anim",
                "Enter a name for the animation clip"
            );
            
            if (string.IsNullOrEmpty(path))
                return;
            
            AnimationClip clip = new AnimationClip();
            clip.frameRate = 60;
            
            switch (_animationType)
            {
                case AnimationType.FadeIn:
                    CreateFadeInAnimation(clip);
                    break;
                case AnimationType.FadeOut:
                    CreateFadeOutAnimation(clip);
                    break;
                case AnimationType.SlideInFromLeft:
                    CreateSlideInAnimation(clip, -1920f, 0f);
                    break;
                case AnimationType.SlideInFromRight:
                    CreateSlideInAnimation(clip, 1920f, 0f);
                    break;
                case AnimationType.SlideInFromBottom:
                    CreateSlideInAnimation(clip, 0f, -1080f, true);
                    break;
                case AnimationType.ScalePulse:
                    CreateScalePulseAnimation(clip);
                    break;
                case AnimationType.SpinAndScale:
                    CreateSpinAndScaleAnimation(clip);
                    break;
                case AnimationType.TypeWriter:
                    CreateTypeWriterAnimation(clip);
                    break;
            }
            
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = clip;
            
            Debug.Log($"[AnimationClipCreator] ✅ Created animation: {path}");
            EditorUtility.DisplayDialog("Success",
                $"Animation clip created!\n\n" +
                $"Next steps:\n" +
                "1. Open Timeline window\n" +
                "2. Add Animation Track\n" +
                "3. Drag this clip onto the track\n" +
                "4. Bind track to your GameObject",
                "OK");
        }
        
        private void CreateFadeInAnimation(AnimationClip clip)
        {
            AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            if (HasComponent<CanvasGroup>(_targetObject))
            {
                clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", alphaCurve);
            }
            else if (HasComponent<Image>(_targetObject))
            {
                AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                clip.SetCurve("", typeof(Image), "m_Color.a", colorCurve);
            }
            else if (HasComponent<TextMeshProUGUI>(_targetObject))
            {
                AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                clip.SetCurve("", typeof(TextMeshProUGUI), "m_fontColor.a", colorCurve);
            }
        }
        
        private void CreateFadeOutAnimation(AnimationClip clip)
        {
            AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            
            if (HasComponent<CanvasGroup>(_targetObject))
            {
                clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", alphaCurve);
            }
            else if (HasComponent<Image>(_targetObject))
            {
                clip.SetCurve("", typeof(Image), "m_Color.a", alphaCurve);
            }
            else if (HasComponent<TextMeshProUGUI>(_targetObject))
            {
                clip.SetCurve("", typeof(TextMeshProUGUI), "m_fontColor.a", alphaCurve);
            }
        }
        
        private void CreateSlideInAnimation(AnimationClip clip, float startX, float startY, bool isVertical = false)
        {
            RectTransform rect = _targetObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogWarning("[AnimationClipCreator] Target must have RectTransform for slide animation!");
                return;
            }
            
            Vector2 currentPos = rect.anchoredPosition;
            
            AnimationCurve posXCurve = isVertical ? 
                AnimationCurve.Linear(0f, currentPos.x, 1f, currentPos.x) :
                AnimationCurve.EaseInOut(0f, startX, 1f, currentPos.x);
            
            AnimationCurve posYCurve = isVertical ?
                AnimationCurve.EaseInOut(0f, startY, 1f, currentPos.y) :
                AnimationCurve.Linear(0f, currentPos.y, 1f, currentPos.y);
            
            AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", posXCurve);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", posYCurve);
            
            if (HasComponent<CanvasGroup>(_targetObject))
            {
                clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", alphaCurve);
            }
        }
        
        private void CreateScalePulseAnimation(AnimationClip clip)
        {
            AnimationCurve scaleCurve = new AnimationCurve();
            scaleCurve.AddKey(new Keyframe(0f, 1f, 0f, 0f));
            scaleCurve.AddKey(new Keyframe(0.5f, 1.1f, 0f, 0f));
            scaleCurve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
            
            for (int i = 0; i < scaleCurve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(scaleCurve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(scaleCurve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            
            clip.SetCurve("", typeof(Transform), "m_LocalScale.x", scaleCurve);
            clip.SetCurve("", typeof(Transform), "m_LocalScale.y", scaleCurve);
            
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }
        
        private void CreateSpinAndScaleAnimation(AnimationClip clip)
        {
            AnimationCurve scaleCurve = new AnimationCurve();
            scaleCurve.AddKey(new Keyframe(0f, 0f));
            scaleCurve.AddKey(new Keyframe(1.5f, 1.2f));
            scaleCurve.AddKey(new Keyframe(2f, 1f));
            
            AnimationCurve rotationCurve = AnimationCurve.Linear(0f, -180f, 2f, 0f);
            AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            clip.SetCurve("", typeof(Transform), "m_LocalScale.x", scaleCurve);
            clip.SetCurve("", typeof(Transform), "m_LocalScale.y", scaleCurve);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", rotationCurve);
            
            if (HasComponent<CanvasGroup>(_targetObject))
            {
                clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", alphaCurve);
            }
        }
        
        private void CreateTypeWriterAnimation(AnimationClip clip)
        {
            TextMeshProUGUI textComponent = _targetObject.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogWarning("[AnimationClipCreator] TypeWriter animation requires TextMeshProUGUI component!");
                return;
            }
            
            int charCount = Mathf.Max(textComponent.text.Length, 50);
            
            AnimationCurve charCurve = AnimationCurve.Linear(0f, 0f, 2f, charCount);
            clip.SetCurve("", typeof(TextMeshProUGUI), "m_maxVisibleCharacters", charCurve);
        }
        
        private bool HasComponent<T>(GameObject obj) where T : Component
        {
            return obj != null && obj.GetComponent<T>() != null;
        }
    }
}
