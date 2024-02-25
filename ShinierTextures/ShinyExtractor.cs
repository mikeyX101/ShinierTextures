

using SixLabors.ImageSharp;

namespace ShinierTextures;

public sealed class ShinyExtractor: IDisposable, IAsyncDisposable
{
    private enum Game : byte
    {
        Undefined = 0,
        PokemonColosseum = 1,
        PokemonXDGaleOfDarkness = 2,
        PokemonBattleRevolution = 3
    }
    
    private FileStream PkxStream { get; }
    
    private Game DetectedGame { get; }
    private long InitialOffset { get; }

    public ShinyExtractor(string pkxFile)
    {
        PkxStream = File.OpenRead(pkxFile);
        DetectedGame = DetectGame();
        InitialOffset = DetectedGame switch
        {
            Game.PokemonColosseum => PkxStream.Length - 0x11,
            Game.PokemonXDGaleOfDarkness => 0x73,
            Game.PokemonBattleRevolution or Game.Undefined or _ => throw new ArgumentOutOfRangeException(nameof(pkxFile),"File does not contain color.")
        };
    }

    private Game DetectGame()
    {
        // Detect where header ends
        PkxStream.Seek(0, SeekOrigin.Begin);
        byte firstByte = (byte)PkxStream.ReadByte();
        
        PkxStream.Seek(0x40, SeekOrigin.Begin);
        byte datByte = (byte)PkxStream.ReadByte();

        return firstByte == datByte ? Game.PokemonColosseum : Game.PokemonXDGaleOfDarkness;
    }
    
    public Color[]? GetColors()
    {
        if (DetectedGame is Game.Undefined or Game.PokemonBattleRevolution)
        {
            return null;
        }
        
        PkxStream.Seek(InitialOffset, SeekOrigin.Begin);
        byte red1 = (byte)PkxStream.ReadByte();
        PkxStream.Seek(3, SeekOrigin.Current);
        byte green1 = (byte)PkxStream.ReadByte();
        PkxStream.Seek(3, SeekOrigin.Current);
        byte blue1 = (byte)PkxStream.ReadByte();
        PkxStream.Seek(3, SeekOrigin.Current);
        byte alpha1 = (byte)PkxStream.ReadByte();

        byte red2;
        byte green2;
        byte blue2;
        byte alpha2;
        switch (DetectedGame)
        {
            case Game.PokemonColosseum:
                alpha2 = (byte)PkxStream.ReadByte();
                blue2 = (byte)PkxStream.ReadByte();
                green2 = (byte)PkxStream.ReadByte();
                red2 = (byte)PkxStream.ReadByte();
                break;
            case Game.PokemonXDGaleOfDarkness:
                red2 = (byte)PkxStream.ReadByte();
                green2 = (byte)PkxStream.ReadByte();
                blue2 = (byte)PkxStream.ReadByte();
                alpha2 = (byte)PkxStream.ReadByte();
                break;
            case Game.Undefined:
            case Game.PokemonBattleRevolution:
            default:
                throw new Exception("This shouldn't have happened.");
        }

        return
        [
            Color.FromRgba(red1, green1, blue1, alpha1),
            Color.FromRgba(red2, green2, blue2, alpha2)
        ];
    }

    public void Dispose()
    {
        PkxStream.Dispose();
    }
    
    public async ValueTask DisposeAsync()
    {
        await PkxStream.DisposeAsync();
    }
}