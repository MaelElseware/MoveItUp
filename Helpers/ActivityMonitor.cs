using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Threading;

namespace TriviaExercise.Helpers
{
    public class ActivityMonitor
    {
        // Windows API imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)]
            public uint cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        private DispatcherTimer _activityTimer;
        private DateTime _lastActivityTime;
        private uint _inactivityThresholdMinutes;
        private bool _wasInactive = false;
        private uint _lastRecordedIdleTime = 0;

        // Events
        public event Action UserBecameActive;
        public event Action<uint> UserBecameInactive; // passes minutes of inactivity
        public event Action<uint> UserStillInactive; // periodic update while inactive

        // Properties
        public bool IsUserActive { get; private set; } = true;
        public uint InactivityThresholdMinutes
        {
            get => _inactivityThresholdMinutes;
            set => _inactivityThresholdMinutes = Math.Max(1, value); // Minimum 1 minute
        }

        public bool IsMonitoring => _activityTimer?.IsEnabled == true;

        public ActivityMonitor(uint inactivityThresholdMinutes = 10)
        {
            InactivityThresholdMinutes = inactivityThresholdMinutes;
            _lastActivityTime = DateTime.Now;

            // Check activity every 10 seconds
            _activityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _activityTimer.Tick += CheckUserActivity;
        }

        public void StartMonitoring()
        {
            if (_activityTimer.IsEnabled) return; // Already monitoring

            _activityTimer.Start();
            _lastActivityTime = DateTime.Now;
            IsUserActive = true;
            _wasInactive = false;
            _lastRecordedIdleTime = 0;

            // Test the API immediately
            TestGetLastInputInfo();

            Debug.WriteLine($"Activity monitoring started (threshold: {InactivityThresholdMinutes} minutes)");
        }

        public void StopMonitoring()
        {
            _activityTimer?.Stop();
            _wasInactive = false;
            IsUserActive = true;

            Debug.WriteLine("Activity monitoring stopped");
        }

        private void TestGetLastInputInfo()
        {
            uint idleTime = GetSystemIdleTime();
            uint idleSeconds = idleTime / 1000;
            uint idleMinutes = idleSeconds / 60;

            Debug.WriteLine($"Initial activity test - Idle time: {idleSeconds} seconds ({idleMinutes} minutes)");

            if (idleTime == 0)
            {
                Debug.WriteLine("WARNING: GetLastInputInfo may not be working correctly - always returning 0");
            }
        }

        private void CheckUserActivity(object sender, EventArgs e)
        {
            uint currentIdleTime = GetSystemIdleTime();
            uint idleTimeMinutes = currentIdleTime / 60000; // Convert to minutes
            uint idleTimeSeconds = currentIdleTime / 1000; // Convert to seconds

            bool wasActiveLastCheck = IsUserActive;

            IsUserActive = idleTimeMinutes < InactivityThresholdMinutes;

            // Debug output every check
            Debug.WriteLine($"Activity check - Idle: {idleTimeSeconds}s ({idleTimeMinutes}min), Threshold: {InactivityThresholdMinutes}min, Active: {IsUserActive}");

            // Check if idle time is actually changing (to detect if API is working)
            if (currentIdleTime == _lastRecordedIdleTime && currentIdleTime == 0)
            {
                Debug.WriteLine("WARNING: GetLastInputInfo consistently returning 0 - may not be working correctly");
            }
            _lastRecordedIdleTime = currentIdleTime;

            // User became inactive
            if (wasActiveLastCheck && !IsUserActive)
            {
                _wasInactive = true;
                UserBecameInactive?.Invoke(idleTimeMinutes);
                Debug.WriteLine($"🔴 User became INACTIVE (idle for {idleTimeMinutes} minutes)");
            }
            // User became active again after being inactive
            else if (!wasActiveLastCheck && IsUserActive && _wasInactive)
            {
                _lastActivityTime = DateTime.Now;
                _wasInactive = false;
                UserBecameActive?.Invoke();
                Debug.WriteLine($"🟢 User became ACTIVE again (was idle for {idleTimeMinutes} minutes)");
            }
            // User is still inactive - provide periodic updates
            else if (!IsUserActive && _wasInactive)
            {
                UserStillInactive?.Invoke(idleTimeMinutes);
                Debug.WriteLine($"🟡 User still inactive ({idleTimeMinutes} minutes)");
            }
        }

        private uint GetSystemIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)LASTINPUTINFO.SizeOf;
            lastInputInfo.dwTime = 0;

            bool success = GetLastInputInfo(ref lastInputInfo);

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"GetLastInputInfo failed with error: {error}");
                return 0; // If we can't get the info, assume user is active
            }

            uint currentTickCount = GetTickCount();
            uint idleTime = currentTickCount - lastInputInfo.dwTime;

            // Additional debugging
            Debug.WriteLine($"GetLastInputInfo - Success: {success}, CurrentTick: {currentTickCount}, LastInput: {lastInputInfo.dwTime}, IdleTime: {idleTime}ms");

            return idleTime;
        }

        public uint GetCurrentIdleTimeMinutes()
        {
            return GetSystemIdleTime() / 60000;
        }

        public uint GetCurrentIdleTimeSeconds()
        {
            return GetSystemIdleTime() / 1000;
        }

        public void UpdateThreshold(uint newThresholdMinutes)
        {
            InactivityThresholdMinutes = newThresholdMinutes;
            Debug.WriteLine($"Activity threshold updated to {newThresholdMinutes} minutes");
        }

        /// <summary>
        /// Test method to manually check if the API is working
        /// Call this method and then wait a few seconds without touching mouse/keyboard
        /// </summary>
        public void PerformManualTest()
        {
            for (int i = 0; i < 5; i++)
            {
                uint idleTime = GetSystemIdleTime();
                uint seconds = idleTime / 1000;
                Debug.WriteLine($"Manual test {i + 1}: Idle time = {seconds} seconds");
                System.Threading.Thread.Sleep(2000); // Wait 2 seconds between checks
            }
        }

        public void Dispose()
        {
            StopMonitoring();
            _activityTimer = null;
        }
    }

    /// <summary>
    /// Configuration for activity monitoring behavior
    /// </summary>
    public enum ActivityBehavior
    {
        Disabled,       // No activity monitoring
        PauseOnly,      // Just pause the timer while away
        PauseAndReset   // Pause timer and reset when returning after threshold
    }
}