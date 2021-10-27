/*
 *  Managed C# wrapper for GameNetworkingSockets library by Valve Software
 *  Copyright (c) 2018 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Valve.Sockets {
	using ListenSocket = UInt32;
	using Connection = UInt32;
	using PollGroup = UInt32;
	using Microseconds = Int64;

	[Flags]
	public enum SendFlags {
		Unreliable = 0,
		NoNagle = 1 << 0,
		NoDelay = 1 << 2,
		Reliable = 1 << 3
	}

	public enum IdentityType {
		Invalid = 0,
		SteamID = 16,
		IPAddress = 1,
		GenericString = 2,
		GenericBytes = 3
	}

	public enum ConnectionState {
		None = 0,
		Connecting = 1,
		FindingRoute = 2,
		Connected = 3,
		ClosedByPeer = 4,
		ProblemDetectedLocally = 5
	}

	public enum ConfigurationScope {
		Global = 1,
		SocketsInterface = 2,
		ListenSocket = 3,
		Connection = 4
	}

	public enum ConfigurationDataType {
		Int32 = 1,
		Int64 = 2,
		Float = 3,
		String = 4,
		FunctionPtr = 5
	}

	public enum ConfigurationValue {
		Invalid = 0,
		FakePacketLossSend = 2,
		FakePacketLossRecv = 3,
		FakePacketLagSend = 4,
		FakePacketLagRecv = 5,
		FakePacketReorderSend = 6,
		FakePacketReorderRecv = 7,
		FakePacketReorderTime = 8,
		FakePacketDupSend = 26,
		FakePacketDupRecv = 27,
		FakePacketDupTimeMax = 28,
		TimeoutInitial = 24,
		TimeoutConnected = 25,
		SendBufferSize = 9,
		SendRateMin = 10,
		SendRateMax = 11,
		NagleTime = 12,
		IPAllowWithoutAuth = 23,
		MTUPacketSize = 32,
		MTUDataSize = 33,
		Unencrypted = 34,
		EnumerateDevVars = 35,
		SymmetricConnect = 37,
		LocalVirtualPort = 38,
		ConnectionStatusChanged = 201,
		AuthStatusChanged = 202,
		RelayNetworkStatusChanged = 203,
		MessagesSessionRequest = 204,
		MessagesSessionFailed = 205,
		P2PSTUNServerList = 103,
		P2PTransportICEEnable = 104,
		P2PTransportICEPenalty = 105,
		P2PTransportSDRPenalty = 106,
		SDRClientConsecutitivePingTimeoutsFailInitial = 19,
		SDRClientConsecutitivePingTimeoutsFail = 20,
		SDRClientMinPingsBeforePingAccurate = 21,
		SDRClientSingleSocket = 22,
		SDRClientForceRelayCluster = 29,
		SDRClientDebugTicketAddress = 30,
		SDRClientForceProxyAddr = 31,
		SDRClientFakeClusterPing = 36,
		LogLevelAckRTT = 13,
		LogLevelPacketDecode = 14,
		LogLevelMessage = 15,
		LogLevelPacketGaps = 16,
		LogLevelP2PRendezvous = 17,
		LogLevelSDRRelayPings = 18
	}

	public enum ConfigurationValueResult {
		BadValue = -1,
		BadScopeObject = -2,
		BufferTooSmall = -3,
		OK = 1,
		OKInherited = 2
	}

	public enum DebugType {
		None = 0,
		Bug = 1,
		Error = 2,
		Important = 3,
		Warning = 4,
		Message = 5,
		Verbose = 6,
		Debug = 7,
		Everything = 8
	}

	public enum Result {
		OK = 1,
		Fail = 2,
		NoConnection = 3,
		InvalidPassword = 5,
		LoggedInElsewhere = 6,
		InvalidProtocolVer = 7,
		InvalidParam = 8,
		FileNotFound = 9,
		Busy = 10,
		InvalidState = 11,
		InvalidName = 12,
		InvalidEmail = 13,
		DuplicateName = 14,
		AccessDenied = 15,
		Timeout = 16,
		Banned = 17,
		AccountNotFound = 18,
		InvalidSteamID = 19,
		ServiceUnavailable = 20,
		NotLoggedOn = 21,
		Pending = 22,
		EncryptionFailure = 23,
		InsufficientPrivilege = 24,
		LimitExceeded = 25,
		Revoked = 26,
		Expired = 27,
		AlreadyRedeemed = 28,
		DuplicateRequest = 29,
		AlreadyOwned = 30,
		IPNotFound = 31,
		PersistFailed = 32,
		LockingFailed = 33,
		LogonSessionReplaced = 34,
		ConnectFailed = 35,
		HandshakeFailed = 36,
		IOFailure = 37,
		RemoteDisconnect = 38,
		ShoppingCartNotFound = 39,
		Blocked = 40,
		Ignored = 41,
		NoMatch = 42,
		AccountDisabled = 43,
		ServiceReadOnly = 44,
		AccountNotFeatured = 45,
		AdministratorOK = 46,
		ContentVersion = 47,
		TryAnotherCM = 48,
		PasswordRequiredToKickSession = 49,
		AlreadyLoggedInElsewhere = 50,
		Suspended = 51,
		Cancelled = 52,
		DataCorruption = 53,
		DiskFull = 54,
		RemoteCallFailed = 55,
		PasswordUnset = 56,
		ExternalAccountUnlinked = 57,
		PSNTicketInvalid = 58,
		ExternalAccountAlreadyLinked = 59,
		RemoteFileConflict = 60,
		IllegalPassword = 61,
		SameAsPreviousValue = 62,
		AccountLogonDenied = 63,
		CannotUseOldPassword = 64,
		InvalidLoginAuthCode = 65,
		AccountLogonDeniedNoMail = 66,
		HardwareNotCapableOfIPT = 67,
		IPTInitError = 68,
		ParentalControlRestricted = 69,
		FacebookQueryError = 70,
		ExpiredLoginAuthCode = 71,
		IPLoginRestrictionFailed = 72,
		AccountLockedDown = 73,
		AccountLogonDeniedVerifiedEmailRequired = 74,
		NoMatchingURL = 75,
		BadResponse = 76,
		RequirePasswordReEntry = 77,
		ValueOutOfRange = 78,
		UnexpectedError = 79,
		Disabled = 80,
		InvalidCEGSubmission = 81,
		RestrictedDevice = 82,
		RegionLocked = 83,
		RateLimitExceeded = 84,
		AccountLoginDeniedNeedTwoFactor = 85,
		ItemDeleted = 86,
		AccountLoginDeniedThrottle = 87,
		TwoFactorCodeMismatch = 88,
		TwoFactorActivationCodeMismatch = 89,
		AccountAssociatedToMultiplePartners = 90,
		NotModified = 91,
		NoMobileDevice = 92,
		TimeNotSynced = 93,
		SmsCodeFailed = 94,
		AccountLimitExceeded = 95,
		AccountActivityLimitExceeded = 96,
		PhoneActivityLimitExceeded = 97,
		RefundToWallet = 98,
		EmailSendFailure = 99,
		NotSettled = 100,
		NeedCaptcha = 101,
		GSLTDenied = 102,
		GSOwnerDenied = 103,
		InvalidItemType = 104,
		IPBanned = 105,
		GSLTExpired = 106,
		InsufficientFunds = 107,
		TooManyPending = 108,
		NoSiteLicensesFound = 109,
		WGNetworkSendExceeded = 110
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Address : IEquatable<Address> {
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] ip;
		public ushort port;

		public bool IsLocalHost {
			get {
				return Native.SteamAPI_SteamNetworkingIPAddr_IsLocalHost(ref this);
			}
		}

		public string GetIP() {
			return ip.ParseIP();
		}

		public void SetLocalHost(ushort port) {
			Native.SteamAPI_SteamNetworkingIPAddr_SetIPv6LocalHost(ref this, port);
		}

		public void SetAddress(string ip, ushort port) {
			if (!ip.Contains(":"))
				Native.SteamAPI_SteamNetworkingIPAddr_SetIPv4(ref this, ip.ParseIPv4(), port);
			else
				Native.SteamAPI_SteamNetworkingIPAddr_SetIPv6(ref this, ip.ParseIPv6(), port);
		}

		public bool Equals(Address other) {
			return Native.SteamAPI_SteamNetworkingIPAddr_IsEqualTo(ref this, ref other);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Configuration {
		public ConfigurationValue value;
		public ConfigurationDataType dataType;
		public ConfigurationData data;

		[StructLayout(LayoutKind.Explicit)]
		public struct ConfigurationData {
			[FieldOffset(0)]
			public int Int32;
			[FieldOffset(0)]
			public long Int64;
			[FieldOffset(0)]
			public float Float;
			[FieldOffset(0)]
			public IntPtr String;
			[FieldOffset(0)]
			public IntPtr FunctionPtr;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct StatusInfo {
		private const int callback = Library.socketsCallbacks + 1;
		public Connection connection;
		public ConnectionInfo connectionInfo;
		private ConnectionState oldState;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ConnectionInfo {
		public NetworkingIdentity identity;
		public long userData;
		public ListenSocket listenSocket;
		public Address address;
		private ushort pad;
		private uint popRemote;
		private uint popRelay;
		public ConnectionState state;
		public int endReason;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string endDebug;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string connectionDescription;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		private uint[] reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ConnectionStatus {
		public ConnectionState state;
		public int ping;
		public float connectionQualityLocal;
		public float connectionQualityRemote;
		public float outPacketsPerSecond;
		public float outBytesPerSecond;
		public float inPacketsPerSecond;
		public float inBytesPerSecond;
		public int sendRateBytesPerSecond;
		public int pendingUnreliable;
		public int pendingReliable;
		public int sentUnackedReliable;
		public Microseconds queueTime;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		private uint[] reserved;
	}

	[StructLayout(LayoutKind.Explicit, Size = 136)]
	public struct NetworkingIdentity {
		[FieldOffset(0)]
		public IdentityType type;

		public bool IsInvalid {
			get {
				return Native.SteamAPI_SteamNetworkingIdentity_IsInvalid(ref this);
			}
		}

		public ulong GetSteamID() {
			return Native.SteamAPI_SteamNetworkingIdentity_GetSteamID64(ref this);
		}

		public void SetSteamID(ulong steamID) {
			Native.SteamAPI_SteamNetworkingIdentity_SetSteamID64(ref this, steamID);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NetworkingMessage {
		public IntPtr data;
		public int length;
		public Connection connection;
		public NetworkingIdentity identity;
		public long connectionUserData;
		public Microseconds timeReceived;
		public long messageNumber;
		internal IntPtr freeData;
		internal IntPtr release;
		public int channel;
		public int flags;
		public long userData;

		public void CopyTo(byte[] destination) {
			if (destination == null)
				throw new ArgumentNullException("destination");

			Marshal.Copy(data, destination, 0, length);
		}

		#if !VALVESOCKETS_SPAN
			public void Destroy() {
				if (release == IntPtr.Zero)
					throw new InvalidOperationException("Message not created");

				Native.SteamAPI_SteamNetworkingMessage_t_Release(release);
			}
		#endif
	}

	public delegate void StatusCallback(ref StatusInfo info);
	public delegate void DebugCallback(DebugType type, string message);

	#if VALVESOCKETS_SPAN
		public delegate void MessageCallback(in NetworkingMessage message);
	#endif

	internal static class ArrayPool {
		[ThreadStatic]
		private static IntPtr[] pointerBuffer;

		public static IntPtr[] GetPointerBuffer() {
			if (pointerBuffer == null)
				pointerBuffer = new IntPtr[Library.maxMessagesPerBatch];

			return pointerBuffer;
		}
	}

	public class NetworkingSockets {
		private IntPtr nativeSockets;

		public NetworkingSockets() {
			nativeSockets = Native.SteamAPI_SteamNetworkingSockets_v009();

			if (nativeSockets == IntPtr.Zero)
				throw new InvalidOperationException("Networking sockets not created");
		}

		public ListenSocket CreateListenSocket(ref Address address) {
			return Native.SteamAPI_ISteamNetworkingSockets_CreateListenSocketIP(nativeSockets, ref address, 0, IntPtr.Zero);
		}

		public ListenSocket CreateListenSocket(ref Address address, Configuration[] configurations) {
			if (configurations == null)
				throw new ArgumentNullException("configurations");

			return Native.SteamAPI_ISteamNetworkingSockets_CreateListenSocketIP(nativeSockets, ref address, configurations.Length, configurations);
		}

		public Connection Connect(ref Address address) {
			return Native.SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(nativeSockets, ref address, 0, IntPtr.Zero);
		}

		public Connection Connect(ref Address address, Configuration[] configurations) {
			if (configurations == null)
				throw new ArgumentNullException("configurations");

			return Native.SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(nativeSockets, ref address, configurations.Length, configurations);
		}

		public Result AcceptConnection(Connection connection) {
			return Native.SteamAPI_ISteamNetworkingSockets_AcceptConnection(nativeSockets, connection);
		}

		public bool CloseConnection(Connection connection) {
			return CloseConnection(connection, 0, String.Empty, false);
		}

		public bool CloseConnection(Connection connection, int reason, string debug, bool enableLinger) {
			if (debug.Length > Library.maxCloseMessageLength)
				throw new ArgumentOutOfRangeException("debug");

			return Native.SteamAPI_ISteamNetworkingSockets_CloseConnection(nativeSockets, connection, reason, debug, enableLinger);
		}

		public bool CloseListenSocket(ListenSocket socket) {
			return Native.SteamAPI_ISteamNetworkingSockets_CloseListenSocket(nativeSockets, socket);
		}

		public bool SetConnectionUserData(Connection peer, long userData) {
			return Native.SteamAPI_ISteamNetworkingSockets_SetConnectionUserData(nativeSockets, peer, userData);
		}

		public long GetConnectionUserData(Connection peer) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetConnectionUserData(nativeSockets, peer);
		}

		public void SetConnectionName(Connection peer, string name) {
			Native.SteamAPI_ISteamNetworkingSockets_SetConnectionName(nativeSockets, peer, name);
		}

		public bool GetConnectionName(Connection peer, StringBuilder name, int maxLength) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetConnectionName(nativeSockets, peer, name, maxLength);
		}

		public Result SendMessageToConnection(Connection connection, IntPtr data, uint length) {
			return SendMessageToConnection(connection, data, length, SendFlags.Unreliable);
		}

		public Result SendMessageToConnection(Connection connection, IntPtr data, uint length, SendFlags flags) {
			return Native.SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(nativeSockets, connection, data, length, flags, IntPtr.Zero);
		}

		public Result SendMessageToConnection(Connection connection, IntPtr data, int length, SendFlags flags) {
			return SendMessageToConnection(connection, data, (uint)length, flags);
		}

		public Result SendMessageToConnection(Connection connection, byte[] data) {
			if (data == null)
				throw new ArgumentNullException("data");

			return SendMessageToConnection(connection, data, data.Length, SendFlags.Unreliable);
		}

		public Result SendMessageToConnection(Connection connection, byte[] data, SendFlags flags) {
			if (data == null)
				throw new ArgumentNullException("data");

			return SendMessageToConnection(connection, data, data.Length, flags);
		}

		public Result SendMessageToConnection(Connection connection, byte[] data, int length, SendFlags flags) {
			if (data == null)
				throw new ArgumentNullException("data");

			return Native.SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(nativeSockets, connection, data, (uint)length, flags, IntPtr.Zero);
		}

		public Result FlushMessagesOnConnection(Connection connection) {
			return Native.SteamAPI_ISteamNetworkingSockets_FlushMessagesOnConnection(nativeSockets, connection);
		}

		public bool GetConnectionInfo(Connection connection, ref ConnectionInfo info) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetConnectionInfo(nativeSockets, connection, ref info);
		}

		public bool GetQuickConnectionStatus(Connection connection, ref ConnectionStatus status) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetQuickConnectionStatus(nativeSockets, connection, ref status);
		}

		public int GetDetailedConnectionStatus(Connection connection, StringBuilder status, int statusLength) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetDetailedConnectionStatus(nativeSockets, connection, status, statusLength);
		}

		public bool GetListenSocketAddress(ListenSocket socket, ref Address address) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetListenSocketAddress(nativeSockets, socket, ref address);
		}

		public bool CreateSocketPair(Connection connectionLeft, Connection connectionRight, bool useNetworkLoopback, ref NetworkingIdentity identityLeft, ref NetworkingIdentity identityRight) {
			return Native.SteamAPI_ISteamNetworkingSockets_CreateSocketPair(nativeSockets, connectionLeft, connectionRight, useNetworkLoopback, ref identityLeft, ref identityRight);
		}

		public bool GetIdentity(ref NetworkingIdentity identity) {
			return Native.SteamAPI_ISteamNetworkingSockets_GetIdentity(nativeSockets, ref identity);
		}

		public PollGroup CreatePollGroup() {
			return Native.SteamAPI_ISteamNetworkingSockets_CreatePollGroup(nativeSockets);
		}

		public bool DestroyPollGroup(PollGroup pollGroup) {
			return Native.SteamAPI_ISteamNetworkingSockets_DestroyPollGroup(nativeSockets, pollGroup);
		}

		public bool SetConnectionPollGroup(PollGroup pollGroup, Connection connection) {
			return Native.SteamAPI_ISteamNetworkingSockets_SetConnectionPollGroup(nativeSockets, connection, pollGroup);
		}

		public void RunCallbacks() {
			Native.SteamAPI_ISteamNetworkingSockets_RunCallbacks(nativeSockets);
		}

		public void Poll(int msMaxWaitTime)
        {
			Native.SteamNetworkingSockets_Poll(msMaxWaitTime);
        }

		public void SetManualPollMode(bool bFlag)
        {
			Native.SteamNetworkingSockets_SetManualPollMode(bFlag);
        }

		#if VALVESOCKETS_SPAN
			[MethodImpl(256)]
			public void ReceiveMessagesOnConnection(Connection connection, MessageCallback callback, int maxMessages) {
				if (maxMessages > Library.maxMessagesPerBatch)
					throw new ArgumentOutOfRangeException("maxMessages");

				IntPtr[] nativeMessages = ArrayPool.GetPointerBuffer();
				int messagesCount = Native.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(nativeSockets, connection, nativeMessages, maxMessages);

				for (int i = 0; i < messagesCount; i++) {
					Span<NetworkingMessage> message;

					unsafe {
						message = new Span<NetworkingMessage>((void*)nativeMessages[i], 1);
					}

					callback(in message[0]);

					Native.SteamAPI_SteamNetworkingMessage_t_Release(nativeMessages[i]);
				}
			}

			[MethodImpl(256)]
			public void ReceiveMessagesOnPollGroup(PollGroup pollGroup, MessageCallback callback, int maxMessages) {
				if (maxMessages > Library.maxMessagesPerBatch)
					throw new ArgumentOutOfRangeException("maxMessages");

				IntPtr[] nativeMessages = ArrayPool.GetPointerBuffer();
				int messagesCount = Native.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup(nativeSockets, pollGroup, nativeMessages, maxMessages);

				for (int i = 0; i < messagesCount; i++) {
					Span<NetworkingMessage> message;

					unsafe {
						message = new Span<NetworkingMessage>((void*)nativeMessages[i], 1);
					}

					callback(in message[0]);

					Native.SteamAPI_SteamNetworkingMessage_t_Release(nativeMessages[i]);
				}
			}
		#else
			[MethodImpl(256)]
			public int ReceiveMessagesOnConnection(Connection connection, NetworkingMessage[] messages, int maxMessages) {
				if (messages == null)
					throw new ArgumentNullException("messages");

				if (maxMessages > Library.maxMessagesPerBatch)
					throw new ArgumentOutOfRangeException("maxMessages");

				IntPtr[] nativeMessages = ArrayPool.GetPointerBuffer();
				int messagesCount = Native.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(nativeSockets, connection, nativeMessages, maxMessages);

				for (int i = 0; i < messagesCount; i++) {
					messages[i] = (NetworkingMessage)Marshal.PtrToStructure(nativeMessages[i], typeof(NetworkingMessage));
					messages[i].release = nativeMessages[i];
				}

				return messagesCount;
			}

			[MethodImpl(256)]
			public int ReceiveMessagesOnPollGroup(PollGroup pollGroup, NetworkingMessage[] messages, int maxMessages) {
				if (messages == null)
					throw new ArgumentNullException("messages");

				if (maxMessages > Library.maxMessagesPerBatch)
					throw new ArgumentOutOfRangeException("maxMessages");

				IntPtr[] nativeMessages = ArrayPool.GetPointerBuffer();
				int messagesCount = Native.SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup(nativeSockets, pollGroup, nativeMessages, maxMessages);

				for (int i = 0; i < messagesCount; i++) {
					messages[i] = (NetworkingMessage)Marshal.PtrToStructure(nativeMessages[i], typeof(NetworkingMessage));
					messages[i].release = nativeMessages[i];
				}

				return messagesCount;
			}
		#endif
	}

	public class NetworkingUtils : IDisposable {
		private IntPtr nativeUtils;

		public NetworkingUtils() {
			nativeUtils = Native.SteamAPI_SteamNetworkingUtils_v003();

			if (nativeUtils == IntPtr.Zero)
				throw new InvalidOperationException("Networking utils not created");
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (nativeUtils != IntPtr.Zero) {
				Native.SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(nativeUtils, IntPtr.Zero);
				Native.SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(nativeUtils, DebugType.None, IntPtr.Zero);
				nativeUtils = IntPtr.Zero;
			}
		}

		~NetworkingUtils() {
			Dispose(false);
		}

		public Microseconds Time {
			get {
				return Native.SteamAPI_ISteamNetworkingUtils_GetLocalTimestamp(nativeUtils);
			}
		}

		public ConfigurationValue FirstConfigurationValue {
			get {
				return Native.SteamAPI_ISteamNetworkingUtils_GetFirstConfigValue(nativeUtils);
			}
		}

		public bool SetStatusCallback(StatusCallback callback) {
			return Native.SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(nativeUtils, callback);
		}

		public void SetDebugCallback(DebugType detailLevel, DebugCallback callback) {
			Native.SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(nativeUtils, detailLevel, callback);
		}

		public bool SetConfigurationValue(ConfigurationValue configurationValue, ConfigurationScope configurationScope, IntPtr scopeObject, ConfigurationDataType dataType, IntPtr value) {
			return Native.SteamAPI_ISteamNetworkingUtils_SetConfigValue(nativeUtils, configurationValue, configurationScope, scopeObject, dataType, value);
		}

		public bool SetConfigurationValue(Configuration configuration, ConfigurationScope configurationScope, IntPtr scopeObject) {
			return Native.SteamAPI_ISteamNetworkingUtils_SetConfigValueStruct(nativeUtils, configuration, configurationScope, scopeObject);
		}

		public ConfigurationValueResult GetConfigurationValue(ConfigurationValue configurationValue, ConfigurationScope configurationScope, IntPtr scopeObject, ref ConfigurationDataType dataType, ref IntPtr result, ref IntPtr resultLength) {
			return Native.SteamAPI_ISteamNetworkingUtils_GetConfigValue(nativeUtils, configurationValue, configurationScope, scopeObject, ref dataType, ref result, ref resultLength);
		}
	}

	public static class Extensions {
		public static uint ParseIPv4(this string ip) {
			IPAddress address = default(IPAddress);

			if (IPAddress.TryParse(ip, out address)) {
				if (address.AddressFamily != AddressFamily.InterNetwork)
					throw new Exception("Incorrect format of an IPv4 address");
			}

			byte[] bytes = address.GetAddressBytes();

			Array.Reverse(bytes);

			return BitConverter.ToUInt32(bytes, 0);
		}

		public static byte[] ParseIPv6(this string ip) {
			IPAddress address = default(IPAddress);

			if (IPAddress.TryParse(ip, out address)) {
				if (address.AddressFamily != AddressFamily.InterNetworkV6)
					throw new Exception("Incorrect format of an IPv6 address");
			}

			return address.GetAddressBytes();
		}

		public static string ParseIP(this byte[] ip) {
			IPAddress address = new IPAddress(ip);
			string converted = address.ToString();

			if (converted.Length > 7 && converted.Remove(7) == "::ffff:") {
				Address ipv4 = default(Address);

				ipv4.ip = ip;

				byte[] bytes = BitConverter.GetBytes(Native.SteamAPI_SteamNetworkingIPAddr_GetIPv4(ref ipv4));

				Array.Reverse(bytes);

				address = new IPAddress(bytes);
			}

			return address.ToString();
		}
	}

	public static class Library {
		public const int maxCloseMessageLength = 128;
		public const int maxErrorMessageLength = 1024;
		public const int maxMessagesPerBatch = 256;
		public const int maxMessageSize = 512 * 1024;
		public const int socketsCallbacks = 1220;
        private static bool initialized = false;

		public static bool Initialize() {
            if (initialized == true)
                return false;

			return Initialize(null);
		}

		public static bool Initialize(StringBuilder errorMessage) {

            if (initialized == true)
                return false;

            if (errorMessage != null && errorMessage.Capacity != maxErrorMessageLength)
				throw new ArgumentOutOfRangeException("Capacity of the error message must be equal to " + maxErrorMessageLength);

            initialized = Native.GameNetworkingSockets_Init(IntPtr.Zero, errorMessage);
            return initialized;
		}

		public static bool Initialize(ref NetworkingIdentity identity, StringBuilder errorMessage) {
            if (initialized == true)
                return false;

            if (errorMessage != null && errorMessage.Capacity != maxErrorMessageLength)
				throw new ArgumentOutOfRangeException("Capacity of the error message must be equal to " + maxErrorMessageLength);

			if (Object.Equals(identity, null))
				throw new ArgumentNullException("identity");

            initialized = Native.GameNetworkingSockets_Init(ref identity, errorMessage);
            return initialized;
        }

		public static void Deinitialize() {
            if (initialized)
            {
                initialized = false;
                Native.GameNetworkingSockets_Kill();
            }
		}
	}

	[SuppressUnmanagedCodeSecurity]
	internal static class Native {

        private const string nativeLibrary = "GameNetworkingSockets";

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool GameNetworkingSockets_Init(IntPtr identity, StringBuilder errorMessage);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool GameNetworkingSockets_Init(ref NetworkingIdentity identity, StringBuilder errorMessage);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void GameNetworkingSockets_Kill();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr SteamAPI_SteamNetworkingSockets_v009();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr SteamAPI_SteamNetworkingUtils_v003();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ListenSocket SteamAPI_ISteamNetworkingSockets_CreateListenSocketIP(IntPtr sockets, ref Address address, int configurationsCount, IntPtr configurations);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ListenSocket SteamAPI_ISteamNetworkingSockets_CreateListenSocketIP(IntPtr sockets, ref Address address, int configurationsCount, Configuration[] configurations);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Connection SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(IntPtr sockets, ref Address address, int configurationsCount, IntPtr configurations);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Connection SteamAPI_ISteamNetworkingSockets_ConnectByIPAddress(IntPtr sockets, ref Address address, int configurationsCount, Configuration[] configurations);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result SteamAPI_ISteamNetworkingSockets_AcceptConnection(IntPtr sockets, Connection connection);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_CloseConnection(IntPtr sockets, Connection peer, int reason, string debug, bool enableLinger);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_CloseListenSocket(IntPtr sockets, ListenSocket socket);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_SetConnectionUserData(IntPtr sockets, Connection peer, long userData);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long SteamAPI_ISteamNetworkingSockets_GetConnectionUserData(IntPtr sockets, Connection peer);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_ISteamNetworkingSockets_SetConnectionName(IntPtr sockets, Connection peer, string name);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_GetConnectionName(IntPtr sockets, Connection peer, StringBuilder name, int maxLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(IntPtr sockets, Connection connection, IntPtr data, uint length, SendFlags flags, IntPtr outMessageNumber);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result SteamAPI_ISteamNetworkingSockets_SendMessageToConnection(IntPtr sockets, Connection connection, byte[] data, uint length, SendFlags flags, IntPtr outMessageNumber);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Result SteamAPI_ISteamNetworkingSockets_FlushMessagesOnConnection(IntPtr sockets, Connection connection);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnConnection(IntPtr sockets, Connection connection, IntPtr[] messages, int maxMessages);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_GetConnectionInfo(IntPtr sockets, Connection connection, ref ConnectionInfo info);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_GetQuickConnectionStatus(IntPtr sockets, Connection connection, ref ConnectionStatus status);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int SteamAPI_ISteamNetworkingSockets_GetDetailedConnectionStatus(IntPtr sockets, Connection connection, StringBuilder status, int statusLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_GetListenSocketAddress(IntPtr sockets, ListenSocket socket, ref Address address);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_ISteamNetworkingSockets_RunCallbacks(IntPtr sockets);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_CreateSocketPair(IntPtr sockets, Connection connectionLeft, Connection connectionRight, bool useNetworkLoopback, ref NetworkingIdentity identityLeft, ref NetworkingIdentity identityRight);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_GetIdentity(IntPtr sockets, ref NetworkingIdentity identity);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern PollGroup SteamAPI_ISteamNetworkingSockets_CreatePollGroup(IntPtr sockets);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_DestroyPollGroup(IntPtr sockets, PollGroup pollGroup);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingSockets_SetConnectionPollGroup(IntPtr sockets, Connection connection, PollGroup pollGroup);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int SteamAPI_ISteamNetworkingSockets_ReceiveMessagesOnPollGroup(IntPtr sockets, PollGroup pollGroup, IntPtr[] messages, int maxMessages);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_SteamNetworkingIPAddr_SetIPv6(ref Address address, byte[] ip, ushort port);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_SteamNetworkingIPAddr_SetIPv4(ref Address address, uint ip, ushort port);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint SteamAPI_SteamNetworkingIPAddr_GetIPv4(ref Address address);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_SteamNetworkingIPAddr_SetIPv6LocalHost(ref Address address, ushort port);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_SteamNetworkingIPAddr_IsLocalHost(ref Address address);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_SteamNetworkingIPAddr_IsEqualTo(ref Address address, ref Address other);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_SteamNetworkingIdentity_IsInvalid(ref NetworkingIdentity identity);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_SteamNetworkingIdentity_SetSteamID64(ref NetworkingIdentity identity, ulong steamID);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ulong SteamAPI_SteamNetworkingIdentity_GetSteamID64(ref NetworkingIdentity identity);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Microseconds SteamAPI_ISteamNetworkingUtils_GetLocalTimestamp(IntPtr utils);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(IntPtr utils, IntPtr callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingUtils_SetGlobalCallback_SteamNetConnectionStatusChanged(IntPtr utils, StatusCallback callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(IntPtr utils, DebugType detailLevel, IntPtr callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_ISteamNetworkingUtils_SetDebugOutputFunction(IntPtr utils, DebugType detailLevel, DebugCallback callback);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingUtils_SetConfigValue(IntPtr utils, ConfigurationValue configurationValue, ConfigurationScope configurationScope, IntPtr scopeObject, ConfigurationDataType dataType, IntPtr value);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool SteamAPI_ISteamNetworkingUtils_SetConfigValueStruct(IntPtr utils, Configuration configuration, ConfigurationScope configurationScope, IntPtr scopeObject);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ConfigurationValueResult SteamAPI_ISteamNetworkingUtils_GetConfigValue(IntPtr utils, ConfigurationValue configurationValue, ConfigurationScope configurationScope, IntPtr scopeObject, ref ConfigurationDataType dataType, ref IntPtr result, ref IntPtr resultLength);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ConfigurationValue SteamAPI_ISteamNetworkingUtils_GetFirstConfigValue(IntPtr utils);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamAPI_SteamNetworkingMessage_t_Release(IntPtr nativeMessage);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamNetworkingSockets_Poll(int msMaxWaitTime);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SteamNetworkingSockets_SetManualPollMode(bool bFlag);
	}
}
