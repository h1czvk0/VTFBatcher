namespace VTFBatcher.Enums;

public enum ImageFormatEnum
{
    RGBA8888,
    ABGR8888,
    RGB888,
    BGR888,
    RGB565,
    I8,
    IA88,
    A8,
    RGB888_BLUESCREEN,
    BGR888_BLUESCREEN,
    ARGB8888,
    BGRA8888,
    DXT1,
    DXT3,
    DXT5,
    BGRX8888,
    BGR565,
    BGRX5551,
    BGRA4444,
    DXT1_ONEBITALPHA,
    BGRA5551,
    UV88,
    UVWQ8888,
    RGBA16161616F,
    RGBA16161616,
    UVLX8888,
}

public enum ResizeMethodEnum
{
    Nearest,
    Biggest,
    Smallest,
}

/// <summary>
/// ResizeFilter和MipmapFilter的共用枚举
/// </summary>
public enum ResizeFilterEnum
{
    Point,
    Box,
    Triangle,
    Quadratic,
    Cubic,
    Catrom,
    Mitchell,
    Gaussian,
    Sinc,
    Bessel,
    Hanning,
    Hamming,
    Blackman,
    Kaiser,
}

/// <summary>
/// 锐化算法枚举
/// </summary>
public enum SharpenFilterEnum
{
    None,
    Negative,
    Lighter,
    Darker,
    ContrastMore,
    ContrastLess,
    Smoothen,
    SharpenSoft,
    SharpenMedium,
    SharpenStrong,
    FindEdges,
    Contour,
    EdgeDetect,
    EdgeDetectSoft,
    Emboss,
    MeanRemoval,
    Unsharp,
    XSharpen,
    WarpSharp,
}

public enum SizeEnum
{
    Size1 = 1,
    Size2 = 2,
    Size4 = 4,
    Size8 = 8,
    Size16 = 16,
    Size32 = 32,
    Size64 = 64,
    Size128 = 128,
    Size256 = 256,
    Size512 = 512,
    Size1024 = 1024,
    Size2048 = 2048,
    Size4096 = 4096,
}