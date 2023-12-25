#if ARGPS_USE_VUFORIA
using Logger = ARLocation.Utils.Logger;
using System;
using Vuforia;

namespace ARLocation.Session
{
    public class VuforiaSessionManager : IARSessionManager
    {
        private string sessionInfoString;
        private bool trackingStarted;
        private Action trackingStartedCallback;
        private Action onAfterReset;
        private Action trackingRestoredCallback;
        private Action trackingLostCallback;

        public bool DebugMode { get; set; }

        public VuforiaSessionManager()
        {
            VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
            VuforiaApplication.Instance.OnVuforiaPaused += OnVuforiaPaused;
            VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged += OnDevicePoseStatusChanged;
        }

        private void OnDevicePoseStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            sessionInfoString = $"OnDevicePoseStatusChanged: {targetStatus.Status}";

            Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", sessionInfoString, DebugMode);

            // if (arg1 != / TrackableBehaviour.Status.NO_POSE)
            if (targetStatus.Status != Status.NO_POSE)
            {
                if (!trackingStarted)
                {
                    trackingStarted = true;
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Started!.", DebugMode);
                    trackingStartedCallback?.Invoke();
                }
                else if (targetStatus.Status == Status.NO_POSE)
                {
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Restored!", DebugMode);
                    trackingRestoredCallback?.Invoke();
                }

                if (onAfterReset != null)
                {
                    Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Emitting 'OnAfterReset' event.", DebugMode);
                    onAfterReset.Invoke();
                    onAfterReset = null;
                }
            }
            else if (targetStatus.Status != Status.NO_POSE)
            {
                Logger.LogFromMethod("VuforiaSessionManager", "OnDevicePoseStatusChanged", "Tracking Lost!", DebugMode);
                trackingLostCallback?.Invoke();
            }
        }

        private void OnTrackerStarted()
        {
            sessionInfoString = $"OnTrackerStarted";
        }

        private void OnVuforiaPaused(bool obj)
        {
            sessionInfoString = $"OnVuforiaPaused";
        }

        private void OnVuforiaStarted()
        {
            sessionInfoString = $"OnVuforiaStarted";
        }


        public void Reset(Action callback)
        {
            VuforiaBehaviour.Instance.DevicePoseBehaviour.Reset();
            onAfterReset += callback;
        }

        public string GetSessionInfoString()
        {
            return sessionInfoString;
        }

        public string GetProviderString()
        {
            return "Vuforia (" + VuforiaApplication.GetVuforiaLibraryVersion() + ")";
        }

        public void OnARTrackingStarted(Action callback)
        {
            if (trackingStarted)
            {
                callback?.Invoke();
                return;
            }

            trackingStartedCallback += callback;
        }

        public void OnARTrackingRestored(Action callback)
        {
            trackingRestoredCallback += callback;
        }

        public void OnARTrackingLost(Action callback)
        {
            trackingLostCallback += callback;
        }
    }
}
#endif
