using Microsoft.Extensions.Logging;
using PicoFacialDataModule.Models;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using VRCFaceTracking;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PicoFacialDataModule
{
    enum PicoFacialDataPayload
    {
        FT_INFO_START,
        PXR_EYE_POSE_START = 380,
        PXR_EYE_POSE_END = PXR_EYE_POSE_START + 156
    }

    public class PicoFacialDataModule : ExtTrackingModule
    {
        private const int PORT = 9030;
        private const string MULTICAST_ADDRESS = "239.255.255.250";

        private const string DISCOVER_PAYLOAD = "DISCOVER_DAEMON";

        private const string PING = "MARCO";
        private const string REPLY = "POLO";

        private const string STOP = "STOP";

        private const int DISCOVER_INTERVAL_MS = 1000;

        private UdpClient? _udpClient;
        private IPEndPoint? _client;
        private bool _established;
        private volatile bool _stopping;

#pragma warning disable CS8618 // Because we didn't initialize in the constructor it is WHINING!
        private FaceTrackingParser _faceTrackingParser;
        private EyeTrackingParser _eyeTrackingParser;
        private ModuleSettings _moduleSettings;
#pragma warning restore CS8618

#if EYEDEBUG || FACEDEBUG
        private int _consolePixels;
#endif

        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            try
            {
                ModuleInformation.Name = "Pico 4 P/E Facial Tracking Daemon";
                ModuleInformation.Active = true;

                var stream = GetType().Assembly.GetManifestResourceStream("PicoFacialDataModule.Assets.icon.png");

                ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

                _udpClient = new UdpClient(PORT)
                {
                    EnableBroadcast = true,
                    MulticastLoopback = false,
                };

                _udpClient.Client.ReceiveTimeout = 2000;

                // Windows reports an ICMP "port unreachable" (a reply sent to a daemon session that
                // has already closed) as WSAECONNRESET on the next Receive, which then failed at once
                // and made the discovery loop spin without waiting. Ignore those reports.
                if (OperatingSystem.IsWindows())
                {
                    const int SIO_UDP_CONNRESET = -1744830452;
                    _udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                }

                _moduleSettings = SettingsManager.GetOrCreate();

                _faceTrackingParser = new FaceTrackingParser();
                _eyeTrackingParser = new EyeTrackingParser(_moduleSettings);

                return (!_moduleSettings.DisableEyeTracking, !_moduleSettings.DisableFaceTracking);
            } catch (Exception e)
            {
                Logger.LogCritical($"Initialization failed with the following message: {e.Message}\n Stacktrace:\n{e.StackTrace}");
                return (false, false);
            }
        }

        public override void Update()
        {
#if EYEDEBUG || FACEDEBUG
            var currentConsolePixels = Console.WindowWidth + Console.WindowHeight;
            if (_consolePixels != currentConsolePixels)
            {
                Console.Clear();
                _consolePixels = currentConsolePixels;
            }
#endif

            if (!ModuleInformation.Active)
            {
                Thread.Sleep(500);
                return;
            }

            try
            {
                if (!_established)
                {
                    byte[]? initialResult = Start();
                    if (initialResult == null)
                        return;

                    _established = true;
                    ProcessReply(initialResult);
                }

                byte[]? result = null;

                try
                {
                    IPEndPoint? receiver = null;
                    result = _udpClient!.Receive(ref receiver);
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                {
                    _established = false;
                }
                catch
                {
                    // Any other socket error returns at once: wait before discovering again.
                    _established = false;
                    Thread.Sleep(1000);
                }

                ProcessReply(result);

            } catch (Exception e)
            {
                Logger.LogCritical($"The module failed with the following exception: {e.Message}\n Stacktrace:\n{e.StackTrace}");

                // Good night!
                Thread.Sleep(Timeout.Infinite);
            }
        }

        public override void Teardown()
        {
            _stopping = true;

            if (_udpClient != null && _client != null)
                _udpClient.Send(Encoding.UTF8.GetBytes(STOP), _client);

            if (_udpClient != null)
                _udpClient.Dispose();
        }

        private void ProcessReply(byte[]? result)
        {
            if (result == null)
                return;

            // Keep-alive ping.
            if (result.Length == PING.Length + 1)
            {
                _udpClient!.Send(Encoding.UTF8.GetBytes(REPLY), _client);
                return;
            }

            if (result.Length < (int)PicoFacialDataPayload.PXR_EYE_POSE_END || result.Length > (int)PicoFacialDataPayload.PXR_EYE_POSE_END)
                return;

            if (!MemoryMarshal.TryRead<PicoFTInfo>(result![(int)PicoFacialDataPayload.FT_INFO_START..(int)PicoFacialDataPayload.PXR_EYE_POSE_START], out var picoFTInfo))
                return;

            if (!MemoryMarshal.TryRead<PxrEyePoseDataV2>(result![(int)PicoFacialDataPayload.PXR_EYE_POSE_START..], out var eyeData))
                return;

            if (!_moduleSettings!.DisableFaceTracking)
                _faceTrackingParser!.Parse(picoFTInfo);
           
            if (!_moduleSettings.DisableEyeTracking)
                _eyeTrackingParser!.Parse(eyeData, picoFTInfo);
        }

        /// <summary>
        /// Wakes up the peer daemon by sending a ping, and waiting for a reply.
        /// The daemon will shut down automatically once the UDP port gets disposed.
        /// </summary>
        /// <returns></returns>
        private byte[]? Start()
        {
            IPEndPoint endpoint = new IPEndPoint(
                string.IsNullOrEmpty(_moduleSettings.IP) ? IPAddress.Parse(MULTICAST_ADDRESS) : IPAddress.Parse(_moduleSettings.IP), 
                PORT
            );

            var discoverPayload = Encoding.UTF8.GetBytes(DISCOVER_PAYLOAD);

            byte[]? reply = null;

            // Get all network cards.
            var networkIPs = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            while (!_stopping)
            {
                // At most one discovery round per second, however fast Receive returns.
                var roundStart = Environment.TickCount64;

                foreach (var IP in networkIPs)
                {
                    _udpClient!.Client.SetSocketOption(
                        SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface,
                        IP.GetAddressBytes()
                     );
                    _udpClient.Send(discoverPayload, discoverPayload.Length, endpoint);
                }

                IPEndPoint? receiver = null;

                try
                {
                    reply = _udpClient!.Receive(ref receiver);
                }
                catch { }

                if (reply != null)
                {
                    _client = receiver!;
                    return reply;
                }

                var elapsed = Environment.TickCount64 - roundStart;
                if (elapsed < DISCOVER_INTERVAL_MS)
                    Thread.Sleep((int)(DISCOVER_INTERVAL_MS - elapsed));
            }

            return null;
        }
    }
}
