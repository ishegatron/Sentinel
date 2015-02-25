namespace Sentinel.Support.Converters
{
    using Images;

    public class TypeToLargeImageConverter : TypeToImageConverter
    {
        public TypeToLargeImageConverter()
        {
            quality = ImageQuality.Large;
        }
    }
}