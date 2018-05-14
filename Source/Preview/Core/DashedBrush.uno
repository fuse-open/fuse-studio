using Uno;
using Uno.Collections;
using Uno.Collections.EnumerableExtensions;
using Uno.Graphics;
using Uno.UX;
using Fuse.Drawing;
using Fuse;

public sealed class DashedSolidColor : DynamicBrush
{
	static Selector _colorName = "Color";
	static Selector _dashSizeName = "DashSize";

	float4 _color;
	[UXOriginSetter("SetColor")]
	public float4 Color
	{
		get { return _color; }
		set
		{
			if (_color != value)
			{
				_color = value;
				OnPropertyChanged(_colorName);
			}
		}
	}
	
	float _dashSize;
	public float DashSize
	{
		get { return _dashSize; }
		set
		{
			if(_dashSize != value)
			{
				_dashSize = value;
				OnPropertyChanged(_dashSizeName);
			}
		}
	}

	public void SetColor(float4 c, IPropertyListener origin)
	{
		if (_color != c)
		{
			_color = c;
			OnPropertyChanged(_colorName, origin);
		}
	}

	// Needed for data binding
	internal void SetColor(float4 c)
	{
		Color = c;
	}

	public override bool IsCompletelyTransparent { get { return base.IsCompletelyTransparent || Color.W == 0; } }

	static float Box(float2 p, float2 b)
	{
		float2 d = Math.Abs(p) - b;
		return Math.Min(Math.Max(d.X,d.Y),0.0f) + Vector.Length(Math.Max(d,0.0f));
	}

	static float2 Rep(float2 p, float2 c)
	{
		return Math.Mod(p,c)-0.5f*c;
	}

	float2 p: req(TexCoord as float2) pixel TexCoord.XY * CanvasSize.XY;
	float t:
	{
		float2 nRep = CanvasSize.XY / DashSize;
		int2 iRep = (int2)nRep;
		if(Math.Mod(iRep.X,2) == 0)
			++iRep.X;			
		if(Math.Mod(iRep.Y,2) == 0)
			++iRep.Y;
				
		nRep = CanvasSize.XY / (float2)iRep;
		float t = Box(Rep(p,nRep*2)+float2(DashSize),float2(DashSize));
		return -Math.Floor(t);
	};
	FinalColor: Math.Lerp(float4(0), Color, Math.Clamp(t,0.f,1.f));

	public DashedSolidColor()
	{
		_color = float4(1);
	}

	public DashedSolidColor(float4 color)
	{
		_color = color;
	}
}