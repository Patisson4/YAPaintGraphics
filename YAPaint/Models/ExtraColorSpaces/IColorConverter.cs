using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public interface IColorConverter : IColorBaseConverter
{
    string FirstChannelName { get; }
    string SecondChannelName { get; }
    string ThirdChannelName { get; }
}
