using System.Runtime.InteropServices;

namespace DeadworksManaged.Api.Sounds;

[StructLayout(LayoutKind.Sequential)]
internal struct SoundEventField
{
	public SosFieldType Type;
	private ulong _lo;
	private uint _hi;

	public static SoundEventField FromBool(bool value)
	{
		var f = new SoundEventField { Type = SosFieldType.Bool };
		f._lo = value ? 1ul : 0ul;
		return f;
	}

	public static SoundEventField FromInt32(int value)
	{
		var f = new SoundEventField { Type = SosFieldType.Int32 };
		f._lo = unchecked((uint)value);
		return f;
	}

	public static SoundEventField FromUInt32(uint value)
	{
		var f = new SoundEventField { Type = SosFieldType.UInt32 };
		f._lo = value;
		return f;
	}

	public static SoundEventField FromUInt64(ulong value)
	{
		var f = new SoundEventField { Type = SosFieldType.UInt64 };
		f._lo = value;
		return f;
	}

	public static SoundEventField FromFloat(float value)
	{
		var f = new SoundEventField { Type = SosFieldType.Float };
		f._lo = BitConverter.SingleToUInt32Bits(value);
		return f;
	}

	public static SoundEventField FromFloat3(float x, float y, float z)
	{
		var f = new SoundEventField { Type = SosFieldType.Float3 };
		uint xb = BitConverter.SingleToUInt32Bits(x);
		uint yb = BitConverter.SingleToUInt32Bits(y);
		uint zb = BitConverter.SingleToUInt32Bits(z);
		f._lo = xb | ((ulong)yb << 32);
		f._hi = zb;
		return f;
	}

	public bool AsBool => (_lo & 1) != 0;
	public int AsInt32 => unchecked((int)(uint)_lo);
	public uint AsUInt32 => (uint)_lo;
	public ulong AsUInt64 => _lo;
	public float AsFloat => BitConverter.UInt32BitsToSingle((uint)_lo);

	public (float X, float Y, float Z) AsFloat3 => (
		BitConverter.UInt32BitsToSingle((uint)_lo),
		BitConverter.UInt32BitsToSingle((uint)(_lo >> 32)),
		BitConverter.UInt32BitsToSingle(_hi)
	);

	public int PayloadSize => Type switch
	{
		SosFieldType.Bool   => 1,
		SosFieldType.Int32  => 4,
		SosFieldType.UInt32 => 4,
		SosFieldType.UInt64 => 8,
		SosFieldType.Float  => 4,
		SosFieldType.Float3 => 12,
		_ => 0,
	};

	public void WritePayload(Span<byte> dest)
	{
		switch (Type)
		{
			case SosFieldType.Bool:
				dest[0] = (byte)(AsBool ? 1 : 0);
				break;
			case SosFieldType.Int32:
			case SosFieldType.UInt32:
			case SosFieldType.Float:
				BitConverter.TryWriteBytes(dest, (uint)_lo);
				break;
			case SosFieldType.UInt64:
				BitConverter.TryWriteBytes(dest, _lo);
				break;
			case SosFieldType.Float3:
				BitConverter.TryWriteBytes(dest[..8], _lo);
				BitConverter.TryWriteBytes(dest.Slice(8, 4), _hi);
				break;
		}
	}
}
