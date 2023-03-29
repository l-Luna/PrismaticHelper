using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace PrismaticHelper.Entities;

public class Stencils{

	private static VirtualRenderTarget _MaskRenderTarget;
	private static VirtualRenderTarget _ObjectRenderTarget;

	public static readonly BlendState AlphaMaskBlendState = new(){
		ColorSourceBlend = Blend.Zero,
		ColorBlendFunction = BlendFunction.Add,
		ColorDestinationBlend = Blend.SourceColor,
		AlphaSourceBlend = Blend.Zero,
		AlphaBlendFunction = BlendFunction.Add,
		AlphaDestinationBlend = Blend.SourceColor
	};
	
	public static readonly BlendState AlphaMaskClearBlendState = new(){
		ColorSourceBlend = Blend.Zero,
		ColorBlendFunction = BlendFunction.Add,
		ColorDestinationBlend = Blend.SourceAlpha,
		AlphaSourceBlend = Blend.Zero,
		AlphaBlendFunction = BlendFunction.Add,
		AlphaDestinationBlend = Blend.SourceAlpha
	};
	
	public static VirtualRenderTarget MaskRenderTarget{
		get{
			return _MaskRenderTarget ??= VirtualContent.CreateRenderTarget("PrismaticHelper:mask", 320, 180);
		}
	}
	
	public static VirtualRenderTarget ObjectRenderTarget{
		get{
			return _ObjectRenderTarget ??= VirtualContent.CreateRenderTarget("PrismaticHelper:object", 320, 180);
		}
	}

	public static void Unload(){
		_MaskRenderTarget?.Dispose();
		_ObjectRenderTarget?.Dispose();
	}
}