using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace ShinierTextures;

public static class ImageColor2Extensions
{
    public static IImageProcessingContext ApplyColor2(this IImageProcessingContext ctx, Color color2)
    {
        return ctx.ApplyProcessor(new Color2Processor(color2));
    }

    private class Color2Processor : IImageProcessor
    {
        private Color Color2 { get; }
        
        public Color2Processor(Color color2)
        {
            Color2 = color2;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Color2Processor<TPixel>(configuration, source, sourceRectangle, Color2);
        }
    }
    
    private class Color2Processor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private Configuration Configuration { get; }
        private Image<TPixel> Source { get; }
        private Rectangle SourceRectangle { get; }
        
        private Color Color2 { get; }
        
        public Color2Processor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, Color color2)
        {
            Configuration = configuration;
            Source = source;
            SourceRectangle = sourceRectangle;

            Color2 = color2;
        }
        
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Execute()
        {
            Rgba32 color2 = Color2.ToPixel<Rgba32>();

            Source.Mutate(ctx =>
            {
                ctx.ChannelBrightness(Utils.ConvertRange(0, 255, 0, 2, color2.R), ColorChannel.R);
                ctx.ChannelBrightness(Utils.ConvertRange(0, 255, 0, 2, color2.G), ColorChannel.G);
                ctx.ChannelBrightness(Utils.ConvertRange(0, 255, 0, 2, color2.B), ColorChannel.B);
                ctx.ChannelBrightness(Utils.ConvertRange(0, 255, 0, 2, color2.A), ColorChannel.A);
            });
        }

        public void Dispose()
        {
            
        }
    }
}

public enum ColorChannel
{
    R,G,B,A
}

public static class ImageChannelBrightnessExtensions
{
    public static IImageProcessingContext ChannelBrightness(this IImageProcessingContext ctx, float amount, ColorChannel colorChannel)
    {
        ColorMatrix colorMatrix = new ColorMatrix
        {
            M11 = 1F,
            M22 = 1F,
            M33 = 1F,
            M44 = 1F
        };
        if (colorChannel == ColorChannel.R)
        {
            colorMatrix.M11 = amount;
        }
        else if (colorChannel == ColorChannel.G)
        {
            colorMatrix.M22 = amount;
        }
        else if (colorChannel == ColorChannel.B)
        {
            colorMatrix.M33 = amount;
        }
        else if (colorChannel == ColorChannel.A)
        {
            colorMatrix.M44 = amount;
        }
        
        return ctx.ApplyProcessor(new ChannelBrightnessProcessor(colorMatrix));
    }

    private class ChannelBrightnessProcessor(ColorMatrix colorMatrix) : FilterProcessor(colorMatrix);
}