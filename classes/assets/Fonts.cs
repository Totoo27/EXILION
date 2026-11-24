using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;
public sealed class Fonts
{

    public SpriteFont PixelArt { get; private set; }
    public SpriteFont PixelArtBig { get; private set; }
    public SpriteFont PixelArtSmall { get; private set; }
    public async Task Load(ContentManager content)
    {

        MainLoader.SetLoadingText("Fonts");

        PixelArt = content.Load<SpriteFont>("Fonts/PixelArt");
        PixelArtBig = content.Load<SpriteFont>("Fonts/PixelArtBig");
        PixelArtSmall = content.Load<SpriteFont>("Fonts/PixelArtSmall");

        MainLoader.addCompletedTask();

    }
}