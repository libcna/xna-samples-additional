using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

public sealed class Canvas : IDisposable
{
    private readonly SpriteBatch batch;
    private readonly Texture2D pixel, font;
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.:/+()=?";
    // Original compact 5x7 diagnostic glyphs. No external font is required.
    private static readonly string[] Glyphs = {
        "01110/10001/10001/11111/10001/10001/10001", "11110/10001/10001/11110/10001/10001/11110",
        "01111/10000/10000/10000/10000/10000/01111", "11110/10001/10001/10001/10001/10001/11110",
        "11111/10000/10000/11110/10000/10000/11111", "11111/10000/10000/11110/10000/10000/10000",
        "01111/10000/10000/10111/10001/10001/01111", "10001/10001/10001/11111/10001/10001/10001",
        "11111/00100/00100/00100/00100/00100/11111", "00111/00010/00010/00010/10010/10010/01100",
        "10001/10010/10100/11000/10100/10010/10001", "10000/10000/10000/10000/10000/10000/11111",
        "10001/11011/10101/10101/10001/10001/10001", "10001/11001/10101/10011/10001/10001/10001",
        "01110/10001/10001/10001/10001/10001/01110", "11110/10001/10001/11110/10000/10000/10000",
        "01110/10001/10001/10001/10101/10010/01101", "11110/10001/10001/11110/10100/10010/10001",
        "01111/10000/10000/01110/00001/00001/11110", "11111/00100/00100/00100/00100/00100/00100",
        "10001/10001/10001/10001/10001/10001/01110", "10001/10001/10001/10001/10001/01010/00100",
        "10001/10001/10001/10101/10101/10101/01010", "10001/10001/01010/00100/01010/10001/10001",
        "10001/10001/01010/00100/00100/00100/00100", "11111/00001/00010/00100/01000/10000/11111",
        "01110/10001/10011/10101/11001/10001/01110", "00100/01100/00100/00100/00100/00100/01110",
        "01110/10001/00001/00010/00100/01000/11111", "11110/00001/00001/01110/00001/00001/11110",
        "00010/00110/01010/10010/11111/00010/00010", "11111/10000/10000/11110/00001/00001/11110",
        "01110/10000/10000/11110/10001/10001/01110", "11111/00001/00010/00100/01000/01000/01000",
        "01110/10001/10001/01110/10001/10001/01110", "01110/10001/10001/01111/00001/00001/01110",
        "00000/00000/00000/11111/00000/00000/00000", "00000/00000/00000/00000/00000/00110/00110",
        "00000/00110/00110/00000/00110/00110/00000", "00001/00010/00010/00100/01000/01000/10000",
        "00000/00100/00100/11111/00100/00100/00000", "00010/00100/01000/01000/01000/00100/00010",
        "01000/00100/00010/00010/00010/00100/01000", "00000/00000/11111/00000/11111/00000/00000",
        "01110/10001/00001/00010/00100/00000/00100"
    };
    public Canvas(GraphicsDevice device)
    {
        batch = new SpriteBatch(device);
        pixel = new Texture2D(device, 1, 1); pixel.SetData(new[] { Color.White });
        font = new Texture2D(device, 96, 24); var data = new Color[96 * 24];
        for (int i = 0; i < Glyphs.Length; i++)
        {
            string[] rows = Glyphs[i].Split('/');
            for (int y = 0; y < 7; y++) for (int x = 0; x < 5; x++)
                    if (rows[y][x] == '1') data[((i / 16) * 8 + y) * 96 + (i % 16) * 6 + x] = Color.White;
        }
        font.SetData(data);
    }
    public void Begin(RasterizerState? rasterizer = null, DepthStencilState? depth = null, Effect? effect = null) =>
        batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
            depth ?? DepthStencilState.None, rasterizer ?? RasterizerState.CullNone, effect);
    public void End() => batch.End();
    public void Box(Rectangle rectangle, Color color) => batch.Draw(pixel, rectangle, color);
    public void Image(Texture2D texture, Rectangle rectangle) => batch.Draw(texture, rectangle, Color.White);
    public void Text(string value, int x, int y, Color color, int scale = 2)
    {
        int start = x;
        foreach (char character in value.ToUpperInvariant())
        {
            if (character == '\n') { x = start; y += 9 * scale; continue; }
            int i = Characters.IndexOf(character);
            if (i >= 0) batch.Draw(font, new Rectangle(x, y, 5 * scale, 7 * scale),
                new Rectangle((i % 16) * 6, (i / 16) * 8, 5, 7), color);
            x += 6 * scale;
        }
    }
    public void Line(Vector2 from, Vector2 to, Color color, float width = 2)
    {
        Vector2 delta = to - from;
        batch.Draw(pixel, from, null, color, MathF.Atan2(delta.Y, delta.X), Vector2.Zero,
            new Vector2(delta.Length(), width), SpriteEffects.None, 0);
    }
    public void Dispose() { batch.Dispose(); pixel.Dispose(); font.Dispose(); }
}
