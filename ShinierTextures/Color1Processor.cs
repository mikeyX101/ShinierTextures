using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace ShinierTextures;

public static class ImageColor1Extensions
{
    public static IImageProcessingContext ApplyColor1(this IImageProcessingContext ctx, Color color1)
    {
        return ctx.ApplyProcessor(new Color1Processor(color1));
    }

    private class Color1Processor : IImageProcessor
    {
        private Color Color1 { get; }
        
        public Color1Processor(Color color1)
        {
            Color1 = color1;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Color1Processor<TPixel>(configuration, source, sourceRectangle, Color1);
        }
    }
    
    private class Color1Processor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private Configuration Configuration { get; }
        private Image<TPixel> Source { get; }
        private Rectangle SourceRectangle { get; }
        
        private Color Color1 { get; }
        
        public Color1Processor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, Color color1)
        {
            Configuration = configuration;
            Source = source;
            SourceRectangle = sourceRectangle;

            Color1 = color1;
        }
        
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Execute()
        {
            Rgba32 color1 = Color1.ToPixel<Rgba32>();
            
            Source.Mutate(ctx =>
            {
                ctx.ProcessPixelRowsAsVector4(row =>
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        Vector4 pix = row[i];
                        Vector4 newPix = new()
                        {
                            X = RouteColorChannel(pix, color1.R),
                            Y = RouteColorChannel(pix, color1.G),
                            Z = RouteColorChannel(pix, color1.B),
                            W = RouteColorChannel(pix, color1.A)
                        };

                        row[i] = newPix;
                    }
                }, SourceRectangle, PixelConversionModifiers.Premultiply);
            });
        }

        private static float RouteColorChannel(Vector4 pix, byte target)
        {
            if (target == 0 % 4)
            {
                return pix.X;
            }
            else if (target == 1 % 4)
            {
                return pix.Y;
            }
            else if (target == 2 % 4)
            {
                return pix.Z;
            }
            else if (target == 3 % 4)
            {
                return pix.W;
            }
            throw new UnreachableException();
        }

        public void Dispose()
        {
            
        }
    }
}