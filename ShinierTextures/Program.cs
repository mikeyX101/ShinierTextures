using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ShinierTextures;

public static class Program 
{
    public static int Main(string[] args)
    {
        string gameFilePath = args[0];
        string outputPath = args[1];

        if (string.IsNullOrWhiteSpace(gameFilePath) || !Directory.Exists(gameFilePath))
        {
            Console.Error.WriteLine("Invalid \"Game Files\" Path, it doesn't exist or is not a folder.");
            return -1;
        }
        if (string.IsNullOrWhiteSpace(outputPath) || !Directory.Exists(outputPath))
        {
            Console.Error.WriteLine("Invalid Output Path, it doesn't exist or is not a folder.");
            return -1;
        }

        foreach (string modelFolder in Directory.EnumerateDirectories(gameFilePath, "pkx_*", SearchOption.TopDirectoryOnly))
        {
            ProcessModelTextures(modelFolder, Path.Combine(outputPath, Path.GetFileName(modelFolder)));
        }

        return 0;
    }

    private static void ProcessModelTextures(string modelFolder, string outputFolder)
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] pkxModels = Directory.GetFiles(modelFolder, "*.pkx", SearchOption.TopDirectoryOnly);
        if (pkxModels.Length == 0)
        {
            Console.Error.WriteLine("No pkx models found for " + modelFolder + ". Aborting.");
            return;
        }
        else if (pkxModels.Length != 1)
        {
            Console.Error.WriteLine("Multiple pkx models found for " + modelFolder + ". Aborting.");
            return;
        }

        string pkxModel = pkxModels[0];
        using ShinyExtractor shinyExtractor = new ShinyExtractor(pkxModel);
        Color[]? colors = shinyExtractor.GetColors();
        if (colors is null || colors.Length != 2)
        {
            Console.Error.WriteLine("No colors found in model for " + modelFolder + ". Aborting.");
            return;
        }

        Console.WriteLine("Processing model " + Path.GetFileName(modelFolder));
        
        Color color1 = colors[0];
        Color color2 = colors[1];

        foreach (string texture in Directory.EnumerateFiles(modelFolder, "*.png", SearchOption.TopDirectoryOnly))
        {
            TextureToShinyTexture(texture, color1, color2, outputFolder);
        }
    }

    private static void TextureToShinyTexture(string texture, Color color1, Color color2, string outputTo)
    {
        string textureName = Path.GetFileName(texture);
        Console.WriteLine("Processing texture " + textureName);
        
        using Image textureImg = Image.Load(texture);
        textureImg.Mutate(ctx => ctx.ApplyColor1(color1).ApplyColor2(color2));
        textureImg.SaveAsPng(Path.Combine(outputTo, textureName));
    }
}