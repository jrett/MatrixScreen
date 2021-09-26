namespace MatrixScreen
{
    public interface IRenderable
    {
        void Update(long elapsedTime);
        void Draw(long elapsedTime);
    }
}
