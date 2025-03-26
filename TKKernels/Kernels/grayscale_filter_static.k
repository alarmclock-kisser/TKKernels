__kernel void grayscale_filter_static(__global uchar* buffer, const int length) {
    int gid = get_global_id(0);

    int bytesPerPixel = 4;

    if (gid < length / bytesPerPixel) {
        int pixelOffset = gid * bytesPerPixel;

        uchar red = buffer[pixelOffset];
        uchar green = buffer[pixelOffset + 1];
        uchar blue = buffer[pixelOffset + 2];
        uchar alpha = buffer[pixelOffset + 3];

        float grayScaleFloat = 0.299f * red + 0.587f * green + 0.114f * blue;
        uchar grayScale = (uchar)grayScaleFloat;

        buffer[pixelOffset] = grayScale; 
        buffer[pixelOffset + 1] = grayScale;
        buffer[pixelOffset + 2] = grayScale;
        buffer[pixelOffset + 3] = alpha;
    }
}