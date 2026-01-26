using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TimeRewind
{
    /// <summary>
    /// Provides visual and audio feedback during time rewind.
    /// Attach this to a GameObject to enable rewind effects.
    /// </summary>
    public class RewindEffects : MonoBehaviour
    {
        [Header("Visual Effects")]
        [Tooltip("Post-processing volume to modify during rewind (optional)")]
        [SerializeField] private Volume postProcessVolume;
        
        [Tooltip("Target saturation during rewind (-100 to 100, default is desaturated)")]
        [SerializeField] private float rewindSaturation = -50f;
        
        [Tooltip("Target chromatic aberration intensity during rewind (0-1)")]
        [SerializeField] private float rewindChromaticAberration = 0.5f;
        
        [Tooltip("Target vignette intensity during rewind (0-1)")]
        [SerializeField] private float rewindVignetteIntensity = 0.4f;
        
        [Tooltip("How fast effects transition in/out")]
        [SerializeField] private float effectTransitionSpeed = 5f;
        
        [Header("Screen Tint")]
        [Tooltip("Apply a color tint during rewind")]
        [SerializeField] private bool useScreenTint = true;
        
        [Tooltip("The color tint to apply")]
        [SerializeField] private Color rewindTintColor = new Color(0.5f, 0.7f, 1f, 0.2f);
        
        [Header("Audio Effects")]
        [Tooltip("Audio source for rewind sound effects")]
        [SerializeField] private AudioSource audioSource;
        
        [Tooltip("Looping sound to play during rewind")]
        [SerializeField] private AudioClip rewindLoopSound;
        
        [Tooltip("Sound to play when rewind starts")]
        [SerializeField] private AudioClip rewindStartSound;
        
        [Tooltip("Sound to play when rewind ends")]
        [SerializeField] private AudioClip rewindEndSound;
        
        [Header("UI")]
        [Tooltip("UI element to show rewind progress (optional)")]
        [SerializeField] private UnityEngine.UI.Image progressBar;
        
        [Tooltip("UI element to show during rewind (optional)")]
        [SerializeField] private GameObject rewindIndicator;
        
        // Post-processing components
        private ColorAdjustments _colorAdjustments;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;
        
        // Original values
        private float _originalSaturation;
        private float _originalChromaticAberration;
        private float _originalVignetteIntensity;
        
        // Current effect values
        private float _currentEffectWeight;
        private bool _isRewinding;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Initialize post-processing if volume is assigned
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                InitializePostProcessing();
            }
            
            // Hide UI elements initially
            if (rewindIndicator != null)
                rewindIndicator.SetActive(false);
            
            if (progressBar != null)
                progressBar.fillAmount = 0f;
        }
        
        private void OnEnable()
        {
            // Subscribe to rewind events
            if (TimeRewindManager.Instance != null)
            {
                TimeRewindManager.Instance.OnRewindStart += HandleRewindStart;
                TimeRewindManager.Instance.OnRewindStop += HandleRewindStop;
                TimeRewindManager.Instance.OnRewindProgress += HandleRewindProgress;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from rewind events
            if (TimeRewindManager.Instance != null)
            {
                TimeRewindManager.Instance.OnRewindStart -= HandleRewindStart;
                TimeRewindManager.Instance.OnRewindStop -= HandleRewindStop;
                TimeRewindManager.Instance.OnRewindProgress -= HandleRewindProgress;
            }
        }
        
        private void Update()
        {
            UpdateEffects();
        }
        
        #endregion

        #region Initialization
        
        private void InitializePostProcessing()
        {
            var profile = postProcessVolume.profile;
            
            // Get or add color adjustments
            if (profile.TryGet(out _colorAdjustments))
            {
                _originalSaturation = _colorAdjustments.saturation.value;
            }
            
            // Get or add chromatic aberration
            if (profile.TryGet(out _chromaticAberration))
            {
                _originalChromaticAberration = _chromaticAberration.intensity.value;
            }
            
            // Get or add vignette
            if (profile.TryGet(out _vignette))
            {
                _originalVignetteIntensity = _vignette.intensity.value;
            }
        }
        
        #endregion

        #region Event Handlers
        
        private void HandleRewindStart()
        {
            _isRewinding = true;
            
            // Play start sound
            if (audioSource != null && rewindStartSound != null)
            {
                audioSource.PlayOneShot(rewindStartSound);
            }
            
            // Start loop sound
            if (audioSource != null && rewindLoopSound != null)
            {
                audioSource.clip = rewindLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            // Show UI
            if (rewindIndicator != null)
                rewindIndicator.SetActive(true);
        }
        
        private void HandleRewindStop()
        {
            _isRewinding = false;
            
            // Stop loop sound
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == rewindLoopSound)
            {
                audioSource.Stop();
            }
            
            // Play end sound
            if (audioSource != null && rewindEndSound != null)
            {
                audioSource.PlayOneShot(rewindEndSound);
            }
            
            // Hide UI
            if (rewindIndicator != null)
                rewindIndicator.SetActive(false);
        }
        
        private void HandleRewindProgress(float progress)
        {
            // Update progress bar
            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }
        }
        
        #endregion

        #region Effect Updates
        
        private void UpdateEffects()
        {
            // Smoothly transition effect weight
            float targetWeight = _isRewinding ? 1f : 0f;
            _currentEffectWeight = Mathf.MoveTowards(
                _currentEffectWeight, 
                targetWeight, 
                effectTransitionSpeed * Time.unscaledDeltaTime
            );
            
            // Apply post-processing effects
            ApplyPostProcessingEffects();
        }
        
        private void ApplyPostProcessingEffects()
        {
            if (_colorAdjustments != null)
            {
                _colorAdjustments.saturation.value = Mathf.Lerp(
                    _originalSaturation, 
                    rewindSaturation, 
                    _currentEffectWeight
                );
            }
            
            if (_chromaticAberration != null)
            {
                _chromaticAberration.intensity.value = Mathf.Lerp(
                    _originalChromaticAberration, 
                    rewindChromaticAberration, 
                    _currentEffectWeight
                );
            }
            
            if (_vignette != null)
            {
                _vignette.intensity.value = Mathf.Lerp(
                    _originalVignetteIntensity, 
                    rewindVignetteIntensity, 
                    _currentEffectWeight
                );
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Manually set the post-processing volume at runtime
        /// </summary>
        public void SetPostProcessVolume(Volume volume)
        {
            postProcessVolume = volume;
            if (volume != null && volume.profile != null)
            {
                InitializePostProcessing();
            }
        }
        
        /// <summary>
        /// Manually set the audio source at runtime
        /// </summary>
        public void SetAudioSource(AudioSource source)
        {
            audioSource = source;
        }
        
        #endregion
    }
}
