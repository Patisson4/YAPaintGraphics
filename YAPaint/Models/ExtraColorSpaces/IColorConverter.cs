﻿using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

/// <summary>
/// Interface used only to distinguish three channeled ColorSpace converters from one channeled ones
/// </summary>
public interface IColorConverter : IColorBaseConverter { }