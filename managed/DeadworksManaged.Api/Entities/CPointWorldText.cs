using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DeadworksManaged.Api;

/// <summary>Wraps the point_worldtext entity — a world-space text panel rendered in 3D.</summary>
[NativeClass("CPointWorldText")]
public sealed unsafe class CPointWorldText : CBaseEntity {
	internal CPointWorldText(nint handle) : base(handle) { }

	private static readonly SchemaAccessor<bool> _enabled = new("CPointWorldText"u8, "m_bEnabled"u8);
	private static readonly SchemaAccessor<bool> _fullbright = new("CPointWorldText"u8, "m_bFullbright"u8);
	private static readonly SchemaAccessor<float> _fontSize = new("CPointWorldText"u8, "m_flFontSize"u8);
	private static readonly SchemaAccessor<float> _worldUnitsPerPx = new("CPointWorldText"u8, "m_flWorldUnitsPerPx"u8);
	private static readonly SchemaAccessor<float> _depthOffset = new("CPointWorldText"u8, "m_flDepthOffset"u8);
	private static readonly SchemaAccessor<uint> _color = new("CPointWorldText"u8, "m_Color"u8);
	private static readonly SchemaAccessor<HorizontalJustify> _justifyH = new("CPointWorldText"u8, "m_nJustifyHorizontal"u8);
	private static readonly SchemaAccessor<VerticalJustify> _justifyV = new("CPointWorldText"u8, "m_nJustifyVertical"u8);
	public static readonly SchemaAccessor<byte> FontNameAccessor = new("CPointWorldText"u8, "m_FontName"u8);

	public bool Enabled { get => _enabled.Get(Handle); set => _enabled.Set(Handle, value); }
	public bool Fullbright { get => _fullbright.Get(Handle); set => _fullbright.Set(Handle, value); }
	public float FontSize { get => _fontSize.Get(Handle); set => _fontSize.Set(Handle, value); }
	public float WorldUnitsPerPx { get => _worldUnitsPerPx.Get(Handle); set => _worldUnitsPerPx.Set(Handle, value); }
	public float DepthOffset { get => _depthOffset.Get(Handle); set => _depthOffset.Set(Handle, value); }
	public HorizontalJustify JustifyHorizontal { get => _justifyH.Get(Handle); set => _justifyH.Set(Handle, value); }
	public VerticalJustify JustifyVertical { get => _justifyV.Get(Handle); set => _justifyV.Set(Handle, value); }
	public string FontName {
		get {
			byte* ptr = (byte*)FontNameAccessor.GetAddress(Handle);
			return System.Text.Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr));
		}
		set {
			byte* ptr = (byte*)FontNameAccessor.GetAddress(Handle);
			int max = 63;
			int written = System.Text.Encoding.UTF8.GetBytes(value.AsSpan(), new Span<byte>(ptr, max));
			ptr[written] = 0;
		}
	}

	/// <summary>Gets or sets the color as an ABGR uint32.</summary>
	public uint ColorABGR { get => _color.Get(Handle); set => _color.Set(Handle, value); }

	/// <summary>Sets the message text via entity input.</summary>
	public void SetMessage(string text) => AcceptInput("SetMessage", value: text);

	public void SetColor(Color color) => ColorABGR = ToABGR(color);
	public void SetColor(byte r, byte g, byte b, byte a = 255) => ColorABGR = ToABGR(r, g, b, a);

	private static uint ToABGR(Color c) => (uint)(c.A << 24 | c.B << 16 | c.G << 8 | c.R);
	private static uint ToABGR(byte r, byte g, byte b, byte a) => (uint)(a << 24 | b << 16 | g << 8 | r);

	/// <summary>Creates and spawns a CPointWorldText at the given position.</summary>
	public static CPointWorldText? Create(string message, Vector3 position, float fontSize = 100f, float? worldUnitsPerPx = null, string? fontName = null, byte r = 255, byte g = 255, byte b = 255, byte a = 255, int reorientMode = 0) {
		var baseEntity = CreateByName("point_worldtext");
		if (baseEntity == null) return null;

		var wt = new CPointWorldText(baseEntity.Handle);
		float wupp = worldUnitsPerPx ?? (0.25f / 1050f) * fontSize;

		void* ekv = NativeInterop.CreateEntityKeyValues();
		Span<byte> msgVal = Utf8.Encode(message, stackalloc byte[Utf8.Size(message)]);
		Span<byte> fontVal = fontName != null ? Utf8.Encode(fontName, stackalloc byte[Utf8.Size(fontName)]) : default;

		fixed (byte* enabledKey = "enabled\0"u8.ToArray(),
		             fullbrightKey = "fullbright\0"u8.ToArray(),
		             fontSizeKey = "font_size\0"u8.ToArray(),
		             wuppKey = "world_units_per_pixel\0"u8.ToArray(),
		             colorKey = "color\0"u8.ToArray(),
		             justifyHKey = "justify_horizontal\0"u8.ToArray(),
		             justifyVKey = "justify_vertical\0"u8.ToArray(),
		             reorientKey = "reorient_mode\0"u8.ToArray(),
		             fontNameKey = "font_name\0"u8.ToArray(),
		             msgValPtr = msgVal,
		             fontValPtr = fontVal,
		             msgTextKey = "message_text\0"u8.ToArray()) {
			NativeInterop.EKVSetString(ekv, msgTextKey, msgValPtr);
			NativeInterop.EKVSetBool(ekv, enabledKey, 1);
			NativeInterop.EKVSetInt(ekv, fullbrightKey, 1);
			NativeInterop.EKVSetFloat(ekv, fontSizeKey, fontSize);
			NativeInterop.EKVSetFloat(ekv, wuppKey, wupp);
			NativeInterop.EKVSetColor(ekv, colorKey, r, g, b, a);
			NativeInterop.EKVSetInt(ekv, justifyHKey, 0);
			NativeInterop.EKVSetInt(ekv, justifyVKey, 0);
			NativeInterop.EKVSetInt(ekv, reorientKey, reorientMode);
			if (fontValPtr != null)
				NativeInterop.EKVSetString(ekv, fontNameKey, fontValPtr);
		}

		wt.Teleport(position: position);
		wt.Spawn(ekv);

		wt.AcceptInput("SetMessage", value: message);
		wt.AcceptInput("Enable");

		return wt;
	}
}
